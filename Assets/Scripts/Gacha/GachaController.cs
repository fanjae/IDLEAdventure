using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 가챠의 확률 계산, 재화 차감, 보상 지급을 담당함
public sealed class GachaController
{
    private readonly GachaDatabaseSO database;
    private readonly HeroDatabaseSO heroDatabase;
    private readonly Dictionary<string, GachaBannerProgressSaveData> progressByGroup = new(StringComparer.Ordinal);

    public event Action<GachaDrawResult> OnDrawCompleted;
    public IReadOnlyList<GachaBannerDataSO> Banners => database.Banners;

    // 정적 배너 데이터와 런타임 진행도 컨테이너를 연결함
    public GachaController(GachaDatabaseSO database, HeroDatabaseSO heroDatabase)
    {
        this.database = database ?? throw new ArgumentNullException(nameof(database));
        this.heroDatabase = heroDatabase ?? throw new ArgumentNullException(nameof(heroDatabase));
    }

    // 배너에서 지정 횟수만큼 소환을 시도함
    public bool TryDraw(string bannerId, int drawCount, out GachaDrawResult drawResult, out GachaDrawFailure failure)
    {
        drawResult = null;
        failure = GachaDrawFailure.None;

        if (!database.TryGetBanner(bannerId, out GachaBannerDataSO banner))
        {
            failure = GachaDrawFailure.BannerNotFound;
            return false;
        }

        if (drawCount != 1 && drawCount != 10)
        {
            failure = GachaDrawFailure.InvalidDrawCount;
            return false;
        }

        if (HeroManager.Instance == null || !HeroManager.Instance.IsInitialized)
        {
            failure = GachaDrawFailure.HeroSystemUnavailable;
            return false;
        }

        if (!HasUsablePool(banner))
        {
            failure = GachaDrawFailure.InvalidPool;
            return false;
        }

        int gemCost = banner.GetGemCost(drawCount);
        if (CurrencyManager.Instance.GetCurrency(CurrencyType.GEM) < gemCost)
        {
            failure = GachaDrawFailure.NotEnoughGem;
            return false;
        }

        GachaBannerProgressSaveData progress = GetOrCreateProgress(banner.PityGroupId);
        List<GachaRolledEntry> selectedEntries = RollEntries(banner, drawCount, progress.PullCountSinceTier2, out int nextPityCount);

        HeroController heroController = HeroManager.Instance.Controller;
        if (selectedEntries.Any(rolledEntry => !heroDatabase.TryGetHero(rolledEntry.Entry.HeroId, out _)))
        {
            failure = GachaDrawFailure.HeroDataNotFound;
            return false;
        }

        List<string> heroIdsToGrant = new();
        List<GachaPullResult> pullResults = new(selectedEntries.Count);
        HashSet<string> heroIdsAcquiredInThisDraw = new(StringComparer.Ordinal);
        int totalDuplicateGold = 0;

        foreach (GachaRolledEntry rolledEntry in selectedEntries)
        {
            GachaHeroPoolEntry entry = rolledEntry.Entry;
            bool isDuplicate = heroController.ContainsHero(entry.HeroId) || !heroIdsAcquiredInThisDraw.Add(entry.HeroId);
            int convertedGold = isDuplicate ? banner.GetDuplicateGold(entry.Rarity) : 0;

            if (isDuplicate)
            {
                totalDuplicateGold += convertedGold;
            }
            else
            {
                heroIdsToGrant.Add(entry.HeroId);
            }

            pullResults.Add(new GachaPullResult(entry.HeroId, entry.Rarity, entry.IsPickup, rolledEntry.IsPity, isDuplicate, convertedGold));
        }

        // 다중 소환 전체가 지급 가능한지 먼저 검증해 부분 지급을 막음
        if (!GachaHeroGrantPreflight.CanGrantAll(heroController, heroDatabase, heroIdsToGrant))
        {
            failure = GachaDrawFailure.HeroGrantFailed;
            return false;
        }

        if (!CurrencyManager.Instance.UseCurrency(CurrencyType.GEM, gemCost))
        {
            failure = GachaDrawFailure.NotEnoughGem;
            return false;
        }

        // 사전 검증한 신규 영웅을 지급함. 현재 HeroController 공개 API만 사용함.
        foreach (string heroId in heroIdsToGrant)
        {
            if (!heroController.TryAcquireHero(heroId))
            {
                CurrencyManager.Instance.AddCurrency(CurrencyType.GEM, gemCost);
                failure = GachaDrawFailure.HeroGrantFailed;
                return false;
            }
        }

        if (totalDuplicateGold > 0)
        {
            CurrencyManager.Instance.AddCurrency(CurrencyType.GOLD, totalDuplicateGold);
        }

        progress.PullCountSinceTier2 = nextPityCount;
        progress.TotalPullCount += drawCount;

        drawResult = new GachaDrawResult(banner.BannerId, gemCost, banner.Tier2PityCount - nextPityCount, pullResults);
        OnDrawCompleted?.Invoke(drawResult);
        SaveManager.Instance?.Save();
        return true;
    }

