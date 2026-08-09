
// 영웅 원본 데이터와 플레이어의 보유 영웅 목록을 관리하는 클래스
// 외부에서는 HeroController를 통해 보유 영웅 기능에 접근
using System;
using System.Collections.Generic;

public sealed class HeroController
{
    private readonly HeroDatabaseSO heroDatabase;
    private readonly HeroCollection heroCollection;

    // 현재 보유 중인 영웅 목록 반환
    public IReadOnlyCollection<OwnedHeroData> Heroes => heroCollection.Heroes;

    // 보유 영웅 목록 변경시 이벤트 호출
    public event Action OnHeroCollectionChanged;

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
}