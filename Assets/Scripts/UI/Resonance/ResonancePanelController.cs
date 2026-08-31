using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 공명 패널의 보유 영웅 목록 UI 관리
public sealed class ResonancePanelController : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject resonanceHeroContentPanel;

    [Header("패널 연출")]
    [SerializeField] private UIPanelTransition panelTransition;

    [SerializeField] private Transform heroContent;
    [SerializeField] private ResonanceHeroCardView heroCardPrefab;
    [SerializeField] private HeroClassIconCatalog classIconCatalog;
    [SerializeField] private ResonanceSlotView[] resonanceSlots;
    [SerializeField] private TMP_Text resonanceLevelText;

    [SerializeField] private Button placeButton;
    [SerializeField] private Button heroDetailButton;
    [SerializeField] private TMP_Text placeButtonText;

    private string selectedHeroId;

    private readonly List<ResonanceHeroCardView> heroCardViews = new();

    private HeroController heroController;
    private ResonanceController resonanceController;

    public event Action<string> OnHeroDetailRequested;
    public event Action OnClosed;

    private void OnEnable()
    {
        InitializeControllers();
        Refresh();

        if (backButton != null)
        {
            backButton.onClick.AddListener(HandleBackButtonClicked);
        }

        if (placeButton != null)
        {
            placeButton.onClick.AddListener(HandlePlaceButtonClicked);
        }

        if (heroDetailButton != null)
        {
            heroDetailButton.onClick.AddListener(HandleHeroDetailButtonClicked);
        }

    }

    private void OnDisable()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(HandleBackButtonClicked);
        }

        if (placeButton != null)
        {
            placeButton.onClick.RemoveListener(HandlePlaceButtonClicked);
        }

        if (heroDetailButton != null)
        {
            heroDetailButton.onClick.RemoveListener(HandleHeroDetailButtonClicked);
        }

        Unsubscribe();
    }

    // 현재 공명 상태를 기준으로 UI 갱신
    private void Refresh()
    {
        RefreshResonanceSlots();
        RefreshHeroList();
        RefreshSelectionVisuals();
        RefreshBottomArea();
        RefreshResonanceLevel();
    }

    // 공명 UI에서 사용할 컨트롤러 연결
    private void InitializeControllers()
    {
        if (HeroManager.Instance == null || !HeroManager.Instance.IsInitialized)
        {
            return;
        }

        if (ResonanceManager.Instance == null || !ResonanceManager.Instance.IsInitialized)
        {
            return;
        }

        if (heroController != null || resonanceController != null)
        {
            return;
        }

        heroController = HeroManager.Instance.Controller;
        resonanceController = ResonanceManager.Instance.Controller;

        heroController.OnHeroCollectionChanged += Refresh;
        heroController.OnHeroLevelChanged += HandleHeroLevelChanged;
        resonanceController.OnResonanceSlotChanged += Refresh;
    }

    // 공명 슬롯에 등록되지 않은 보유 영웅 목록 갱신
    private void RefreshHeroList()
    {
        if (heroController == null || resonanceController == null)
        {
            return;
        }

        int visibleCount = 0;

        foreach (OwnedHeroData hero in heroController.Heroes)
        {
            // 공명 슬롯에 등록된 영웅은 목록에서 제외
            if (resonanceController.ContainsResonanceSlotHero(hero.HeroId))
            {
                continue;
            }

            ResonanceHeroCardView cardView = GetHeroCardView(visibleCount);
            cardView.Bind(hero, classIconCatalog, HandleHeroCardSelected);
            cardView.gameObject.SetActive(true);

            visibleCount++;
        }

        // 현재 표시할 영웅 수보다 남는 카드는 비활성화
        for (int index = visibleCount; index < heroCardViews.Count; index++)
        {
            heroCardViews[index].gameObject.SetActive(false);
        }
    }

    // 영웅 레벨 변경 시 공명 UI 갱신
    private void HandleHeroLevelChanged(OwnedHeroData _)
    {
        Refresh();
    }

    // 필요한 수만큼 영웅 카드 생성
    private ResonanceHeroCardView GetHeroCardView(int index)
    {
        while (heroCardViews.Count <= index)
        {
            ResonanceHeroCardView cardView = Instantiate(heroCardPrefab, heroContent);
            cardView.gameObject.SetActive(false);
            heroCardViews.Add(cardView);
        }

        return heroCardViews[index];
    }

    // 보유 영웅 이벤트 구독 해제
    private void Unsubscribe()
    {
        if (heroController != null)
        {
            heroController.OnHeroCollectionChanged -= Refresh;
            heroController.OnHeroLevelChanged -= HandleHeroLevelChanged;
        }

        if (resonanceController != null)
        {
            resonanceController.OnResonanceSlotChanged -= Refresh;
        }

        heroController = null;
        resonanceController = null;
    }

    // 현재 공명 슬롯에 등록된 영웅 정보 갱신
    private void RefreshResonanceSlots()
    {
        if (heroController == null || resonanceController == null || resonanceSlots == null)
        {
            return;
        }

        IReadOnlyList<string> heroIds = resonanceController.ResonanceSlotHeroIds;

        for (int index = 0; index < resonanceSlots.Length; index++)
        {
            ResonanceSlotView slotView = resonanceSlots[index];

            if (slotView == null)
            {
                continue;
            }

            // 등록된 영웅이 없는 슬롯은 빈 상태로 표시
            if (index >= heroIds.Count)
            {
                slotView.Clear();
                continue;
            }

            // 공명 슬롯에 등록된 영웅 데이터 조회
            if (!heroController.TryGetHero(heroIds[index], out OwnedHeroData hero))
            {
                slotView.Clear();
                continue;
            }

            slotView.Bind(hero, classIconCatalog, HandleHeroCardSelected);
        }
    }

    // 영웅 카드 선택
    private void HandleHeroCardSelected(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            return;
        }

        selectedHeroId = heroId;

        RefreshSelectionVisuals();
        RefreshBottomArea();
    }

    // 영웅 배치 또는 배치 해제
    private void HandlePlaceButtonClicked()
    {
        if (string.IsNullOrEmpty(selectedHeroId) ||
            resonanceController == null)
        {
            return;
        }

        if (resonanceController.ContainsResonanceSlotHero(selectedHeroId))
        {
            resonanceController.TryRemoveResonanceSlotHero(selectedHeroId);
            return;
        }

        resonanceController.TryAddResonanceSlotHero(selectedHeroId);
    }

    // 선택된 영웅 상세 정보 표시
    private void HandleHeroDetailButtonClicked()
    {
        if (string.IsNullOrEmpty(selectedHeroId))
        {
            return;
        }

        OnHeroDetailRequested?.Invoke(selectedHeroId);
    }

    // 모든 영웅 카드 선택 표시 갱신
    private void RefreshSelectionVisuals()
    {
        foreach (ResonanceHeroCardView cardView in heroCardViews)
        {
            if (cardView == null)
            {
                continue;
            }

            cardView.SetSelected(cardView.HeroId == selectedHeroId);
        }

        if (resonanceSlots == null)
        {
            return;
        }

        foreach (ResonanceSlotView slotView in resonanceSlots)
        {
            slotView?.SetSelected(selectedHeroId);
        }
    }

    // 하단 영웅 액션 버튼 상태 갱신
    private void RefreshBottomArea()
    {
        bool hasSelection = !string.IsNullOrEmpty(selectedHeroId) && heroController != null && heroController.TryGetHero(selectedHeroId, out _);
        bool isPlaced = hasSelection && resonanceController != null && resonanceController.ContainsResonanceSlotHero(selectedHeroId);
        bool canPlace = hasSelection && !isPlaced && resonanceController != null && resonanceController.ResonanceSlotHeroIds.Count < 3;

        if (placeButtonText != null)
        {
            placeButtonText.text = isPlaced ? "배치 해제" : "영웅 배치";
        }

        if (placeButton != null)
        {
            placeButton.interactable = canPlace || isPlaced;
        }

        if (heroDetailButton != null)
        {
            heroDetailButton.interactable = hasSelection;
        }
    }

    // 현재 공명 레벨 표시 갱신
    private void RefreshResonanceLevel()
    {
        if (resonanceLevelText == null)
        {
            return;
        }

        // 공명 슬롯 4명이 모두 등록되지 않은 경우 미적용 상태로 표시
        if (resonanceController == null || !resonanceController.TryGetResonanceLevel(out int resonanceLevel))
        {
            resonanceLevelText.text = "공명 레벨 : 미적용";
            return;
        }

        resonanceLevelText.text = $"공명 레벨 : {resonanceLevel}";
    }

    // 공명 패널 닫기
    private void HandleBackButtonClicked()
    {
        if (resonanceHeroContentPanel == null)
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

    // 공명 패널 종료 처리
    private void ClosePanel()
    {
        resonanceHeroContentPanel.SetActive(false);
        OnClosed?.Invoke();
    }

    // 공명 패널 오픈 연출
    public void PlayOpenAnimation()
    {
        panelTransition?.PlayOpen();
    }


}