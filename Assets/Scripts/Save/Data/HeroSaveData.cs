using System;
using System.Collections.Generic;

// 보유 영웅 전체 저장 데이터

[Serializable]
public sealed class HeroSaveData
{
    // 플레이어가 보유한 영웅 저장 목록
    public List<OwnedHeroSaveData> OwnedHeroes { get; set; } = new();
}

// 보유 영웅 한 명의 저장 데이터
[Serializable]
public sealed class OwnedHeroSaveData
{
    // HeroData의 UnitId.
    public string HeroId { get; set; }

    // 보유 영웅의 현재 레벨
    public int Level { get; set; }
}
