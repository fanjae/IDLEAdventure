using UnityEngine;
using UnityEngine.UI;

// 메인 화면 하단 메뉴 UI 관리
public sealed class MainBottomPanelController : MonoBehaviour
{
    private enum BottomMenuType
    {
        None,
        Hero,
        Equipment,
        Gacha,
        All
    }

    [Header("공명 패널")]
    [SerializeField] private GameObject resonanceHeroContentPanel;
    [SerializeField] private ResonancePanelController resonancePanelController;

    [Header("장비 패널")]
    [SerializeField] private GameObject equipmentRoot;
    [SerializeField] private EquipmentPanelController equipmentPanelController;

    [Header("소집 패널")]
    [SerializeField] private GameObject gachaRoot;
    [SerializeField] private UIPanelTransition gachaPanelTransition;

    [Header("전체 메뉴 패널")]
    [SerializeField] private GameObject allPanelRoot;
    [SerializeField] private AllPanelController allPanelController;

    [Header("자동 스테이지")]
    [SerializeField] private Button autoStageButton;
    [SerializeField] private AutoStagePanelController autoStagePanelController;

    [Header("하단 메뉴")]
    [SerializeField] private BottomMenuButtonData heroMenu;
    [SerializeField] private BottomMenuButtonData equipmentMenu;
    [SerializeField] private BottomMenuButtonData gachaMenu;
    [SerializeField] private BottomMenuButtonData allMenu;

    [Header("퀘스트 패널")]
    [SerializeField] private QuestPanel questPanel;

    private BottomMenuType selectedMenu = BottomMenuType.None;

    private void Awake()
    {
        // 씬 진입 시 모든 메뉴를 기본 상태로 초기화
        SetSelectedMenu(BottomMenuType.None);

        if (resonancePanelController != null)
        {
            resonancePanelController.OnClosed += HandleChildPanelClosed;
        }

        if (equipmentPanelController != null)
        {
            equipmentPanelController.OnClosed += HandleChildPanelClosed;
        }

        if (autoStagePanelController != null)
        {
            autoStagePanelController.OnClosed += HandleChildPanelClosed;
        }

        if (allPanelController != null)
        {
            allPanelController.OnClosed += HandleChildPanelClosed;
        }

        if (questPanel != null)
        {
            questPanel.OnMainClosed += HandleQuestPanelClosed;
        }
    }

    private void OnEnable()
    {
        if (autoStageButton != null) autoStageButton.onClick.AddListener(HandleAutoStageButtonClicked);
        if (heroMenu?.Button != null) heroMenu.Button.onClick.AddListener(HandleHeroButtonClicked);
        if (equipmentMenu?.Button != null) equipmentMenu.Button.onClick.AddListener(HandleEquipmentButtonClicked);
        if (gachaMenu?.Button != null) gachaMenu.Button.onClick.AddListener(HandleGachaButtonClicked);
        if (allMenu?.Button != null) allMenu.Button.onClick.AddListener(HandleAllButtonClicked);
    }

    private void OnDisable()
    {
        if (autoStageButton != null) autoStageButton.onClick.RemoveListener(HandleAutoStageButtonClicked);
        if (heroMenu?.Button != null) heroMenu.Button.onClick.RemoveListener(HandleHeroButtonClicked);
        if (equipmentMenu?.Button != null) equipmentMenu.Button.onClick.RemoveListener(HandleEquipmentButtonClicked);
        if (gachaMenu?.Button != null) gachaMenu.Button.onClick.RemoveListener(HandleGachaButtonClicked);
        if (allMenu?.Button != null) allMenu.Button.onClick.RemoveListener(HandleAllButtonClicked);
    }

    private void OnDestroy()
    {
        if (resonancePanelController != null)
        {
            resonancePanelController.OnClosed -= HandleChildPanelClosed;
        }

        if (equipmentPanelController != null)
        {
            equipmentPanelController.OnClosed -= HandleChildPanelClosed;
        }

        if (autoStagePanelController != null)
        {
            autoStagePanelController.OnClosed -= HandleChildPanelClosed;
        }

        if (allPanelController != null)
        {
            allPanelController.OnClosed -= HandleChildPanelClosed;
        }

        if (questPanel != null)
        {
            questPanel.OnMainClosed -= HandleQuestPanelClosed;
        }
    }

