using System;
using System.Collections.Generic;
using System.Linq;

// 상점 상품 구매, 일일 제한, 출석 보상 수령을 처리함
public sealed class ShopController
{
    private readonly ShopDatabaseSO database;
    private readonly Func<DateTime> utcNowProvider;
    private readonly Func<int> currentStageIdProvider;
    private readonly Func<int, bool> hasDefeatedStageProvider;
    private readonly HashSet<string> purchasedOnceProductIds = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> dailyPurchaseCounts = new(StringComparer.Ordinal);
    private readonly HashSet<int> claimedAttendanceRewardIndices = new();

    private string lastDailyResetDate;
    private string attendanceCycleStartDate;

    public event Action OnShopStateChanged;
    public event Action<ShopPurchaseResult> OnProductPurchased;
    public event Action<ShopAttendanceClaimResult> OnAttendanceClaimed;

    public IReadOnlyList<ShopProductSO> Products => database.Products;
    public IReadOnlyList<ShopRewardEntry> AttendanceRewards => database.AttendanceRewardDatabase.Rewards;

    // 데이터베이스와 테스트 가능한 UTC 시각 공급자를 연결함
    public ShopController(
        ShopDatabaseSO database,
        Func<DateTime> utcNowProvider = null,
        Func<int> currentStageIdProvider = null,
        Func<int, bool> hasDefeatedStageProvider = null)
    {
        this.database = database ?? throw new ArgumentNullException(nameof(database));
        this.utcNowProvider = utcNowProvider ?? (() => DateTime.UtcNow);
        this.currentStageIdProvider = currentStageIdProvider ?? (() => new StageProgressController().CurrentStageId);
        this.hasDefeatedStageProvider = hasDefeatedStageProvider ?? (stageId => new StageProgressController().HasDefeatedStage(stageId));
        EnsureDailyState();
        EnsureAttendanceState();
    }

    // 기본 노출 설정과 스테이지 클리어·패배 조건을 모두 만족한 상품만 표시함
    public bool IsProductVisible(ShopProductSO product) =>
        product != null && product.IsVisible && product.IsUnlockedAtCurrentProgress(currentStageIdProvider(), hasDefeatedStageProvider);

    // 아직 구매하지 않은 표시 가능한 1회 한정 패키지를 표시 순서대로 반환함
    public IReadOnlyList<ShopProductSO> GetUnpurchasedPackages()
    {
        EnsureDailyState();

        return database.Products
            .Where(item => item != null && item.Category == ShopProductCategory.Package &&
                           item.PurchaseLimitType == ShopPurchaseLimitType.Once &&
                           IsProductVisible(item) && !purchasedOnceProductIds.Contains(item.ProductId))
            .OrderBy(item => item.DisplayOrder)
            .ToList();
    }

    // 아직 구매하지 않은 표시 가능한 1회 한정 패키지 중 먼저 안내할 상품을 반환함
    public bool TryGetFirstUnpurchasedPackage(out ShopProductSO product)
    {
        product = GetUnpurchasedPackages().FirstOrDefault();
        return product != null;
    }

    // 상품의 현재 구매 가능 상태와 남은 일일 구매 횟수를 조회함
    public ShopProductAvailability GetAvailability(string productId)
    {
        EnsureDailyState();

        if (!database.TryGetProduct(productId, out ShopProductSO product))
        {
            return ShopProductAvailability.NotFound(productId);
        }

        if (!product.IsVisible)
        {
            return new ShopProductAvailability(product, false, ShopFailure.ProductHidden, 0);
        }

        if (!product.IsUnlockedAtCurrentProgress(currentStageIdProvider(), hasDefeatedStageProvider))
        {
            return new ShopProductAvailability(product, false, ShopFailure.StageRequirementNotMet, 0);
        }

        if (product.PurchaseLimitType == ShopPurchaseLimitType.Once && purchasedOnceProductIds.Contains(product.ProductId))
        {
            return new ShopProductAvailability(product, false, ShopFailure.AlreadyPurchased, 0);
        }

        if (product.PurchaseLimitType == ShopPurchaseLimitType.Daily)
        {
            int purchasedCount = GetDailyPurchaseCount(product.ProductId);
            int remaining = Math.Max(0, product.DailyPurchaseLimit - purchasedCount);
            return remaining > 0
                ? new ShopProductAvailability(product, true, ShopFailure.None, remaining)
                : new ShopProductAvailability(product, false, ShopFailure.DailyPurchaseLimitReached, 0);
        }

        return new ShopProductAvailability(product, true, ShopFailure.None, int.MaxValue);
    }

