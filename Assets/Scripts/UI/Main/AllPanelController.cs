using System;
using UnityEngine;
using UnityEngine.UI;

// 전체 메뉴 패널의 열기 및 닫기 처리
public sealed class AllPanelController : MonoBehaviour
{
    [Header("공통")]
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject allPanelRoot;
    [SerializeField] private GameObject allMenuRoot;

    [Header("인벤토리")]
    [SerializeField] private Button inventoryButton;
    [SerializeField] private GameObject inventoryPanelRoot;
    [SerializeField] private InventoryPanelController inventoryPanelController;

    [Header("상점")]
    [SerializeField] private Button shopButton;
    [SerializeField] private GameObject shopPanelRoot;
    [SerializeField] private ShopPanelPresenter shopPanelPresenter;

    [Header("퀘스트")]
    [SerializeField] private Button questButton;

    [Header("업적")]
    [SerializeField] private Button achievementButton;
    [SerializeField] private AchievementPanelPresenter achievementPanelPresenter;

    [Header("설정")]
    [SerializeField] private Button settingButton;

    [Header("패널 연출")]
    [SerializeField] private UIPanelTransition panelTransition;

    public event Action OnClosed;

    private void OnEnable()
    {
        if (backButton != null) backButton.onClick.AddListener(HandleBackButtonClicked);
        if (inventoryButton != null) inventoryButton.onClick.AddListener(HandleInventoryButtonClicked);
        if (shopButton != null) shopButton.onClick.AddListener(HandleShopButtonClicked);
        if (shopPanelPresenter != null) shopPanelPresenter.OnClosed += HandleShopPanelClosed;
        if (achievementButton != null) achievementButton.onClick.AddListener(HandleAchievementButtonClicked);
        if (achievementPanelPresenter != null) achievementPanelPresenter.OnClosed += HandleAchievementPanelClosed;
    }

    private void OnDisable()
    {
        if (backButton != null) backButton.onClick.RemoveListener(HandleBackButtonClicked);
        if (inventoryButton != null) inventoryButton.onClick.RemoveListener(HandleInventoryButtonClicked);
        if (shopButton != null) shopButton.onClick.RemoveListener(HandleShopButtonClicked);
        if (shopPanelPresenter != null) shopPanelPresenter.OnClosed -= HandleShopPanelClosed;
        if (achievementButton != null) achievementButton.onClick.RemoveListener(HandleAchievementButtonClicked);
        if (achievementPanelPresenter != null) achievementPanelPresenter.OnClosed -= HandleAchievementPanelClosed;
    }

    // 전체 메뉴 패널 오픈 연출
    public void PlayOpenAnimation()
    {
        panelTransition?.PlayOpen();
    }

    // 인벤토리 버튼 클릭 처리
    private void HandleInventoryButtonClicked()
    {
        if (allMenuRoot == null || inventoryPanelRoot == null)
        {
            return;
        }

        // 전체 메뉴 화면을 숨기고 인벤토리 패널 표시
        allMenuRoot.SetActive(false);
        inventoryPanelRoot.SetActive(true);

        inventoryPanelController?.PlayOpenAnimation();
    }

    // 상점 버튼 클릭 처리
    private void HandleShopButtonClicked()
    {
        if (allMenuRoot == null || shopPanelPresenter == null)
        {
            return;
        }

        // 전체 메뉴 화면을 숨기고 상점 패널 표시
        allMenuRoot.SetActive(false);
        shopPanelPresenter.gameObject.SetActive(true);

        shopPanelPresenter.PlayOpenAnimation();
    }

    // 상점 패널 종료 후 전체 메뉴 화면 복원
    private void HandleShopPanelClosed()
    {
        if (allMenuRoot == null || shopPanelPresenter == null)
        {
            return;
        }

        shopPanelPresenter.gameObject.SetActive(false);
        allMenuRoot.SetActive(true);
    }

    // 업적 버튼 클릭 처리
    private void HandleAchievementButtonClicked()
    {
        if (allMenuRoot == null || achievementPanelPresenter == null)
        {
            return;
        }

        // 전체 메뉴 화면을 숨기고 업적 패널 표시
        allMenuRoot.SetActive(false);
        achievementPanelPresenter.Open();
    }

    // 업적 패널 종료 후 전체 메뉴 화면 복원
    private void HandleAchievementPanelClosed()
    {
        if (allMenuRoot == null)
        {
            return;
        }

        allMenuRoot.SetActive(true);
    }

    // 뒤로가기 버튼 클릭 처리
    private void HandleBackButtonClicked()
    {
        if (allPanelRoot == null)
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

    // 전체 메뉴 패널 종료 처리
    private void ClosePanel()
    {
        allPanelRoot.SetActive(false);
        OnClosed?.Invoke();
    }
}