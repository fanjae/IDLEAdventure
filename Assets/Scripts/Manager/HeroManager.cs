using System;
using UnityEngine;

// 게임 전체에서 사용할 HeroController 인스턴스 관리
public sealed class HeroManager : Singleton<HeroManager>
{
    // 실제 보유 영웅 관련 기능을 처리할 컨트롤러
    private HeroController controller;

    // 외부에서 사용할 Controller 반환
    public HeroController Controller
    {
        get
        {
            if (controller == null)
            {
                throw new InvalidOperationException("HeroManager가 초기화되지 않았습니다.");
            }

            return controller;
        }
    }

    // 초기화 체크
    public bool IsInitialized => controller != null;

    // HeroDatabaseSO를 이용해 게임에서 사용할 HeroController 생성
    public void Initialize(HeroDatabaseSO heroDatabase, InventoryController inventoryController)
    {
        if (heroDatabase == null)
        {
            throw new ArgumentNullException(nameof(heroDatabase));
        }

        if (inventoryController == null)
        {
            throw new ArgumentNullException(nameof(inventoryController));
        }

        if (IsInitialized)
        {
            return;
        }

        // 장비 능력치 계산기 생성
        EquipmentStatCalculator equipmentStatCalculator = new(inventoryController);

        // 영웅 최종 능력치 계산기 생성
        HeroStatCalculator heroStatCalculator = new(equipmentStatCalculator);

        // 영웅 관련 기능을 처리할 컨트롤러 생성
        controller = new HeroController(heroDatabase, heroStatCalculator);

        // 장비 변경시 영웅 최종 능력치 전달
        inventoryController.OnEquipmentChanged += controller.NotifyStatChanged;

        // 보유 영웅 상태를 저장 대상으로 등록
        SaveManager.Instance.RegisterWriter(controller);
    }

    // 보유 영웅 제거 및 관련 참조 정리
    public bool TryRemoveOwnedHero(string heroId)
    {
        if (!IsInitialized || string.IsNullOrEmpty(heroId))
        {
            return false;
        }

        if (!controller.ContainsHero(heroId))
        {
            return false;
        }

        if (ResonanceManager.TryGetExistingInstance(out ResonanceManager resonanceManager) && resonanceManager.IsInitialized)
        {
            if (resonanceManager.Controller.ContainsResonanceSlotHero(heroId))
            {
                resonanceManager.Controller.TryRemoveResonanceSlotHero(heroId);
            }
        }

        return controller.TryRemoveHero(heroId);
    }

    protected override void OnDestroy()
    {
        if (controller != null && SaveManager.TryGetExistingInstance(out SaveManager saveManager))
        {
            saveManager.UnregisterWriter(controller);
        }

        base.OnDestroy();
    }
}