    // 현재 배너가 2티어 확정까지 남긴 소환 횟수 반환함
    public bool TryGetPullsUntilTier2Pity(string bannerId, out int pullsUntilPity)
    {
        pullsUntilPity = 0;

        if (!database.TryGetBanner(bannerId, out GachaBannerDataSO banner))
        {
            return false;
        }

        GachaBannerProgressSaveData progress = GetOrCreateProgress(banner.PityGroupId);
        pullsUntilPity = Mathf.Max(0, banner.Tier2PityCount - progress.PullCountSinceTier2);
        return true;
    }

    // 배너 화면이 표시할 원본 데이터를 조회함
    public bool TryGetBannerData(string bannerId, out GachaBannerDataSO banner)
    {
        return database.TryGetBanner(bannerId, out banner);
    }

    // 저장된 배너별 천장 진행도를 불러옴
    public void LoadSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        saveData.Gacha ??= new GachaSaveData();
        saveData.Gacha.BannerProgresses ??= new List<GachaBannerProgressSaveData>();
        progressByGroup.Clear();

        foreach (GachaBannerProgressSaveData savedProgress in saveData.Gacha.BannerProgresses)
        {
            if (savedProgress == null || string.IsNullOrWhiteSpace(savedProgress.PityGroupId))
            {
                continue;
            }

            if (!progressByGroup.TryAdd(savedProgress.PityGroupId, new GachaBannerProgressSaveData
            {
                PityGroupId = savedProgress.PityGroupId,
                PullCountSinceTier2 = Mathf.Max(0, savedProgress.PullCountSinceTier2),
                TotalPullCount = Mathf.Max(0, savedProgress.TotalPullCount)
            }))
            {
                Debug.LogWarning($"[Gacha] 중복 천장 저장 데이터 무시됨: {savedProgress.PityGroupId}");
            }
        }
    }

    // 현재 천장 진행도를 저장 데이터에 기록함
    public void WriteSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        saveData.Gacha = new GachaSaveData
        {
            BannerProgresses = progressByGroup.Values
                .Select(progress => new GachaBannerProgressSaveData
                {
                    PityGroupId = progress.PityGroupId,
                    PullCountSinceTier2 = progress.PullCountSinceTier2,
                    TotalPullCount = progress.TotalPullCount
                })
                .ToList()
        };
    }

    // 각 티어가 뽑힐 수 있는 영웅을 하나 이상 가지는지 확인함
    private static bool HasUsablePool(GachaBannerDataSO banner)
    {
        bool hasTier1Entry = banner.HeroPool.Any(entry => entry != null && entry.Rarity == GachaRarity.Tier1 && !string.IsNullOrWhiteSpace(entry.HeroId));
        bool hasTier2Entry = banner.HeroPool.Any(entry => entry != null && entry.Rarity == GachaRarity.Tier2 && !string.IsNullOrWhiteSpace(entry.HeroId));
        return hasTier1Entry && hasTier2Entry && banner.Tier1Weight > 0 && banner.Tier2Weight > 0;
    }

    // 실제 상태를 바꾸지 않고 이번 결과와 다음 천장 수치를 미리 계산함
    private static List<GachaRolledEntry> RollEntries(GachaBannerDataSO banner, int drawCount, int currentPityCount, out int nextPityCount)
    {
        List<GachaRolledEntry> results = new(drawCount);
        int pityCount = Mathf.Max(0, currentPityCount);

        for (int i = 0; i < drawCount; i++)
        {
            bool isPityDraw = pityCount + 1 >= Mathf.Max(1, banner.Tier2PityCount);
            GachaHeroPoolEntry entry = isPityDraw ? PickTierEntry(banner, GachaRarity.Tier2) : PickNormalEntry(banner);
            results.Add(new GachaRolledEntry(entry, isPityDraw));
            pityCount = entry.Rarity == GachaRarity.Tier2 ? 0 : pityCount + 1;
        }

        nextPityCount = pityCount;
        return results;
    }

    // 티어 가중치로 일반 소환의 등급을 먼저 선택함
    private static GachaHeroPoolEntry PickNormalEntry(GachaBannerDataSO banner)
    {
        int totalTierWeight = banner.Tier1Weight + banner.Tier2Weight;
        if (totalTierWeight <= 0)
        {
            throw new InvalidOperationException("티어 가중치 합계가 0 이하임");
        }

        int roll = UnityEngine.Random.Range(0, totalTierWeight);
        GachaRarity rarity = roll < banner.Tier1Weight ? GachaRarity.Tier1 : GachaRarity.Tier2;
        return PickTierEntry(banner, rarity);
    }

    // 선택된 티어 안에서 픽업 영웅만 보정해 영웅을 선택함
    private static GachaHeroPoolEntry PickTierEntry(GachaBannerDataSO banner, GachaRarity rarity)
    {
        List<GachaHeroPoolEntry> tierEntries = banner.HeroPool
            .Where(entry => entry != null && entry.Rarity == rarity && !string.IsNullOrWhiteSpace(entry.HeroId))
            .ToList();

        if (tierEntries.Count == 0)
        {
            throw new InvalidOperationException($"{rarity} 영웅 풀이 비어 있음");
        }

        bool hasPickup = tierEntries.Any(entry => entry.IsPickup);
        float totalWeight = tierEntries.Sum(entry => hasPickup && entry.IsPickup ? banner.PickupWeightMultiplier : 1f);
        float roll = UnityEngine.Random.value * totalWeight;

        foreach (GachaHeroPoolEntry entry in tierEntries)
        {
            roll -= hasPickup && entry.IsPickup ? banner.PickupWeightMultiplier : 1f;
            if (roll < 0)
            {
                return entry;
            }
        }

        return tierEntries[^1];
    }

    // 배너 그룹별 천장 진행도 컨테이너를 준비함
    private GachaBannerProgressSaveData GetOrCreateProgress(string pityGroupId)
    {
        if (progressByGroup.TryGetValue(pityGroupId, out GachaBannerProgressSaveData progress))
        {
            return progress;
        }

        progress = new GachaBannerProgressSaveData { PityGroupId = pityGroupId };
        progressByGroup.Add(pityGroupId, progress);
        return progress;
    }

    // 실제 지급 전 계산한 영웅과 천장 발동 여부를 함께 보관함
    private sealed class GachaRolledEntry
    {
        public GachaHeroPoolEntry Entry { get; }
        public bool IsPity { get; }

        public GachaRolledEntry(GachaHeroPoolEntry entry, bool isPity)
        {
            Entry = entry;
            IsPity = isPity;
        }
    }
}