    // 지정 상품을 구매하고 모든 보상을 한 번에 지급함
    public bool TryPurchase(string productId, out ShopPurchaseResult result, out ShopFailure failure)
    {
        result = null;
        failure = ShopFailure.None;

        ShopProductAvailability availability = GetAvailability(productId);
        if (!availability.CanPurchase)
        {
            failure = availability.Failure;
            return false;
        }

        ShopProductSO product = availability.Product;
        if (!TryGetRewardSystems(out CurrencyManager currencyManager, out HeroController heroController, out failure) ||
            !CanGrantRewards(product.Rewards, heroController, out failure))
        {
            return false;
        }

        if (product.PriceType == ShopPriceType.Currency &&
            (currencyManager.GetCurrency(product.PriceCurrency) < product.PriceAmount ||
             !currencyManager.UseCurrency(product.PriceCurrency, product.PriceAmount)))
        {
            failure = ShopFailure.NotEnoughCurrency;
            return false;
        }

        if (!TryGrantRewards(product.Rewards, currencyManager, heroController, out List<ShopGrantedReward> grantedRewards))
        {
            if (product.PriceType == ShopPriceType.Currency)
                currencyManager.AddCurrency(product.PriceCurrency, product.PriceAmount);
            failure = ShopFailure.RewardGrantFailed;
            return false;
        }

        MarkProductPurchased(product);
        result = new ShopPurchaseResult(product, product.PriceCurrency, product.PriceAmount, grantedRewards);
        OnProductPurchased?.Invoke(result);
        OnShopStateChanged?.Invoke();
        SaveImmediately();
        return true;
    }

    // 선택한 날짜의 열린 출석 보상을 수령함
    public bool TryClaimAttendance(int rewardIndex, out ShopAttendanceClaimResult result, out ShopFailure failure)
    {
        result = null;
        failure = ShopFailure.None;
        EnsureAttendanceState();

        if (AttendanceRewards == null || AttendanceRewards.Count == 0)
        {
            failure = ShopFailure.AttendanceNotConfigured;
            return false;
        }

        if (rewardIndex < 0 || rewardIndex >= AttendanceRewards.Count)
        {
            failure = ShopFailure.AttendanceNotConfigured;
            return false;
        }

        if (claimedAttendanceRewardIndices.Contains(rewardIndex))
        {
            failure = ShopFailure.AttendanceAlreadyClaimed;
            return false;
        }

        if (!IsAttendanceRewardUnlocked(rewardIndex))
        {
            failure = ShopFailure.AttendanceNotAvailableYet;
            return false;
        }

        ShopRewardEntry reward = AttendanceRewards[rewardIndex];
        if (!TryGetRewardSystems(out CurrencyManager currencyManager, out HeroController heroController, out failure) ||
            !CanGrantRewards(new[] { reward }, heroController, out failure) ||
            !TryGrantRewards(new[] { reward }, currencyManager, heroController, out List<ShopGrantedReward> grantedRewards))
        {
            return false;
        }

        claimedAttendanceRewardIndices.Add(rewardIndex);
        result = new ShopAttendanceClaimResult(rewardIndex, grantedRewards);
        OnAttendanceClaimed?.Invoke(result);
        OnShopStateChanged?.Invoke();
        SaveImmediately();
        return true;
    }

    // 지정한 출석 보상을 현재 수령할 수 있는지 반환함
    public bool CanClaimAttendance(int rewardIndex)
    {
        EnsureAttendanceState();
        return rewardIndex >= 0 && rewardIndex < AttendanceRewards.Count &&
               !claimedAttendanceRewardIndices.Contains(rewardIndex) && IsAttendanceRewardUnlocked(rewardIndex);
    }

    // 지정한 출석 보상의 열린 상태와 수령 상태를 반환함
    public ShopAttendanceRewardState GetAttendanceRewardState(int rewardIndex)
    {
        EnsureAttendanceState();
        bool isValid = rewardIndex >= 0 && rewardIndex < AttendanceRewards.Count;
        bool isClaimed = isValid && claimedAttendanceRewardIndices.Contains(rewardIndex);
        bool isUnlocked = isValid && IsAttendanceRewardUnlocked(rewardIndex);
        return new ShopAttendanceRewardState(rewardIndex, isUnlocked, isClaimed);
    }

