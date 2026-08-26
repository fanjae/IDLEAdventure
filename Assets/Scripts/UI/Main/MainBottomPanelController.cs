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

    [Header("자동 전투")]
    [SerializeField] private Button autoStageButton;
    [SerializeField] private MainAdventurePanelController adventurePanelController;

    [Header("하단 메뉴 버튼")]
    [SerializeField] private Button heroButton;
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Button gachaButton;
    [SerializeField] private Button allButton;

    [Header("하단 메뉴 이미지")]
    [SerializeField] private Image heroImage;
    [SerializeField] private Image equipmentImage;
    [SerializeField] private Image gachaImage;
    [SerializeField] private Image allImage;

    [Header("기본 스프라이트")]
    [SerializeField] private Sprite heroNormalSprite;
    [SerializeField] private Sprite equipmentNormalSprite;
    [SerializeField] private Sprite gachaNormalSprite;
    [SerializeField] private Sprite allNormalSprite;

    [Header("선택 스프라이트")]
    [SerializeField] private Sprite heroSelectedSprite;
    [SerializeField] private Sprite equipmentSelectedSprite;
    [SerializeField] private Sprite gachaSelectedSprite;
    [SerializeField] private Sprite allSelectedSprite;

    private BottomMenuType selectedMenu = BottomMenuType.None;

    private void Awake()
    {
        // 씬 진입 시 모든 메뉴를 기본 상태로 초기화
        SetSelectedMenu(BottomMenuType.None);
    }

    private void OnEnable()
    {
        if (autoStageButton != null) autoStageButton.onClick.AddListener(HandleAutoStageButtonClicked);
        if (heroButton != null) heroButton.onClick.AddListener(HandleHeroButtonClicked);
        if (equipmentButton != null) equipmentButton.onClick.AddListener(HandleEquipmentButtonClicked);
        if (gachaButton != null) gachaButton.onClick.AddListener(HandleGachaButtonClicked);
        if (allButton != null) allButton.onClick.AddListener(HandleAllButtonClicked);
    }

    private void OnDisable()
    {
        autoStageButton.onClick.RemoveListener(HandleAutoStageButtonClicked);
        heroButton.onClick.RemoveListener(HandleHeroButtonClicked);
        equipmentButton.onClick.RemoveListener(HandleEquipmentButtonClicked);
        gachaButton.onClick.RemoveListener(HandleGachaButtonClicked);
        allButton.onClick.RemoveListener(HandleAllButtonClicked);
    }

    // 자동 전투 패널 표시
    private void HandleAutoStageButtonClicked()
    {
        if (adventurePanelController == null)
        {
            return;
        }

        // 자동 전투 패널을 먼저 표시한 뒤 기존 하단 메뉴를 숨김
        adventurePanelController.OpenAdventurePanel();
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

    // 모험단 메뉴 닫기
    public void CloseResonanceHeroPanel()
    {
        if (resonanceHeroContentPanel != null)
        {
            resonanceHeroContentPanel.SetActive(false);
        }

        gameObject.SetActive(true);
        SetSelectedMenu(BottomMenuType.None);
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

    // 소집 메뉴 선택
    private void HandleGachaButtonClicked()
    {
        SetSelectedMenu(BottomMenuType.Gacha);
    }

    // 전체 메뉴 선택
    private void HandleAllButtonClicked()
    {
        SetSelectedMenu(BottomMenuType.All);
    }

    // 선택된 하단 메뉴 갱신
    private void SetSelectedMenu(BottomMenuType menuType)
    {
        selectedMenu = menuType;

        // 하나의 메뉴만 선택 상태가 유지되도록 모든 이미지를 함께 갱신
        SetMenuSprite(heroImage, heroNormalSprite, heroSelectedSprite, selectedMenu == BottomMenuType.Hero);
        SetMenuSprite(equipmentImage, equipmentNormalSprite, equipmentSelectedSprite, selectedMenu == BottomMenuType.Equipment);
        SetMenuSprite(gachaImage, gachaNormalSprite, gachaSelectedSprite, selectedMenu == BottomMenuType.Gacha);
        SetMenuSprite(allImage, allNormalSprite, allSelectedSprite, selectedMenu == BottomMenuType.All);
    }

    // 메뉴 선택 여부에 따라 스프라이트 변경
    private void SetMenuSprite(Image targetImage, Sprite normalSprite, Sprite selectedSprite, bool isSelected)
    {
        if (targetImage == null)
        {
            return;
        }

        // 선택 스프라이트가 아직 지정되지 않았다면 기존 이미지를 유지
        Sprite targetSprite = isSelected ? selectedSprite : normalSprite;

        if (targetSprite != null)
        {
            targetImage.sprite = targetSprite;
        }
    }

    // 하단 메뉴 선택 상태 초기화
    public void ResetSelectedMenu()
    {
        SetSelectedMenu(BottomMenuType.None);
    }

}