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
    [SerializeField] private MainBottomPanelController mainBottomPanelController;

    [Header("패널 연출")]
    [SerializeField] private RectTransform equipmentContentRect;
    [SerializeField] private float moveDistance = 100f;
    [SerializeField] private float openDuration = 0.25f;
    [SerializeField] private float closeDuration = 0.2f;

    private Vector2 equipmentContentOriginPosition;
    private Tween panelTween;

    private InventoryController inventoryController;
    private HeroClassType currentClass;
    private Tween classChangeTween;
    private void Awake()
    {
        if (equipmentContentRect != null)
        {
            equipmentContentOriginPosition = equipmentContentRect.anchoredPosition;
        }
    }
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

        Initialize(initialClass);
    }

    // 장비 패널 오픈 연출
    public void PlayOpenAnimation()
    {
        if (equipmentContentRect == null || equipmentContentGroup == null)
        {
            return;
        }

        // 이전 오픈 연출이 남아있으면 중단
        panelTween?.Kill();

        Vector2 startPosition = equipmentContentOriginPosition + Vector2.down * moveDistance;

        // 아래 위치와 투명 상태에서 시작
        equipmentContentRect.anchoredPosition = startPosition;
        equipmentContentGroup.alpha = 0f;
        equipmentContentGroup.interactable = false;
        equipmentContentGroup.blocksRaycasts = false;

        Sequence sequence = DOTween.Sequence();

        // 아래에서 위로 이동하면서 동시에 페이드인
        sequence.Join(equipmentContentRect.DOAnchorPos(equipmentContentOriginPosition, openDuration).SetEase(Ease.OutCubic));
        sequence.Join(equipmentContentGroup.DOFade(1f, openDuration));

        sequence.OnComplete(() =>
        {
            // 다음 오픈을 위해 장비 UI 상태 복원
            equipmentContentRect.anchoredPosition = equipmentContentOriginPosition;
            equipmentContentGroup.alpha = 1f;
            equipmentContentGroup.interactable = true;
            equipmentContentGroup.blocksRaycasts = true;
        });

        panelTween = sequence;
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
        panelTween?.Kill();

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

    // 장비 패널 종료 후 메인 하단 메뉴로 복귀
    private void HandleBackButtonClicked()
    {
        if (equipmentContentRect == null || equipmentContentGroup == null || equipmentRoot == null)
        {
            return;
        }

        // 실행 중인 장비 UI 연출 종료
        panelTween?.Kill();
        classChangeTween?.Kill();

        equipmentContentGroup.interactable = false;
        equipmentContentGroup.blocksRaycasts = false;

        Vector2 closePosition = equipmentContentOriginPosition + Vector2.down * moveDistance;

        Sequence sequence = DOTween.Sequence();

        // 아래로 이동하면서 동시에 페이드아웃
        sequence.Join(equipmentContentRect.DOAnchorPos(closePosition, closeDuration).SetEase(Ease.InCubic));
        sequence.Join(equipmentContentGroup.DOFade(0f, closeDuration));

        sequence.OnComplete(() =>
        {
            equipmentRoot.SetActive(false);

            // 다음 오픈을 위해 장비 UI 상태 복원
            equipmentContentRect.anchoredPosition = equipmentContentOriginPosition;
            equipmentContentGroup.alpha = 1f;
            equipmentContentGroup.interactable = true;
            equipmentContentGroup.blocksRaycasts = true;

            // 하단 메뉴 선택 상태 초기화 후 다시 표시
            if (mainBottomPanelController != null)
            {
                mainBottomPanelController.ResetSelectedMenu();
                mainBottomPanelController.gameObject.SetActive(true);
            }
        });

        panelTween = sequence;
    }
}