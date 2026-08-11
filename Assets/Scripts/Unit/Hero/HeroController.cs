using System;
using System.Collections.Generic;

// 보유 영웅 관련 기능을 처리하는 컨트롤러
// 게임 전역에서는 HeroManager를 통해 접근
public sealed class HeroController
{
    private readonly HeroDatabaseSO heroDatabase;
    private readonly HeroCollection heroCollection;

    // 현재 보유 중인 영웅 목록 반환
    public IReadOnlyCollection<OwnedHeroData> Heroes => heroCollection.Heroes;

    // 보유 영웅 목록 변경시 이벤트 호출
    public event Action OnHeroCollectionChanged;

    // 보유 영웅 레벨 변경 시 호출
    public event Action<OwnedHeroData> OnHeroLevelChanged;

    public HeroController(HeroDatabaseSO heroDatabase)
    {
        if (heroDatabase == null)
        {
            throw new ArgumentNullException(nameof(heroDatabase));
        }

        this.heroDatabase = heroDatabase;
        heroCollection = new HeroCollection();
    }
    
    // UnitID에 해당하는 영웅을 보유 목록에 추가
    public bool TryAcquireHero(string heroId)
    {
        if(!heroDatabase.TryGetHero(heroId, out HeroData heroData))
        {
            return false;
        }

        if (!heroCollection.TryAdd(heroData))
        {
            return false;
        }

        OnHeroCollectionChanged?.Invoke();
        return true;
    }

    // UnitID에 해당하는 보유 영웅 조회 
    public bool TryGetHero(string heroId, out OwnedHeroData hero)
    {
        return heroCollection.TryGet(heroId, out hero);
    }

    // 지정한 영웅을 보유하고 있는지 확인
    public bool ContainsHero(string heroId)
    {
        return heroCollection.Contains(heroId);
    }

    // 보유 영웅의 레벨 변경
    public bool TrySetHeroLevel(string heroId, int level)
    {
        if (level < 1)
        {
            return false;
        }

        if (!heroCollection.TryGet(heroId, out OwnedHeroData hero))
        {
            return false;
        }

        if (hero.Level == level)
        {
            return false;
        }

        hero.SetLevel(level);
        OnHeroLevelChanged?.Invoke(hero);

        return true;
    }
}