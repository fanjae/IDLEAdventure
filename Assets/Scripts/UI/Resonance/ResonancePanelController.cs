using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 공명 패널의 보유 영웅 목록 UI 관리
public sealed class ResonancePanelController : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject resonanceHeroContentPanel;
    [SerializeField] private MainBottomPanelController mainBottomPanelController;

    [Header("패널 연출")]
    [SerializeField] private CanvasGroup resonanceCanvasGroup;
    [SerializeField] private RectTransform resonancePanelTransform;
    [SerializeField] private float moveDistance = 100f;
    [SerializeField] private float openDuration = 0.2f;
    [SerializeField] private float closeDuration = 0.2f;

    [SerializeField] private Transform heroContent;
    [SerializeField] private ResonanceHeroCardView heroCardPrefab;
    [SerializeField] private HeroClassIconCatalog classIconCatalog;
    [SerializeField] private ResonanceSlotView[] resonanceSlots;
    [SerializeField] private TMP_Text resonanceLevelText;

    private readonly List<ResonanceHeroCardView> heroCardViews = new();

    private HeroController heroController;
    private ResonanceController resonanceController;
    private Tween panelTween;
    private Vector2 resonancePanelOriginPosition;

    public event Action<string> OnHeroDetailRequested;

    private void Awake()
    {
        if (resonancePanelTransform != null)
        {
            resonancePanelOriginPosition = resonancePanelTransform.anchoredPosition;
        }
    }

    // 실행 중인 패널 연출 종료
    private void OnDestroy()
    {
        panelTween?.Kill();
    }

    // 현재 공명 상태를 기준으로 UI 갱신
    private void Refresh()
    {
        RefreshResonanceSlots();
        RefreshHeroList();
        RefreshResonanceLevel();
    }

    private void OnEnable()
    {
        InitializeControllers();
        Refresh();

        if (backButton != null)
        {
            backButton.onClick.AddListener(HandleBackButtonClicked);
        }
    }

    private void OnDisable()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(HandleBackButtonClicked);
        }

        Unsubscribe();
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

        // 공명 슬롯 변경 시 UI 갱신
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
            cardView.Bind(hero, classIconCatalog, HandleHeroCardClicked, HandleHeroCardRightClicked);
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
            // 공명 슬롯 변경 이벤트 구독 해제
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

            slotView.Bind(hero, classIconCatalog, HandleResonanceSlotClicked);
        }
    }

    // 보유 영웅 카드를 선택해 공명 슬롯에 등록
    private void HandleHeroCardClicked(string heroId)
    {
        if (resonanceController == null)
        {
            return;
        }

        resonanceController.TryAddResonanceSlotHero(heroId);
    }

    // 공명 슬롯 영웅을 선택해 등록 해제
    private void HandleResonanceSlotClicked(string heroId)
    {
        if (resonanceController == null)
        {
            return;
        }

        resonanceController.TryRemoveResonanceSlotHero(heroId);
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

    // 보유 영웅 카드 우클릭 시 상세 패널 표시 요청
    private void HandleHeroCardRightClicked(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            return;
        }

        OnHeroDetailRequested?.Invoke(heroId);
    }

    private void HandleBackButtonClicked()
    {
        if (resonanceHeroContentPanel == null || resonanceCanvasGroup == null || resonancePanelTransform == null)
        {
            return;
        }

        panelTween?.Kill();

        resonanceCanvasGroup.interactable = false;
        resonanceCanvasGroup.blocksRaycasts = false;

        Vector2 closePosition = resonancePanelOriginPosition + Vector2.down * moveDistance;

        Sequence sequence = DOTween.Sequence();

        // 아래로 이동하면서 투명하게 처리
        sequence.Join(resonancePanelTransform.DOAnchorPos(closePosition, closeDuration).SetEase(Ease.InCubic));
        sequence.Join(resonanceCanvasGroup.DOFade(0f, closeDuration));

        sequence.OnComplete(() =>
        {
            resonanceHeroContentPanel.SetActive(false);

            // 다음 오픈을 위해 기본 상태 복원
            resonancePanelTransform.anchoredPosition = resonancePanelOriginPosition;
            resonanceCanvasGroup.alpha = 1f;
            resonanceCanvasGroup.interactable = true;
            resonanceCanvasGroup.blocksRaycasts = true;

            if (mainBottomPanelController != null)
            {
                mainBottomPanelController.ResetSelectedMenu();
                mainBottomPanelController.gameObject.SetActive(true);
            }
        });

        panelTween = sequence;
    }

    // 공명 패널 오픈 연출
    public void PlayOpenAnimation()
    {
        if (resonanceCanvasGroup == null || resonancePanelTransform == null)
        {
            return;
        }

        panelTween?.Kill();

        Vector2 startPosition = resonancePanelOriginPosition + Vector2.down * moveDistance;

        // 아래 위치와 투명 상태에서 시작
        resonancePanelTransform.anchoredPosition = startPosition;
        resonanceCanvasGroup.alpha = 0f;
        resonanceCanvasGroup.interactable = false;
        resonanceCanvasGroup.blocksRaycasts = false;

        Sequence sequence = DOTween.Sequence();

        // 아래에서 위로 이동하면서 동시에 표시
        sequence.Join(resonancePanelTransform.DOAnchorPos(resonancePanelOriginPosition, openDuration).SetEase(Ease.OutCubic));
        sequence.Join(resonanceCanvasGroup.DOFade(1f, openDuration));

        sequence.OnComplete(() =>
        {
            resonanceCanvasGroup.interactable = true;
            resonanceCanvasGroup.blocksRaycasts = true;
        });

        panelTween = sequence;
    }
}