using System;
using System.Collections.Generic;
using System.Linq;

// 업적 컨트롤러를 전역에서 관리하고 게임 이벤트를 누적 통계로 전달함
public sealed class AchievementManager : Singleton<AchievementManager>
{
    private AchievementController controller;
    private AchievementDatabaseSO database;
    private GachaController subscribedGachaController;
    private HeroController subscribedHeroController;
    private ResonanceController subscribedResonanceController;

    public bool IsInitialized => controller != null;

    // 현재 수령 가능한 보상 업적이 하나 이상 있는지 반환함
    public bool HasClaimableRewards => IsInitialized && database != null &&
        database.Definitions.Any(definition => definition != null && definition.HasReward && controller.CanClaim(definition));

    // 지정 분류 안에 수령 가능한 보상이 하나 이상 있는지 반환함
    public bool HasClaimableRewardsInCategory(AchievementCategory category) => IsInitialized && database != null &&
        database.Definitions.Any(definition => definition != null && definition.Category == category &&
                                                definition.HasReward && controller.CanClaim(definition));

    public AchievementController Controller
    {
        get
        {
            if (controller == null)
            {
                throw new InvalidOperationException("AchievementManager 초기화되지 않음");
            }

            return controller;
        }
    }

    // 저장 데이터, 업적 정의, 가챠 컨트롤러를 연결해 업적 시스템을 초기화함
    public void Initialize(GameSaveData saveData, AchievementDatabaseSO achievementDatabase, GachaController gachaController)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        if (gachaController == null)
        {
            throw new ArgumentNullException(nameof(gachaController));
        }

        if (achievementDatabase == null)
        {
            throw new ArgumentNullException(nameof(achievementDatabase));
        }

        if (IsInitialized)
        {
            return;
        }

        if (!achievementDatabase.TryValidate(out string validationError))
        {
            throw new InvalidOperationException($"AchievementDatabase 설정 오류: {validationError}");
        }

        database = achievementDatabase;
        controller = new AchievementController();
        controller.LoadSaveData(saveData, GetLegacyTotalGachaPulls(saveData));
        controller.RecordFirstLogin();

        subscribedGachaController = gachaController;
        subscribedGachaController.OnDrawCompleted += HandleGachaDrawCompleted;

        SubscribeHeroEvents();
        SubscribeResonanceEvents();
    }

    // 스테이지 클리어 지점이 호출할 최고 진행도 기록 진입점임
    public void RecordStageCleared(int stageNumber)
    {
        SetMetricMaximum(AchievementMetric.MaxClearedStage, stageNumber);
    }

    // 공명 최고 레벨을 현재 수치보다 높을 때만 기록함
    public void RecordResonanceLevel(int resonanceLevel)
    {
        SetMetricMaximum(AchievementMetric.MaxResonanceLevel, resonanceLevel);
    }

    // 보유 영웅 수는 최고 보유 기록으로 유지함
    public void RecordOwnedHeroCount(int ownedHeroCount)
    {
        SetMetricMaximum(AchievementMetric.MaxOwnedHeroCount, ownedHeroCount);
    }

    // 픽업 배너에서 실행한 실제 소환 횟수를 누적함
    public void RecordPickupDraws(int drawCount)
    {
        AddMetricValue(AchievementMetric.TotalPickupDraws, drawCount);
    }

    // 중복이 아닌 픽업 영웅 획득 횟수를 누적함
    public void RecordPickupHeroAcquisitions(int acquisitionCount)
    {
        AddMetricValue(AchievementMetric.PickupHeroAcquisitions, acquisitionCount);
    }

    // 중복 영웅이 골드로 전환된 횟수를 누적함
    public void RecordDuplicateHeroGoldConversions(int conversionCount)
    {
        AddMetricValue(AchievementMetric.DuplicateHeroGoldConversions, conversionCount);
    }

    // 전투 승리와 5인 원정대 승리를 함께 기록함
    public void RecordBattleVictory(bool wasFullParty)
    {
        AddMetricValue(AchievementMetric.TotalBattleVictories, 1);

        if (wasFullParty)
        {
            AddMetricValue(AchievementMetric.FullPartyBattleVictories, 1);
        }
    }

    // 실제 방치 보상 수령 성공 횟수를 누적함
    public void RecordIdleRewardClaim()
    {
        AddMetricValue(AchievementMetric.IdleRewardClaims, 1);
    }

    // 누적형 업적 지표에 양수 값을 더함
    public void AddMetricValue(AchievementMetric metric, int amount)
    {
        if (IsInitialized)
        {
            controller.AddMetricValue(metric, amount);
        }
    }

    // 현재 수치형 업적 지표를 지정값으로 갱신함
    public void SetMetricValue(AchievementMetric metric, int value)
    {
        if (IsInitialized)
        {
            controller.SetMetricValue(metric, value);
        }
    }

    // 최고 기록형 업적 지표를 더 높은 값일 때만 갱신함
    public void SetMetricMaximum(AchievementMetric metric, int value)
    {
        if (IsInitialized)
        {
            controller.SetMetricMaximum(metric, value);
        }
    }

