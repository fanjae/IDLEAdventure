using System;
using System.Collections.Generic;
using UnityEngine;

// 전투 배치 영웅 목록 UI 관리
public sealed class FormationHeroListController : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private FormationHeroCardView heroCardPrefab;
    [SerializeField] private HeroClassIconCatalog classIconCatalog;

    private readonly List<FormationHeroCardView> cardViews = new();
    private HeroController heroController;

    public event Action<string> OnHeroSelected;

    private void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        Unsubscribe();

        foreach (FormationHeroCardView cardView in cardViews)
        {
            if (cardView != null)
            {
                cardView.OnSelected -= HandleHeroSelected;
            }
        }
    }

    // 보유 영웅 데이터를 기준으로 목록 초기화
    private void Initialize()
    {
        if (HeroManager.Instance == null || !HeroManager.Instance.IsInitialized)
        {
            Debug.LogError("HeroManager가 초기화되지 않았습니다.");
            return;
        }

        if (content == null)
        {
            Debug.LogError("FormationHeroListController의 Content가 없습니다.");
            return;
        }

        if (heroCardPrefab == null)
        {
            Debug.LogError("FormationHeroListController의 HeroCardPrefab이 없습니다.");
            return;
        }

        heroController = HeroManager.Instance.Controller;
        heroController.OnHeroCollectionChanged += Refresh;
        heroController.OnHeroLevelChanged += HandleHeroLevelChanged;

        Refresh();
    }

    // 현재 보유 영웅 목록을 기준으로 카드 갱신
    private void Refresh()
    {
        if (heroController == null)
        {
            return;
        }

        int visibleCount = 0;

        foreach (OwnedHeroData hero in heroController.Heroes)
        {
            FormationHeroCardView cardView = GetCardView(visibleCount);
            cardView.Bind(hero, classIconCatalog);
            cardView.gameObject.SetActive(true);

            visibleCount++;
        }

        HideUnusedCards(visibleCount);
    }

    // 필요한 수만큼 영웅 카드 생성
    private FormationHeroCardView GetCardView(int index)
    {
        while (cardViews.Count <= index)
        {
            FormationHeroCardView cardView = Instantiate(heroCardPrefab, content);
            cardView.OnSelected += HandleHeroSelected;
            cardView.gameObject.SetActive(false);
            cardViews.Add(cardView);
        }

        return cardViews[index];
    }

    // 현재 사용하지 않는 영웅 카드 숨김
    private void HideUnusedCards(int visibleCount)
    {
        for (int index = visibleCount; index < cardViews.Count; index++)
        {
            if (cardViews[index] != null)
            {
                cardViews[index].gameObject.SetActive(false);
            }
        }
    }

    // 영웅 카드 선택 처리
    private void HandleHeroSelected(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            return;
        }

        OnHeroSelected?.Invoke(heroId);
    }

    // 영웅 레벨 변경 시 목록 갱신
    private void HandleHeroLevelChanged(OwnedHeroData _)
    {
        Refresh();
    }

    // 영웅 데이터 이벤트 구독 해제
    private void Unsubscribe()
    {
        if (heroController == null)
        {
            return;
        }

        heroController.OnHeroCollectionChanged -= Refresh;
        heroController.OnHeroLevelChanged -= HandleHeroLevelChanged;
        heroController = null;
    }
}