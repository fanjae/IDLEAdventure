using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "HeroDatabase", menuName = "Game Data/Unit/Hero Database")]
public sealed class HeroDatabaseSO : ScriptableObject
{
    // 전체 영웅 원본 데이터
    [SerializeField] private List<HeroData> heroes = new();

    // UnitID 기반 런타임 조회용 Dictionary
    private Dictionary<string, HeroData> heroMap;

    // 외부에 저장할 전체 영웅 목록
    public IReadOnlyList<HeroData> Heroes => heroes;

    // Heroes 목록을 UnitID 기준 Dictionary로 변환
    private void Initialize()
    {
        heroMap = new Dictionary<string, HeroData>(StringComparer.Ordinal);

        foreach (HeroData hero in heroes)
        {
            if (hero == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(hero.UnitID))
            {
                Debug.LogError($"[HeroDatabaseSO] UnitID가 비어 있습니다. Hero : {hero.name}", hero);
                continue;
            }

            // 중복 데이터가 기존 데이터를 덮어쓰지 않도록 방지
            if (!heroMap.TryAdd(hero.UnitID, hero))
            {
                Debug.LogError($"[HeroDatabaseSO] 중복된 UnitID가 있습니다. UnitID: {hero.UnitID}, Hero: {hero.name}", hero);
            }
        }
    }

    // UnitID에 해당하는 영웅 원본 데이터 조회
    public bool TryGetHero(string heroId, out HeroData hero)
    {
        EnsureInitialized();

        if(string.IsNullOrEmpty(heroId))
        {
            hero = null;
            return false;
        }

        return heroMap.TryGetValue(heroId, out hero);
    }

    // heroMap이 아직 생성되지 않은 경우에만 Initialize 호출
    private void EnsureInitialized()
    {
        if(heroMap == null)
        {
            Initialize();
        }
    }
}