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

    // 재화 저장 데이터
    public CurrencySaveData Currency { get; set; } = new();
}