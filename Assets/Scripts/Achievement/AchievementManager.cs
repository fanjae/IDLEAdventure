using System;
using System.Collections.Generic;
using System.Linq;

// 업적 컨트롤러를 전역에서 관리하고 게임 이벤트를 누적 통계로 전달함
public sealed class AchievementManager : Singleton<AchievementManager>
{
    private AchievementController controller;
    private AchievementDatabaseSO database;
    private GachaController subscribedGachaController;

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

        // [0823 추가]
        // 저장된 업적 진행도와 보상 수령 상태를 복원
        // 기존 가챠 저장 데이터가 존재하는 경우 업적 누적 횟수로 변환하는 작업도 함께 처리
        bool saveDataChanged = controller.LoadSaveData(saveData, GetLegacyTotalGachaPulls(saveData));

        // 최초 로그인 업적은 게임 실행 시 한 번 달성 상태로 변경
        // 이미 달성되어 있는 경우 false를 반환하기 때문에 기존 값은 변경되지 않음
        saveDataChanged |= controller.RecordFirstLogin();

        // 구 버전 데이터 마이그레이션이나 최초 로그인 기록으로 데이터가 변경된 경우
        // 현재 GameSaveData에도 변경된 업적 상태를 바로 반영
        if (saveDataChanged)
        {
            controller.WriteSaveData(saveData);
        }
        // [0823 추가]

        subscribedGachaController = gachaController;
        subscribedGachaController.OnDrawCompleted += HandleGachaDrawCompleted;

    }

    // 스테이지 클리어 지점이 호출할 최고 진행도 기록 진입점임
    public void RecordStageCleared(int stageNumber)
    {
        SetMetricMaximum(AchievementMetric.MaxClearedStage, stageNumber);
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

        // [0823 추가]
        // 업적에 설정된 재화 보상을 실제 보유 재화에 반영
        currencyManager.AddCurrency(rewardCurrency, rewardAmount);

        // 보상 수령 상태와 지급된 재화를 하나의 저장 시점에서 함께 저장
        // 수령 직후 저장하여 게임 종료 후 같은 업적 보상을 다시 받을 수 없도록 처리
        if (SaveManager.TryGetExistingInstance(out SaveManager saveManager) && saveManager.CurrentData != null)
        {
            saveManager.Save();
        }
        // [0823 추가]

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

        // [0823 추가]
        // 한 개 이상의 업적 보상을 실제로 수령한 경우에만 저장
        // 반복문 내부에서 저장하지 않고 모든 보상 지급이 끝난 뒤 한 번만 저장
        if (claimedCount > 0 && SaveManager.TryGetExistingInstance(out SaveManager saveManager) && saveManager.CurrentData != null)
        {
            saveManager.Save();
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

        base.OnDestroy();
    }

    // 성공한 실제 소환 수만큼 누적 가챠 업적 통계를 증가시킴
    private void HandleGachaDrawCompleted(GachaDrawResult result)
    {
        int pullCount = result?.PullResults?.Count ?? 0;
        AddMetricValue(AchievementMetric.TotalGachaPulls, pullCount);
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
