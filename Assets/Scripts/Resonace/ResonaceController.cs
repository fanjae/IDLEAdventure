using System;
using System.Collections.Generic;

// 영웅 레벨 공명 관련 기능을 처리하는 컨트롤러
public sealed class ResonanceController
{
    private readonly HeroController heroController;
    private readonly List<string> coreHeroIds = new();
    private readonly List<string> slotHeroIds = new();

    // 현재 공명 기준 영웅 ID 목록 반환
    public IReadOnlyList<string> CoreHeroIds => coreHeroIds;

    // 현재 공명 슬롯 영웅 ID 목록 반환
    public IReadOnlyList<string> SlotHeroIds => slotHeroIds;

    public ResonanceController(HeroController heroController)
    {
        if (heroController == null)
        {
            throw new ArgumentNullException(nameof(heroController));
        }

        this.heroController = heroController;
    }

    // 보유 중인 영웅을 공명 기준 영웅으로 등록
    public bool TryAddCoreHero(string heroId)
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
        if (coreHeroIds.Contains(heroId))
        {
            return false;
        }

        coreHeroIds.Add(heroId);
        return true;
    }

    // 공명 기준 영웅 등록 해제
    public bool TryRemoveCoreHero(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            return false;
        }

        return coreHeroIds.Remove(heroId);
    }

    // 지정한 영웅이 공명 기준 영웅인지 확인
    public bool ContainsCoreHero(string heroId)
    {
        return coreHeroIds.Contains(heroId);
    }

    // 현재 공명 기준 영웅 중 가장 낮은 레벨 반환
    public bool TryGetResonanceLevel(out int resonanceLevel)
    {
        resonanceLevel = 0;

        if (coreHeroIds.Count == 0)
        {
            return false;
        }

        int minLevel = int.MaxValue;

        foreach (string heroId in coreHeroIds)
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

    // 보유 중인 영웅을 공명 슬롯에 등록
    public bool TryAddSlotHero(string heroId)
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

        // 공명 기준 영웅은 슬롯에 등록하지 않음
        if (coreHeroIds.Contains(heroId))
        {
            return false;
        }

        // 이미 등록된 영웅은 중복 등록하지 않음
        if (slotHeroIds.Contains(heroId))
        {
            return false;
        }

        slotHeroIds.Add(heroId);
        return true;
    }

    // 공명 슬롯 영웅의 공명 레벨 기준 최종 능력치 계산
    public bool TryGetResonanceHeroStat(string heroId, out HeroStat stat)
    {
        stat = default;

        if (string.IsNullOrEmpty(heroId))
        {
            return false;
        }

        // 공명 슬롯에 등록되지 않은 영웅은 계산하지 않음
        if (!slotHeroIds.Contains(heroId))
        {
            return false;
        }

        // 현재 기준 영웅을 기준으로 공명 레벨 계산
        if (!TryGetResonanceLevel(out int resonanceLevel))
        {
            return false;
        }

        return heroController.TryGetHeroStat(heroId, resonanceLevel, out stat);
    }

    // 공명 슬롯 영웅 등록 해제
    public bool TryRemoveSlotHero(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            return false;
        }

        return slotHeroIds.Remove(heroId);
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
            CoreHeroIds = new List<string>(coreHeroIds),
            SlotHeroIds = new List<string>(slotHeroIds)
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
        saveData.Resonance.CoreHeroIds ??= new List<string>();
        saveData.Resonance.SlotHeroIds ??= new List<string>();


        // 현재 공명 상태 초기화
        coreHeroIds.Clear();
        slotHeroIds.Clear();


        foreach (string heroId in saveData.Resonance.CoreHeroIds)
        {
            if (string.IsNullOrEmpty(heroId))
            {
                continue;
            }


            // 현재 보유 중인 영웅만 기준 영웅으로 복원
            if (!heroController.ContainsHero(heroId))
            {
                continue;
            }


            // 중복된 기준 영웅 데이터는 제외
            if (coreHeroIds.Contains(heroId))
            {
                continue;
            }


            coreHeroIds.Add(heroId);
        }


        foreach (string heroId in saveData.Resonance.SlotHeroIds)
        {
            if (string.IsNullOrEmpty(heroId))
            {
                continue;
            }


            // 현재 보유 중인 영웅만 공명 슬롯에 복원
            if (!heroController.ContainsHero(heroId))
            {
                continue;
            }


            // 기준 영웅은 공명 슬롯에 복원하지 않음
            if (coreHeroIds.Contains(heroId))
            {
                continue;
            }


            // 중복된 슬롯 영웅 데이터는 제외
            if (slotHeroIds.Contains(heroId))
            {
                continue;
            }


            slotHeroIds.Add(heroId);
        }
    }
}