public enum GachaDrawFailure
{
    None,
    BannerNotFound,
    InvalidDrawCount,
    NotEnoughGem,
    InvalidPool,
    HeroSystemUnavailable,
    HeroDataNotFound,
    HeroGrantFailed
}

public sealed class GachaDrawResult
{
    public string BannerId { get; }
    public int SpentGem { get; }
    public int PullsUntilTier2Pity { get; }
    public IReadOnlyList<GachaPullResult> PullResults { get; }

    // 결과 화면이 사용할 소환 결과 묶음 생성함
    public GachaDrawResult(string bannerId, int spentGem, int pullsUntilTier2Pity, IReadOnlyList<GachaPullResult> pullResults)
    {
        BannerId = bannerId;
        SpentGem = spentGem;
        PullsUntilTier2Pity = Mathf.Max(0, pullsUntilTier2Pity);
        PullResults = pullResults;
    }
}

public sealed class GachaPullResult
{
    public string HeroId { get; }
    public GachaRarity Rarity { get; }
    public bool IsPickup { get; }
    public bool IsPity { get; }
    public bool IsDuplicate { get; }
    public int ConvertedGold { get; }

    // 결과 카드가 필요한 표시 정보를 보관함
    public GachaPullResult(string heroId, GachaRarity rarity, bool isPickup, bool isPity, bool isDuplicate, int convertedGold)
    {
        HeroId = heroId;
        Rarity = rarity;
        IsPickup = isPickup;
        IsPity = isPity;
        IsDuplicate = isDuplicate;
        ConvertedGold = convertedGold;
    }
}
