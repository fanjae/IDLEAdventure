using UnityEngine;
using UnityEngine.UI;

// 인벤토리 패널의 화면 전환 관리
public sealed class InventoryPanelController : MonoBehaviour
{
    [Header("공통")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button decompositeButton;
    [SerializeField] private GameObject inventoryPanelRoot;

    [Header("전체 메뉴")]
    [SerializeField] private GameObject allMenuRoot;

    [Header("패널 연출")]
    [SerializeField] private UIPanelTransition panelTransition;
    [SerializeField] private InventoryPanelPresenter presenter;

    private void OnEnable()
    {
        if (backButton != null) backButton.onClick.AddListener(HandleBackButtonClicked);
        if (decompositeButton != null) decompositeButton.onClick.AddListener(HandleDecompositeButtonClicked);
    }

    private void OnDisable()
    {
        if (backButton != null) backButton.onClick.RemoveListener(HandleBackButtonClicked);
        if (decompositeButton != null) decompositeButton.onClick.RemoveListener(HandleDecompositeButtonClicked);
    }

    // 미장착 장비 일괄 분해
    private void HandleDecompositeButtonClicked()
    {
        presenter?.DecomposeUnequippedEquipment();
    }

    // 인벤토리 패널 오픈 연출
    public void PlayOpenAnimation()
    {
        if (panelTransition == null)
        {
            HandleOpenAnimationCompleted();
            return;
        }

        panelTransition.PlayOpen(HandleOpenAnimationCompleted);
    }

    // 인벤토리 패널 오픈 완료 처리
    private void HandleOpenAnimationCompleted()
    {
        presenter?.PlaySlotAnimations();
    }

    // 뒤로가기 버튼 클릭 처리
    private void HandleBackButtonClicked()
    {
        if (inventoryPanelRoot == null)
        {
            return;
        }

        if (panelTransition == null)
        {
            ClosePanel();
            return;
        }

        panelTransition.PlayClose(ClosePanel);
    }

    // 인벤토리 패널 종료 처리
    private void ClosePanel()
    {
        inventoryPanelRoot.SetActive(false);

        if (allMenuRoot != null)
        {
            allMenuRoot.SetActive(true);
        }
    }
}
