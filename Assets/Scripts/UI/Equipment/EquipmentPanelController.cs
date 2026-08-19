using DG.Tweening;
using UnityEngine;

// 장비 패널의 상태 갱신과 입력 연결 처리
public class EquipmentPanelController : MonoBehaviour
{
    [SerializeField] private EquipmentPanelView panelView;
    [SerializeField] private EquipmentClassSelectorView classSelectorView;
    [SerializeField] private CanvasGroup equipmentContentGroup;
    [SerializeField] private HeroClassType initialClass = HeroClassType.Support;

    private InventoryController inventoryController;
    private HeroClassType currentClass;
    private Tween classChangeTween;

    private void Start()
    {
        if (panelView != null)
        {
            panelView.OnAutoEquipClicked += HandleAutoEquipClicked;
        }

        if (classSelectorView != null)
        {
            classSelectorView.OnClassSelected += HandleClassSelected;
        }

        Initialize(initialClass);
    }

    // 장비 패널에서 사용할 현재 클래스 설정
    public void Initialize(HeroClassType heroClass)
    {
        if (!InventoryManager.Instance.IsInitialized)
        {
            return;
        }

        if (inventoryController != null)
        {
            inventoryController.OnInventoryChanged -= Refresh;
            inventoryController.OnEquipmentChanged -= Refresh;
        }

        inventoryController = InventoryManager.Instance.Controller;
        currentClass = heroClass;

        classSelectorView?.UpdateSelectedButton(heroClass);
        panelView?.SetClassTitle(heroClass);

        inventoryController.OnInventoryChanged += Refresh;
        inventoryController.OnEquipmentChanged += Refresh;

        Refresh();
    }

    // 장비 패널에서 표시할 클래스 변경
    public void SetClass(HeroClassType heroClass)
    {
        currentClass = heroClass;

        if (panelView != null)
        {
            panelView.SetClassTitle(heroClass);
        }
        Refresh();
    }

    // 현재 장비 상태를 기준으로 UI 갱신
    public void Refresh()
    {
        if (inventoryController == null || panelView == null)
        {
            return;
        }

        bool canAutoEquip = inventoryController.HasBetterEquippableEquipment(currentClass);
        panelView.SetAutoEquipAvailable(canAutoEquip);

        RefreshEquipmentSlot(EquipmentSlotType.Weapon);
        RefreshEquipmentSlot(EquipmentSlotType.Hands);
        RefreshEquipmentSlot(EquipmentSlotType.Accessory);
        RefreshEquipmentSlot(EquipmentSlotType.Head);
        RefreshEquipmentSlot(EquipmentSlotType.Body);
        RefreshEquipmentSlot(EquipmentSlotType.Legs);
    }

    private void OnDestroy()
    {
        classChangeTween?.Kill();

        if (panelView != null)
        {
            panelView.OnAutoEquipClicked -= HandleAutoEquipClicked;
        }

        if (classSelectorView != null)
        {
            classSelectorView.OnClassSelected -= HandleClassSelected;
        }

        if (inventoryController == null)
        {
            return;
        }

        inventoryController.OnInventoryChanged -= Refresh;
        inventoryController.OnEquipmentChanged -= Refresh;
    }

    // 일괄 장착 버튼 클릭 처리
    private void HandleAutoEquipClicked()
    {
        if (inventoryController == null)
        {
            return;
        }

        inventoryController.TryAutoEquipBetterEquipment(currentClass);
    }

    // 지정한 슬롯의 현재 장착 장비 표시 갱신
    private void RefreshEquipmentSlot(EquipmentSlotType slotType)
    {
        if (inventoryController.TryGetEquippedEquipment(currentClass, slotType, out EquipmentSO equipment))
        {
            panelView.SetEquipmentSlot(slotType, equipment);
            return;
        }

        panelView.SetEquipmentSlot(slotType, null);
    }

    // 선택한 클래스의 장비 패널로 변경
    private void HandleClassSelected(HeroClassType heroClass)
    {
        if (heroClass == currentClass)
        {
            return;
        }

        // Fade 대상 없으면 바로 클래스를 변경
        if (equipmentContentGroup == null)
        {
            SetClass(heroClass);
            return;
        }

        // 이전 클래스 변경 연출이 남아있으면 중단
        classChangeTween?.Kill();

        // Fade 처리 중 장비 영역 입력 방지
        equipmentContentGroup.interactable = false;
        equipmentContentGroup.blocksRaycasts = false;

        // 기존 장비 UI를 숨긴 뒤 클래스 정보 갱신
        classChangeTween = equipmentContentGroup.DOFade(0f, 0.15f)
            .OnComplete(() =>
            {
                SetClass(heroClass);

                // 갱신된 클래스 장비 UI 표시
                classChangeTween = equipmentContentGroup.DOFade(1f, 0.15f)
                    .OnComplete(() =>
                    {
                        equipmentContentGroup.interactable = true;
                        equipmentContentGroup.blocksRaycasts = true;
                    });
            });
    }
}