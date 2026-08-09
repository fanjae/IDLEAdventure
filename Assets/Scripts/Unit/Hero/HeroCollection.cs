
using System;
using System.Collections.Generic;

// 플레이어가 보유한 영웅을 관리하는 클래스
public sealed class HeroCollection
{
    // 영웅은 UnitID를 기준으로 보유 여부와 데이터를 관리
    private readonly Dictionary<string, OwnedHeroData> heroes = new(StringComparer.Ordinal);

    // 현재 보유 중인 영웅 목록 반환
    public IReadOnlyCollection<OwnedHeroData> Heroes => heroes.Values;

    // 영웅 원본 데이터를 기준으로 새로운 보유 영웅 추가
    public bool TryAdd(HeroData heroData)
    {
        if(heroData == null)
        {
            return false;
        }
        
        if(string.IsNullOrEmpty(heroData.UnitID))
        {
            return false;
        }

        // 이미 보유 중인 영우은 중복 추가하지 않음
        if (heroes.ContainsKey(heroData.UnitID))
        {
            return false;
        }

        OwnedHeroData ownedHero = new(heroData);
        heroes.Add(heroData.UnitID, ownedHero);

        return true;
    }

    // UnitID에 해당하는 보유 영웅 조회
    public bool TryGet(string heroId, out OwnedHeroData hero)
    {
        if(string.IsNullOrEmpty(heroId))
        {
            hero = null;
            return false;
        }
        return heroes.TryGetValue(heroId, out hero);
    }

    // 지정한 영웅을 보유하고 있는지 확인
    public bool Contains(string heroId)
    {
        return !string.IsNullOrEmpty(heroId) && heroes.ContainsKey(heroId);
    }
}
