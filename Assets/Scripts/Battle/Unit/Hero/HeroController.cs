using System;
using System.Collections.Generic;

// 보유 영웅 관련 기능을 처리하는 컨트롤러
// 게임 전역에서는 HeroManager를 통해 접근
public sealed class HeroController
{
    private readonly HeroDatabaseSO heroDatabase;
    private readonly HeroCollection heroCollection;
    private readonly HeroStatCalculator statCalculator;

    // 현재 보유 중인 영웅 목록 반환
    public IReadOnlyCollection<OwnedHeroData> Heroes => heroCollection.Heroes;

    // 보유 영웅 목록 변경시 이벤트 호출
    public event Action OnHeroCollectionChanged;

    // 보유 영웅 레벨 변경 시 호출
    public event Action<OwnedHeroData> OnHeroLevelChanged;

    // 영웅 최종 능력치 변경 시 호출
    public event Action OnHeroStatChanged;

    public HeroController(HeroDatabaseSO heroDatabase, HeroStatCalculator statCalculator)
    {
        if (heroDatabase == null)
        {
            throw new ArgumentNullException(nameof(heroDatabase));
        }

        if (statCalculator == null)
        {
            throw new ArgumentNullException(nameof(statCalculator));
        }

        this.heroDatabase = heroDatabase;
        this.statCalculator = statCalculator;
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

    // UnitID에 해당하는 보유 영웅의 최종 능력치 계산
    public bool TryGetHeroStat(string heroId, out HeroStat stat)
    {
        stat = default;

        if (!heroCollection.TryGet(heroId, out OwnedHeroData hero))
        {
            return false;
        }

        stat = statCalculator.Calculate(hero);
        return true;
    }

    // 지정한 적용 레벨을 기준으로 보유 영웅의 최종 능력치 계산
    public bool TryGetHeroStat(string heroId, int appliedLevel, out HeroStat stat)
    {
        stat = default;

        if (appliedLevel < 1)
        {
            return false;
        }

        if (!heroCollection.TryGet(heroId, out OwnedHeroData hero))
        {
            return false;
        }

        stat = statCalculator.Calculate(hero, appliedLevel);
        return true;
    }

    // 영웅 최종 능력치 변경 이벤트 호출
    public void NotifyStatChanged()
    {
        OnHeroStatChanged?.Invoke();
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
        OnHeroStatChanged?.Invoke();

        return true;
    }

    // 현재 보유 영웅 상태를 저장 데이터에 반영
    public void WriteSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        saveData.Heroes = heroCollection.CreateSaveData();
    }

    // 저장 데이터를 기준으로 보유 영웅 상태 복원
    public void LoadSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        // 저장된 영웅 데이터가 없는 경우 기본 데이터 사용
        saveData.Heroes ??= new HeroSaveData();
        saveData.Heroes.OwnedHeroes ??= new List<OwnedHeroSaveData>();

        // 현재 보유 영웅 상태 초기화
        heroCollection.Clear();

        foreach (OwnedHeroSaveData heroSaveData in saveData.Heroes.OwnedHeroes)
        {
            if (heroSaveData == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(heroSaveData.HeroId))
            {
                continue;
            }

            if (heroSaveData.Level < 1)
            {
                continue;
            }

            // 저장된 UnitID를 기준으로 영웅 원본 데이터 조회
            if (!heroDatabase.TryGetHero(heroSaveData.HeroId, out HeroData heroData))
            {
                continue;
            }

            // 저장된 레벨을 적용하여 보유 영웅 데이터 복원
            heroCollection.TryAdd(heroData, heroSaveData.Level);
        }

        // 보유 영웅 목록 변경 이벤트 호출
        OnHeroCollectionChanged?.Invoke();
    }


}