    // 저장 데이터를 기준으로 상점 구매와 출석 상태를 복원함
    public void LoadSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        saveData.Shop ??= new ShopSaveData();
        saveData.Shop.PurchasedOnceProductIds ??= new List<string>();
        saveData.Shop.DailyPurchaseCounts ??= new List<ShopPurchaseCountSaveEntry>();
        saveData.Shop.ClaimedAttendanceRewardIndices ??= new List<int>();

        purchasedOnceProductIds.Clear();
        foreach (string productId in saveData.Shop.PurchasedOnceProductIds.Where(productId => !string.IsNullOrWhiteSpace(productId)))
        {
            purchasedOnceProductIds.Add(productId);
        }

        dailyPurchaseCounts.Clear();
        foreach (ShopPurchaseCountSaveEntry entry in saveData.Shop.DailyPurchaseCounts)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.ProductId) || entry.Count <= 0 || dailyPurchaseCounts.ContainsKey(entry.ProductId))
            {
                continue;
            }

            dailyPurchaseCounts.Add(entry.ProductId, entry.Count);
        }

        lastDailyResetDate = saveData.Shop.LastDailyResetDate;
        attendanceCycleStartDate = saveData.Shop.AttendanceCycleStartDate;
        claimedAttendanceRewardIndices.Clear();
        foreach (int rewardIndex in saveData.Shop.ClaimedAttendanceRewardIndices.Where(index => index >= 0 && index < AttendanceRewards.Count))
        {
            claimedAttendanceRewardIndices.Add(rewardIndex);
        }

        // 기존 순차 수령 저장은 앞에서부터 받은 것으로 이관함
        if (string.IsNullOrWhiteSpace(attendanceCycleStartDate) && saveData.Shop.AttendanceClaimCount > 0)
        {
            int migratedCount = Math.Min(saveData.Shop.AttendanceClaimCount, AttendanceRewards.Count);
            for (int index = 0; index < migratedCount; index++)
                claimedAttendanceRewardIndices.Add(index);
        }

        EnsureDailyState();
        EnsureAttendanceState();
        OnShopStateChanged?.Invoke();
    }

    // 현재 상점 상태를 저장 데이터에 기록함
    public void WriteSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        EnsureDailyState();
        saveData.Shop = new ShopSaveData
        {
            PurchasedOnceProductIds = purchasedOnceProductIds.OrderBy(productId => productId, StringComparer.Ordinal).ToList(),
            DailyPurchaseCounts = dailyPurchaseCounts
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => new ShopPurchaseCountSaveEntry { ProductId = pair.Key, Count = pair.Value })
                .ToList(),
            LastDailyResetDate = lastDailyResetDate,
            AttendanceCycleStartDate = attendanceCycleStartDate,
            ClaimedAttendanceRewardIndices = claimedAttendanceRewardIndices.OrderBy(index => index).ToList()
        };
    }

    // 필요한 매니저가 준비됐는지 확인함
    private static bool TryGetRewardSystems(out CurrencyManager currencyManager, out HeroController heroController, out ShopFailure failure)
    {
        currencyManager = CurrencyManager.Instance;
        heroController = null;
        failure = ShopFailure.None;

        if (currencyManager == null)
        {
            failure = ShopFailure.CurrencySystemUnavailable;
            return false;
        }

        if (!HeroManager.TryGetExistingInstance(out HeroManager heroManager) || !heroManager.IsInitialized)
        {
            failure = ShopFailure.HeroSystemUnavailable;
            return false;
        }

        heroController = heroManager.Controller;
        return true;
    }

    // 모든 보상을 지급하기 전에 영웅 중복과 데이터 오류를 확인함
    private static bool CanGrantRewards(IReadOnlyList<ShopRewardEntry> rewards, HeroController heroController, out ShopFailure failure)
    {
        failure = ShopFailure.None;
        if (rewards == null || rewards.Count == 0)
        {
            failure = ShopFailure.InvalidReward;
            return false;
        }

        HashSet<string> pendingHeroIds = new(StringComparer.Ordinal);
        foreach (ShopRewardEntry reward in rewards)
        {
            if (reward == null)
            {
                failure = ShopFailure.InvalidReward;
                return false;
            }

            if (reward.RewardType == ShopRewardType.Currency)
            {
                if (reward.CurrencyType == CurrencyType.None || reward.CurrencyType >= CurrencyType.Length || reward.Amount <= 0)
                {
                    failure = ShopFailure.InvalidReward;
                    return false;
                }

                continue;
            }

            if (reward.RewardType != ShopRewardType.Hero || reward.HeroData == null || string.IsNullOrWhiteSpace(reward.HeroData.UnitID))
            {
                failure = ShopFailure.InvalidReward;
                return false;
            }

            if (heroController.ContainsHero(reward.HeroData.UnitID) || !pendingHeroIds.Add(reward.HeroData.UnitID))
            {
                failure = ShopFailure.RewardHeroAlreadyOwned;
                return false;
            }
        }

        return true;
    }

    // 사전 검증된 보상을 영웅부터 지급해 부분 지급을 막음
    private static bool TryGrantRewards(
        IReadOnlyList<ShopRewardEntry> rewards,
        CurrencyManager currencyManager,
        HeroController heroController,
        out List<ShopGrantedReward> grantedRewards)
    {
        grantedRewards = new List<ShopGrantedReward>();

        foreach (ShopRewardEntry reward in rewards.Where(reward => reward.RewardType == ShopRewardType.Hero))
        {
            if (!heroController.TryAcquireHero(reward.HeroData.UnitID))
            {
                return false;
            }

            grantedRewards.Add(ShopGrantedReward.FromHero(reward.HeroData.UnitID));
        }

        foreach (ShopRewardEntry reward in rewards.Where(reward => reward.RewardType == ShopRewardType.Currency))
        {
            currencyManager.AddCurrency(reward.CurrencyType, reward.Amount);
            grantedRewards.Add(ShopGrantedReward.FromCurrency(reward.CurrencyType, reward.Amount));
        }

        return true;
    }

    // 상품 제한 상태를 구매 성공 후 갱신함
    private void MarkProductPurchased(ShopProductSO product)
    {
        if (product.PurchaseLimitType == ShopPurchaseLimitType.Once)
        {
            purchasedOnceProductIds.Add(product.ProductId);
        }
        else if (product.PurchaseLimitType == ShopPurchaseLimitType.Daily)
        {
            dailyPurchaseCounts[product.ProductId] = GetDailyPurchaseCount(product.ProductId) + 1;
        }
    }

    // 자정이 지난 경우 일일 상품 구매 횟수를 초기화함
    private void EnsureDailyState()
    {
        string today = GetCurrentDateKey();
        if (string.Equals(lastDailyResetDate, today, StringComparison.Ordinal))
        {
            return;
        }

        dailyPurchaseCounts.Clear();
        lastDailyResetDate = today;
        OnShopStateChanged?.Invoke();
    }

    // 첫 출석 화면 기준으로 1일차를 열고 날짜가 지날수록 보상을 해금함
    private void EnsureAttendanceState()
    {
        if (string.IsNullOrWhiteSpace(attendanceCycleStartDate))
            attendanceCycleStartDate = GetCurrentDateKey();

        claimedAttendanceRewardIndices.RemoveWhere(index => index < 0 || index >= AttendanceRewards.Count);
    }

    // 지정한 일차가 출석 시작일 이후 열렸는지 반환함
    private bool IsAttendanceRewardUnlocked(int rewardIndex)
    {
        if (!DateTime.TryParse(attendanceCycleStartDate, out DateTime startDate))
            return rewardIndex == 0;

        int elapsedDays = Math.Max(0, (GetCurrentDate() - startDate.Date).Days);
        return rewardIndex <= elapsedDays;
    }

    // UTC 날짜를 일일 제한의 공통 기준으로 사용함
    private DateTime GetCurrentDate() => utcNowProvider().Date;

    private string GetCurrentDateKey() => GetCurrentDate().ToString("yyyy-MM-dd");

    // 지정 상품의 오늘 구매 횟수를 반환함
    private int GetDailyPurchaseCount(string productId) =>
        dailyPurchaseCounts.TryGetValue(productId, out int count) ? count : 0;

    // 구매 직후 재화와 제한 상태를 같은 저장 시점에 반영함
    private static void SaveImmediately()
    {
        if (SaveManager.TryGetExistingInstance(out SaveManager saveManager) && saveManager.CurrentData != null)
        {
            saveManager.Save();
        }
    }
}

