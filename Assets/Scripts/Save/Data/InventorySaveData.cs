using System;
using System.Collections.Generic;

[Serializable]
public sealed class InventorySaveData
{
    public List<InventoryItemSaveData> Items { get; set; } = new();
    public List<OwnedEquipmentSaveData> Equipments { get; set; } = new();
}

[Serializable]
public sealed class InventoryItemSaveData
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
}

[Serializable]
public sealed class OwnedEquipmentSaveData
{
    public string InstanceId { get; set; }
    public int EquipmentId { get; set; }
    public int EnhancementLevel { get; set; }
}