// 완료된 업적의 설정된 재화 보상을 한 번 지급하고 수령 상태를 처리함
    public bool TryClaim(AchievementDefinitionSO definition, out CurrencyType rewardCurrency, out int rewardAmount)
    {
        rewardCurrency = CurrencyType.None;
        rewardAmount = 0;

        if (!IsInitialized || definition == null || database == null || !database.Contains(definition) ||
            !definition.HasReward || !controller.CanClaim(definition))
        {
            return false;
        }

        CurrencyManager currencyManager = CurrencyManager.Instance;
        if (currencyManager == null)
        {
            return false;
        }

        rewardCurrency = definition.RewardCurrency;
        rewardAmount = definition.RewardAmount;
        if (!controller.TryMarkClaimed(definition))
        {
            rewardCurrency = CurrencyType.None;
            rewardAmount = 0;
            return false;
        }

        currencyManager.AddCurrency(rewardCurrency, rewardAmount);
        return true;
    }

    // 수령 가능한 모든 업적 보상을 한 번에 지급하고 수령 처리함
    public int TryClaimAll(out List<AchievementClaimReward> rewards)
    {
        return TryClaimAllInternal(null, out rewards);
    }

    // 지정 분류 안에서 수령 가능한 업적 보상을 모두 지급함
    public int TryClaimAll(AchievementCategory category, out List<AchievementClaimReward> rewards)
    {
        return TryClaimAllInternal(category, out rewards);
    }

    // 전체 또는 지정 분류의 수령 가능한 보상을 공통 처리함
    private int TryClaimAllInternal(AchievementCategory? category, out List<AchievementClaimReward> rewards)
    {
        rewards = new List<AchievementClaimReward>();

        if (!IsInitialized || database == null)
        {
            return 0;
        }

        CurrencyManager currencyManager = CurrencyManager.Instance;
        if (currencyManager == null)
        {
            return 0;
        }

        int claimedCount = 0;
        foreach (AchievementDefinitionSO definition in database.Definitions)
        {
            if (definition == null || (category.HasValue && definition.Category != category.Value) ||
                !definition.HasReward || !controller.CanClaim(definition))
            {
                continue;
            }

            if (!controller.TryMarkClaimed(definition))
            {
                continue;
            }

            currencyManager.AddCurrency(definition.RewardCurrency, definition.RewardAmount);
            rewards.Add(new AchievementClaimReward(definition, definition.RewardCurrency, definition.RewardAmount));
            claimedCount++;
        }

        return claimedCount;
    }


    // 업적 상태를 저장 데이터에 반영함
    public void WriteSaveData(GameSaveData saveData)
    {
        if (!IsInitialized)
        {
            return;
        }

        controller.WriteSaveData(saveData);
    }

    protected override void OnDestroy()
    {
        if (subscribedGachaController != null)
        {
            subscribedGachaController.OnDrawCompleted -= HandleGachaDrawCompleted;
            subscribedGachaController = null;
        }

        if (subscribedHeroController != null)
        {
            subscribedHeroController.OnHeroCollectionChanged -= HandleHeroCollectionChanged;
            subscribedHeroController.OnHeroLevelChanged -= HandleHeroLevelChanged;
            subscribedHeroController = null;
        }

        if (subscribedResonanceController != null)
        {
            subscribedResonanceController.OnResonanceSlotChanged -= HandleResonanceSlotChanged;
            subscribedResonanceController = null;
        }

        base.OnDestroy();
    }

    // 성공한 실제 소환 수만큼 누적 가챠 업적 통계를 증가시킴
    private void HandleGachaDrawCompleted(GachaDrawResult result)
    {
        int pullCount = result?.PullResults?.Count ?? 0;
        AddMetricValue(AchievementMetric.TotalGachaPulls, pullCount);

        if (result == null || pullCount == 0)
        {
            return;
        }

        if (subscribedGachaController.TryGetBannerData(result.BannerId, out GachaBannerDataSO banner) &&
            banner.HeroPool.Any(entry => entry != null && entry.IsPickup))
        {
            RecordPickupDraws(pullCount);
        }

        int pickupAcquisitionCount = result.PullResults.Count(pull =>
            pull != null && pull.IsPickup && !pull.IsDuplicate);
        RecordPickupHeroAcquisitions(pickupAcquisitionCount);

        int duplicateConversionCount = result.PullResults.Count(pull =>
            pull != null && pull.IsDuplicate && pull.ConvertedGold > 0);
        RecordDuplicateHeroGoldConversions(duplicateConversionCount);
    }

    // 영웅 보유 목록과 레벨 변경을 업적 통계에 연결함
    private void SubscribeHeroEvents()
    {
        if (!HeroManager.TryGetExistingInstance(out HeroManager heroManager) || !heroManager.IsInitialized)
        {
            return;
        }

        subscribedHeroController = heroManager.Controller;
        subscribedHeroController.OnHeroCollectionChanged += HandleHeroCollectionChanged;
        subscribedHeroController.OnHeroLevelChanged += HandleHeroLevelChanged;
        HandleHeroCollectionChanged();
    }

    // 공명 슬롯 변경을 업적 통계에 연결함
    private void SubscribeResonanceEvents()
    {
        if (!ResonanceManager.TryGetExistingInstance(out ResonanceManager resonanceManager) || !resonanceManager.IsInitialized)
        {
            return;
        }

        subscribedResonanceController = resonanceManager.Controller;
        subscribedResonanceController.OnResonanceSlotChanged += HandleResonanceSlotChanged;
        HandleResonanceSlotChanged();
    }

    // 보유 영웅 수와 현재 공명 레벨을 다시 확인함
    private void HandleHeroCollectionChanged()
    {
        RecordOwnedHeroCount(subscribedHeroController?.Heroes.Count ?? 0);
        HandleResonanceSlotChanged();
    }

    // 공명 기준 영웅 레벨 변경 시 최고 공명 레벨을 다시 확인함
    private void HandleHeroLevelChanged(OwnedHeroData _)
    {
        HandleResonanceSlotChanged();
    }

    // 공명 활성 상태일 때만 현재 공명 레벨을 기록함
    private void HandleResonanceSlotChanged()
    {
        if (subscribedResonanceController != null &&
            subscribedResonanceController.TryGetResonanceLevel(out int resonanceLevel))
        {
            RecordResonanceLevel(resonanceLevel);
        }
    }

    // 업적 도입 전 저장된 배너 그룹별 누적 소환 수를 한 번 합산함
    private static int GetLegacyTotalGachaPulls(GameSaveData saveData)
    {
        return saveData.Gacha?.BannerProgresses?
            .Where(progress => progress != null)
            .Sum(progress => Math.Max(0, progress.TotalPullCount)) ?? 0;
    }
}

// 업적 보상 수령 토스트가 사용할 재화 보상 정보임
public readonly struct AchievementClaimReward
{
    public AchievementDefinitionSO Definition { get; }
    public CurrencyType Currency { get; }
    public int Amount { get; }

    public AchievementClaimReward(AchievementDefinitionSO definition, CurrencyType currency, int amount)
    {
        Definition = definition;
        Currency = currency;
        Amount = amount;
    }
}