// 상점 동작 실패 원인을 UI가 문구로 변환할 때 사용할 값임
public enum ShopFailure
{
    None,
    ProductNotFound,
    ProductHidden,
    StageRequirementNotMet,
    AlreadyPurchased,
    DailyPurchaseLimitReached,
    NotEnoughCurrency,
    CurrencySystemUnavailable,
    HeroSystemUnavailable,
    InvalidReward,
    RewardHeroAlreadyOwned,
    RewardGrantFailed,
    AttendanceNotConfigured,
    AttendanceAlreadyClaimed,
    AttendanceNotAvailableYet
}

// 상품 버튼이 현재 표시할 구매 가능 상태임
public sealed class ShopProductAvailability
{
    public ShopProductSO Product { get; }
    public bool CanPurchase { get; }
    public ShopFailure Failure { get; }
    public int RemainingPurchaseCount { get; }

    public ShopProductAvailability(ShopProductSO product, bool canPurchase, ShopFailure failure, int remainingPurchaseCount)
    {
        Product = product;
        CanPurchase = canPurchase;
        Failure = failure;
        RemainingPurchaseCount = remainingPurchaseCount;
    }

    // 존재하지 않는 상품 조회 결과를 생성함
    public static ShopProductAvailability NotFound(string _) =>
        new(null, false, ShopFailure.ProductNotFound, 0);
}

