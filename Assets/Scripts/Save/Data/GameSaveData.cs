using System;
using UnityEngine;

[Serializable]
public sealed class GameSaveData
{
    public int Version { get; set; } = 2;

    // 마지막 저장 시각
    public long SavedAtUnixTime { get; set; }

    public InventorySaveData Inventory { get; set; } = new();
    public EquipmentSaveData Equipment { get; set; } = new();

    public HeroSaveData Heroes { get; set; } = new();

    public CurrencySaveData Currency { get; set; } = new();

    public ResonanceSaveData Resonance = new();

    public StageProgressSaveData StageProgress { get; set; } = new();

    // 영웅 배치 저장 데이터
    public FormationSaveData Formation { get; set; } = new();

    // 배너별 천장 진행도 저장 데이터
    public GachaSaveData Gacha { get; set; } = new();

    // 업적 공통 통계와 수령 상태
    public AchievementSaveData Achievements { get; set; } = new();

    // 방치 보상 계산에 사용하는 마지막 수령 시간과 잔여 보상 상태
    public IdleRewardSaveData IdleReward { get; set; } = new();
}
