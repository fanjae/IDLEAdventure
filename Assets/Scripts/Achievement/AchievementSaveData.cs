using System;
using System.Collections.Generic;

// 업적 공통 진행도와 보상 수령 상태를 저장함
[Serializable]
public sealed class AchievementSaveData
{
    // 딕셔너리 값을 JSON 저장용 목록으로 변환해 보관함
    public List<AchievementMetricSaveEntry> MetricValues { get; set; } = new();

    // 구 개별 필드 저장값을 범용 목록으로 옮겼는지 기록함
    public bool MetricValuesMigrated { get; set; }

    // 구 버전 저장 파일 호환용 필드임. 새 저장에는 사용하지 않음
    public bool HasFirstLogin { get; set; }
    public int TotalGachaPulls { get; set; }
    public int MaxClearedStage { get; set; }

    // 기존 가챠 저장값을 누적 업적 통계로 옮겼는지 기록함
    public bool HasMigratedGachaPulls { get; set; }

    // 보상을 이미 받은 업적 ID 목록임
    public List<string> ClaimedAchievementIds { get; set; } = new();
}

// JSON에서 업적 지표와 값을 저장할 항목임
[Serializable]
public sealed class AchievementMetricSaveEntry
{
    public AchievementMetric Metric { get; set; }
    public int Value { get; set; }
}
