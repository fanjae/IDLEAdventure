using System;
using UnityEngine;

[Serializable]
public sealed class GameSaveData
{
    public int Version { get; set; } = 1;
    public long SavedAtUnixTime { get; set; }

    public InventorySaveData Inventory { get; set; } = new();
    public EquipmentSaveData Equipment { get; set; } = new();
}