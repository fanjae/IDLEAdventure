using System;
using System.Collections.Generic;

// 영웅 레벨 공명 관련 기능을 처리하는 컨트롤러
public sealed class ResonanceController
{
    private const int MaxResonanceSlotCount = 4;

    private readonly HeroController heroController;
    private readonly List<string> resonanceSlotHeroIds = new();

    // 현재 공명 슬롯 영웅 ID 목록 반환
    public IReadOnlyList<string> ResonanceSlotHeroIds => resonanceSlotHeroIds;

    // 공명 슬롯 구성 변경 알림
    public event Action OnResonanceSlotChanged;

    public ResonanceController(HeroController heroController)
    {
        if (heroController == null)
        {
            throw new ArgumentNullException(nameof(heroController));
        }

        this.heroController = heroController;

        // 공명 슬롯 영웅 레벨 변경 시 공명 레벨 재적용
        heroController.OnHeroLevelChanged += HandleHeroLevelChanged;

        // 공명 활성 상태에서 신규 영웅 획득 시 공명 레벨 적용
        heroController.OnHeroCollectionChanged += HandleHeroCollectionChanged;
    }

    // 보유 중인 영웅을 공명 슬롯에 등록
    public bool TryAddResonanceSlotHero(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            return false;
        }

        // 보유하지 않은 영웅은 등록하지 않음
        if (!heroController.ContainsHero(heroId))
        {
            return false;
        }

        // 이미 등록된 영웅은 중복 등록하지 않음
        if (resonanceSlotHeroIds.Contains(heroId))
        {
            return false;
        }

        // 공명 슬롯은 최대 4명까지 등록
        if (resonanceSlotHeroIds.Count >= MaxResonanceSlotCount)
        {
            return false;
        }

        resonanceSlotHeroIds.Add(heroId);

        // 슬롯 4명이 모두 등록된 경우 공명 레벨 적용
        ApplyResonanceLevel();

        // 공명 슬롯 변경 알림
        OnResonanceSlotChanged?.Invoke();

        return true;
    }

    // 공명 슬롯 영웅 등록 해제
    public bool TryRemoveResonanceSlotHero(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            return false;
        }

        if (!resonanceSlotHeroIds.Remove(heroId))
        {
            return false;
        }

        // 공명 슬롯 변경 알림
        OnResonanceSlotChanged?.Invoke();

        return true;
    }

    // 지정한 영웅이 공명 슬롯에 등록되어 있는지 확인
    public bool ContainsResonanceSlotHero(string heroId)
    {
        return resonanceSlotHeroIds.Contains(heroId);
    }

    // 공명 슬롯 4명의 실제 레벨 중 가장 낮은 레벨 반환
    public bool TryGetResonanceLevel(out int resonanceLevel)
    {
        resonanceLevel = 0;

        // 슬롯 4명이 모두 등록된 경우에만 공명 활성화
        if (resonanceSlotHeroIds.Count != MaxResonanceSlotCount)
        {
            return false;
        }

        int minLevel = int.MaxValue;

        foreach (string heroId in resonanceSlotHeroIds)
        {
            // 등록된 기준 영웅의 현재 보유 데이터 조회
            if (!heroController.TryGetHero(heroId, out OwnedHeroData hero))
            {
                return false;
            }

            if (hero.Level < minLevel)
            {
                minLevel = hero.Level;
            }
        }

        resonanceLevel = minLevel;
        return true;
    }

    // 현재 공명 레벨보다 낮은 보유 영웅의 실제 레벨 동기화
    public bool ApplyResonanceLevel()
    {
        if (!TryGetResonanceLevel(out int resonanceLevel))
        {
            return false;
        }

        foreach (OwnedHeroData hero in heroController.Heroes)
        {
            // 공명 슬롯 영웅은 자신의 현재 레벨 유지
            if (resonanceSlotHeroIds.Contains(hero.HeroId))
            {
                continue;
            }

            // 공명 레벨 이상인 영웅은 변경하지 않음
            if (hero.Level >= resonanceLevel)
            {
                continue;
            }

            heroController.TrySetHeroLevel(hero.HeroId, resonanceLevel);
        }

        return true;
    }

    // 보유 영웅 레벨 변경 시 공명 레벨 갱신
    private void HandleHeroLevelChanged(OwnedHeroData hero)
    {
        if (hero == null)
        {
            return;
        }

        // 공명 슬롯 영웅의 레벨 변경만 공명 레벨에 영향
        if (!resonanceSlotHeroIds.Contains(hero.HeroId))
        {
            return;
        }

        ApplyResonanceLevel();
    }

    // 보유 영웅 목록 변경 시 현재 공명 레벨 적용
    private void HandleHeroCollectionChanged()
    {
        ApplyResonanceLevel();
    }

    // 현재 공명 상태를 저장 데이터에 반영
    public void WriteSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        saveData.Resonance = new ResonanceSaveData
        {
            ResonanceSlotHeroIds = new List<string>(resonanceSlotHeroIds)
        };
    }

    // 저장 데이터를 기준으로 공명 상태 복원
    public void LoadSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        // 저장된 공명 데이터가 없는 경우 기본 데이터 사용
        saveData.Resonance ??= new ResonanceSaveData();
        saveData.Resonance.ResonanceSlotHeroIds ??= new List<string>();

        // 현재 공명 슬롯 상태 초기화
        resonanceSlotHeroIds.Clear();

        foreach (string heroId in saveData.Resonance.ResonanceSlotHeroIds)
        {
            // 공명 슬롯은 최대 4명까지만 복원
            if (resonanceSlotHeroIds.Count >= MaxResonanceSlotCount)
            {
                break;
            }

            if (string.IsNullOrEmpty(heroId))
            {
                continue;
            }

            // 현재 보유 중인 영웅만 공명 슬롯에 복원
            if (!heroController.ContainsHero(heroId))
            {
                continue;
            }

            // 중복된 공명 슬롯 데이터는 제외
            if (resonanceSlotHeroIds.Contains(heroId))
            {
                continue;
            }

            resonanceSlotHeroIds.Add(heroId);
        }

        // 복원된 공명 슬롯을 기준으로 보유 영웅 레벨 동기화
        ApplyResonanceLevel();

        // 복원된 공명 슬롯 변경 알림
        OnResonanceSlotChanged?.Invoke();
    }
}