using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 장비 패널의 상태 갱신과 입력 연결 처리
public class EquipmentPanelController : MonoBehaviour
{
    [SerializeField] private EquipmentPanelView panelView;
    [SerializeField] private EquipmentClassSelectorView classSelectorView;
    [SerializeField] private CanvasGroup equipmentContentGroup;
    [SerializeField] private HeroClassType initialClass = HeroClassType.Support;

    [Header("패널 이동")]
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject equipmentRoot;

    [Header("정보 패널")]
    [SerializeField] private Button infoButton;
    [SerializeField] private Button infoCloseButton;
    [SerializeField] private GameObject infoPanel;

    [Header("패널 연출")]
    [SerializeField] private UIPanelTransition panelTransition;
    [SerializeField] private UIPanelTransition infoPanelTransition;

    private InventoryController inventoryController;
    private HeroClassType currentClass;
    private Tween classChangeTween;

    public event Action OnClosed;

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

        if (backButton != null)
        {
            backButton.onClick.AddListener(HandleBackButtonClicked);
        }

        if (infoButton != null)
        {
            infoButton.onClick.AddListener(HandleInfoButtonClicked);
        }

        if (infoCloseButton != null)
        {
            infoCloseButton.onClick.AddListener(HandleInfoCloseButtonClicked);
        }

        Initialize(initialClass);
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

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(HandleBackButtonClicked);
        }

        if (infoButton != null)
        {
            infoButton.onClick.RemoveListener(HandleInfoButtonClicked);
        }

        if (infoCloseButton != null)
        {
            infoCloseButton.onClick.RemoveListener(HandleInfoCloseButtonClicked);
        }

        if (inventoryController == null)
        {
            return;
        }

        inventoryController.OnInventoryChanged -= Refresh;
        inventoryController.OnEquipmentChanged -= Refresh;
    }

    // 장비 패널 오픈 연출
    public void PlayOpenAnimation()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        panelTransition?.PlayOpen();
    }

    // 장비 패널에서 사용할 현재 클래스 설정
    public void Initialize(HeroClassType heroClass)
    {
        if (InventoryManager.Instance == null || !InventoryManager.Instance.IsInitialized)
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

        // Fade 대상이 없으면 바로 클래스를 변경
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

    // 장비 패널 닫기
    private void HandleBackButtonClicked()
    {
        if (equipmentRoot == null)
        {
            return;
        }

        classChangeTween?.Kill();

        if (infoPanel != null && infoPanel.activeSelf)
        {
            infoPanelTransition?.PlayClose();
        }

        if (panelTransition == null)
        {
            ClosePanel();
            return;
        }

        panelTransition.PlayClose(ClosePanel);
    }

    // 장비 패널 종료 처리
    private void ClosePanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        equipmentRoot.SetActive(false);
        OnClosed?.Invoke();
    }

    // 장비 도움말 열기
    private void HandleInfoButtonClicked()
    {
        if (infoPanel == null)
        {
            return;
        }

        infoPanel.SetActive(true);

        if (infoPanelTransition == null)
        {
            return;
        }

        infoPanelTransition.PlayOpen();
    }

    // 장비 도움말 패널 닫기
    private void HandleInfoCloseButtonClicked()
    {
        CloseInfoPanel();
    }

    // 장비 도움말 패널 종료 처리
    private void CloseInfoPanel()
    {
        if (infoPanel == null || !infoPanel.activeSelf)
        {
            return;
        }

        if (infoPanelTransition == null)
        {
            infoPanel.SetActive(false);
            return;
        }

        infoPanelTransition.PlayClose(() => infoPanel.SetActive(false));
    }
}