// 상품 구매 결과와 지급 보상을 UI에 전달함
public sealed class ShopPurchaseResult
{
    public ShopProductSO Product { get; }
    public CurrencyType SpentCurrency { get; }
    public int SpentAmount { get; }
    public IReadOnlyList<ShopGrantedReward> GrantedRewards { get; }

    public ShopPurchaseResult(ShopProductSO product, CurrencyType spentCurrency, int spentAmount, IReadOnlyList<ShopGrantedReward> grantedRewards)
    {
        Product = product;
        SpentCurrency = spentCurrency;
        SpentAmount = spentAmount;
        GrantedRewards = grantedRewards;
    }
}

// 출석 보상 수령 결과와 순환 보상 인덱스를 UI에 전달함
public sealed class ShopAttendanceClaimResult
{
    public int RewardIndex { get; }
    public IReadOnlyList<ShopGrantedReward> GrantedRewards { get; }

    public ShopAttendanceClaimResult(int rewardIndex, IReadOnlyList<ShopGrantedReward> grantedRewards)
    {
        RewardIndex = rewardIndex;
        GrantedRewards = grantedRewards;
    }
}

// 출석 보상 한 칸의 해금과 수령 상태를 UI에 전달함
public readonly struct ShopAttendanceRewardState
{
    public int RewardIndex { get; }
    public bool IsUnlocked { get; }
    public bool IsClaimed { get; }
    public bool IsClaimable => IsUnlocked && !IsClaimed;

    public ShopAttendanceRewardState(int rewardIndex, bool isUnlocked, bool isClaimed)
    {
        RewardIndex = rewardIndex;
        IsUnlocked = isUnlocked;
        IsClaimed = isClaimed;
    }
}

// 실제 지급한 보상을 종류별로 표현함
public readonly struct ShopGrantedReward
{
    public ShopRewardType RewardType { get; }
    public CurrencyType CurrencyType { get; }
    public int Amount { get; }
    public string HeroId { get; }

    private ShopGrantedReward(ShopRewardType rewardType, CurrencyType currencyType, int amount, string heroId)
    {
        RewardType = rewardType;
        CurrencyType = currencyType;
        Amount = amount;
        HeroId = heroId;
    }

    // 재화 지급 결과를 생성함
    public static ShopGrantedReward FromCurrency(CurrencyType currencyType, int amount) =>
        new(ShopRewardType.Currency, currencyType, amount, null);

    // 영웅 지급 결과를 생성함
    public static ShopGrantedReward FromHero(string heroId) =>
        new(ShopRewardType.Hero, CurrencyType.None, 0, heroId);
}