    // 자동 스테이지 패널 표시
    private void HandleAutoStageButtonClicked()
    {
        if (autoStagePanelController == null)
        {
            return;
        }

        autoStagePanelController.Open();
        gameObject.SetActive(false);
    }

    // 모험단 메뉴 선택
    private void HandleHeroButtonClicked()
    {
        SetSelectedMenu(BottomMenuType.Hero);

        if (resonanceHeroContentPanel == null)
        {
            return;
        }

        // 공명 영웅 목록 패널 활성화 후 기존 하단 메뉴 숨김
        resonanceHeroContentPanel.SetActive(true);
        resonancePanelController?.PlayOpenAnimation();
        gameObject.SetActive(false);
    }

    // 장비 메뉴 선택
    private void HandleEquipmentButtonClicked()
    {
        SetSelectedMenu(BottomMenuType.Equipment);

        if (equipmentRoot == null)
        {
            return;
        }

        // 장비 UI 활성화 후 기존 하단 메뉴 숨김
        equipmentRoot.SetActive(true);
        equipmentPanelController?.PlayOpenAnimation();
        gameObject.SetActive(false);
    }

    // 자식 패널 종료 후 하단 메뉴 표시
    private void HandleChildPanelClosed()
    {
        SetSelectedMenu(BottomMenuType.None);
        gameObject.SetActive(true);
    }

    // 소집 메뉴 선택
    private void HandleGachaButtonClicked()
    {
        SetSelectedMenu(BottomMenuType.Gacha);

        if (gachaRoot == null)
        {
            return;
        }

        // 소집 UI 활성화 후 오픈 연출 재생
        gachaRoot.SetActive(true);
        gachaPanelTransition?.PlayOpen();
        gameObject.SetActive(false);
    }

    // 소집 패널 닫기
    public void CloseGacha()
    {
        if (gachaRoot == null)
        {
            return;
        }

        if (gachaPanelTransition == null)
        {
            CloseGachaPanel();
            return;
        }

        gachaPanelTransition.PlayClose(CloseGachaPanel);
    }

    // 소집 패널 종료 처리
    private void CloseGachaPanel()
    {
        gachaRoot.SetActive(false);
        SetSelectedMenu(BottomMenuType.None);
        gameObject.SetActive(true);
    }

    // 전체 메뉴 선택
    private void HandleAllButtonClicked()
    {
        SetSelectedMenu(BottomMenuType.All);

        if (allPanelRoot == null)
        {
            return;
        }

        // 전체 메뉴 UI 활성화 후 기존 하단 메뉴 숨김
        allPanelRoot.SetActive(true);
        allPanelController?.PlayOpenAnimation();
        gameObject.SetActive(false);
    }

    // 선택된 하단 메뉴 갱신
    private void SetSelectedMenu(BottomMenuType menuType)
    {
        selectedMenu = menuType;

        heroMenu?.SetSelected(selectedMenu == BottomMenuType.Hero);
        equipmentMenu?.SetSelected(selectedMenu == BottomMenuType.Equipment);
        gachaMenu?.SetSelected(selectedMenu == BottomMenuType.Gacha);
        allMenu?.SetSelected(selectedMenu == BottomMenuType.All);
    }

    // 하단 메뉴 선택 상태 초기화
    public void ResetSelectedMenu()
    {
        SetSelectedMenu(BottomMenuType.None);
    }

    // 퀘스트 패널 표시 및 하단 메뉴 숨김
    public void OpenQuestPanel()
    {
        if (questPanel == null)
        {
            return;
        }

        questPanel.OpenQuestPanel();
        gameObject.SetActive(false);
    }

    // 퀘스트 패널 종료 후 하단 메뉴 표시
    private void HandleQuestPanelClosed()
    {
        SetSelectedMenu(BottomMenuType.None);
        gameObject.SetActive(true);
    }
}