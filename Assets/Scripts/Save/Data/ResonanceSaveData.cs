using System;
using System.Collections.Generic;

[Serializable]
public sealed class ResonanceSaveData
{
    // 현재 공명 슬롯에 등록된 영웅 ID 목록
    public List<string> ResonanceSlotHeroIds = new();
}