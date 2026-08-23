using System;
using System.Collections.Generic;
using System.Linq;

// 업적이 공통으로 사용하는 누적 통계와 수령 상태를 관리함
public sealed class AchievementController
{
    private readonly Dictionary<AchievementMetric, int> metricValues = new();
    private bool hasMigratedGachaPulls;
    private readonly HashSet<string> claimedAchievementIds = new(StringComparer.Ordinal);

    public event Action<AchievementMetric, int> OnMetricChanged;
    public event Action<string> OnAchievementClaimed;

    public bool HasFirstLogin => GetMetricValue(AchievementMetric.FirstLogin) > 0;
    public int TotalGachaPulls => GetMetricValue(AchievementMetric.TotalGachaPulls);
    public int MaxClearedStage => GetMetricValue(AchievementMetric.MaxClearedStage);

    // 저장 데이터와 기존 가챠 누적 기록을 업적 상태로 복원함
    public bool LoadSaveData(GameSaveData saveData, int legacyTotalGachaPulls)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        saveData.Achievements ??= new AchievementSaveData();
        AchievementSaveData data = saveData.Achievements;

        metricValues.Clear();
        if (data.MetricValues != null)
        {
            foreach (AchievementMetricSaveEntry entry in data.MetricValues)
            {
                if (entry != null)
                {
                    SetMetricValueSilently(entry.Metric, entry.Value);
                }
            }
        }

        hasMigratedGachaPulls = data.HasMigratedGachaPulls;

        claimedAchievementIds.Clear();
        if (data.ClaimedAchievementIds != null)
        {
            foreach (string achievementId in data.ClaimedAchievementIds)
            {
                if (!string.IsNullOrWhiteSpace(achievementId))
                {
                    claimedAchievementIds.Add(achievementId);
                }
            }
        }

        bool wasChanged = false;
        if (!data.MetricValuesMigrated)
        {
            SetMetricMaximumSilently(AchievementMetric.FirstLogin, data.HasFirstLogin ? 1 : 0);
            SetMetricMaximumSilently(AchievementMetric.TotalGachaPulls, data.TotalGachaPulls);
            SetMetricMaximumSilently(AchievementMetric.MaxClearedStage, data.MaxClearedStage);
            wasChanged = true;
        }

        if (!hasMigratedGachaPulls)
        {
            SetMetricMaximumSilently(AchievementMetric.TotalGachaPulls, legacyTotalGachaPulls);
            hasMigratedGachaPulls = true;
            wasChanged = true;
        }

        return wasChanged;
    }

    // 현재 업적 상태를 저장 데이터에 반영함
    public void WriteSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        saveData.Achievements = new AchievementSaveData
        {
            MetricValues = metricValues
                .OrderBy(pair => pair.Key)
                .Select(pair => new AchievementMetricSaveEntry { Metric = pair.Key, Value = pair.Value })
                .ToList(),
            MetricValuesMigrated = true,
            HasMigratedGachaPulls = hasMigratedGachaPulls,
            ClaimedAchievementIds = new List<string>(claimedAchievementIds)
        };
    }

    // 첫 정상 접속 업적의 공통 통계를 한 번만 기록함
    public bool RecordFirstLogin()
    {
        return SetMetricValue(AchievementMetric.FirstLogin, 1);
    }

    // 성공한 실제 소환 수만큼 누적 소환 통계를 증가시킴
    public bool RecordGachaPulls(int pullCount)
    {
        return AddMetricValue(AchievementMetric.TotalGachaPulls, pullCount);
    }

    // 이미 클리어한 스테이지보다 높은 경우에만 최고 진행도를 갱신함
    public bool RecordStageCleared(int stageNumber)
    {
        return SetMetricMaximum(AchievementMetric.MaxClearedStage, stageNumber);
    }

    // 지정 지표를 정확한 값으로 갱신하고 변경 알림을 보냄
    public bool SetMetricValue(AchievementMetric metric, int value)
    {
        value = Math.Max(0, value);
        if (GetMetricValue(metric) == value)
        {
            return false;
        }

        metricValues[metric] = value;
        OnMetricChanged?.Invoke(metric, value);
        return true;
    }

    // 지정 지표에 양수 값을 더하고 변경 알림을 보냄
    public bool AddMetricValue(AchievementMetric metric, int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        return SetMetricValue(metric, GetMetricValue(metric) + amount);
    }

    // 현재 값보다 높은 경우에만 지정 지표를 갱신함
    public bool SetMetricMaximum(AchievementMetric metric, int value)
    {
        value = Math.Max(0, value);
        return value > GetMetricValue(metric) && SetMetricValue(metric, value);
    }

    // 업적 정의가 바라보는 현재 진행도와 완료 여부를 계산함
    public AchievementProgress GetProgress(AchievementDefinitionSO definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        int target = definition.TargetValue;
        int current = Math.Min(GetMetricValue(definition.Metric), target);
        return new AchievementProgress(current, target, IsClaimed(definition.AchievementId));
    }

    // 보상을 아직 수령하지 않은 완료 업적인지 확인함
    public bool CanClaim(AchievementDefinitionSO definition)
    {
        if (definition == null || string.IsNullOrWhiteSpace(definition.AchievementId))
        {
            return false;
        }

        return GetMetricValue(definition.Metric) >= definition.TargetValue && !IsClaimed(definition.AchievementId);
    }

    // 보상 지급이 성공한 뒤 해당 업적을 수령 완료로 표시함
    public bool TryMarkClaimed(AchievementDefinitionSO definition)
    {
        if (!CanClaim(definition))
        {
            return false;
        }

        claimedAchievementIds.Add(definition.AchievementId);
        OnAchievementClaimed?.Invoke(definition.AchievementId);
        return true;
    }

    // 특정 업적의 수령 완료 여부를 반환함
    public bool IsClaimed(string achievementId)
    {
        return !string.IsNullOrWhiteSpace(achievementId) && claimedAchievementIds.Contains(achievementId);
    }

    // 지정 지표의 현재 저장값을 반환함
    public int GetMetricValue(AchievementMetric metric)
    {
        return metricValues.TryGetValue(metric, out int value) ? value : 0;
    }

    // 저장 복원 과정에서는 이벤트 없이 값만 설정함
    private void SetMetricValueSilently(AchievementMetric metric, int value)
    {
        metricValues[metric] = Math.Max(0, value);
    }

    // 구 저장 필드를 옮길 때 현재 범용 값보다 큰 값만 유지함
    private void SetMetricMaximumSilently(AchievementMetric metric, int value)
    {
        value = Math.Max(0, value);
        if (value > GetMetricValue(metric))
        {
            metricValues[metric] = value;
        }
    }
}

// UI가 사용하는 계산 완료된 업적 상태임
public readonly struct AchievementProgress
{
    public int Current { get; }
    public int Target { get; }
    public bool IsCompleted => Current >= Target;
    public bool IsClaimed { get; }

    public AchievementProgress(int current, int target, bool isClaimed)
    {
        Target = Math.Max(1, target);
        Current = Math.Max(0, Math.Min(current, Target));
        IsClaimed = isClaimed;
    }
}
