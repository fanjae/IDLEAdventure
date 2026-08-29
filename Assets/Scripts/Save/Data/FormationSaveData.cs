using System;
using System.Collections.Generic;

// 영웅 배치 저장 데이터
[Serializable]
public sealed class FormationSaveData
{
    public List<FormationSlotSaveData> Slots { get; set; } = new();
}

// 영웅이 배치된 슬롯 저장 데이터
[Serializable]
public sealed class FormationSlotSaveData
{
    public int SlotNumber { get; set; }
    public string HeroId { get; set; }
}