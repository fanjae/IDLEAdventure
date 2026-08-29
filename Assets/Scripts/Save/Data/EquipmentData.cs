using System;
using System.Collections.Generic;

[Serializable]
public sealed class EquipmentSaveData
{
    public List<ClassEquipmentSaveData> Classes { get; set; } = new();
}

[Serializable]
public sealed class ClassEquipmentSaveData
{
    public HeroClassType HeroClass { get; set; }
    public Dictionary<EquipmentSlotType, string> EquippedInstanceIds { get; set; } = new();
}