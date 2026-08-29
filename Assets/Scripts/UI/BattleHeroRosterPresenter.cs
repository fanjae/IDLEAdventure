using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleHeroRosterPresenter : MonoBehaviour
{
    private const int MaximumExpeditionSize = 5;

    public enum SelectionMode
    {
        Multiple,
        Single
    }

    [SerializeField] private HeroPresentationCatalog presentationCatalog;
    [SerializeField] private HeroClassIconCatalog classIconCatalog;
    [SerializeField] private SelectionMode selectionMode;

    private readonly List<BattleHeroCardView> cardViews = new();
    private readonly List<string> selectedHeroIds = new();
    private readonly HashSet<string> visibleHeroIds = new();
    private HeroController heroController;
    private RectTransform rosterContent;
    private bool hasHeroFilter;

    public event System.Action<IReadOnlyList<string>> OnSelectionChanged;
    public event System.Action<string> OnSelectionDenied;

    //
    public event System.Action<string> OnHeroClicked;
    //

    public IReadOnlyList<string> SelectedHeroIds => selectedHeroIds;

    private void Awake()
    {
        CacheCardViews();
    }

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        SubscribeToHeroData();
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromHeroData();
    }

    public void Refresh()
    {
        CacheCardViews();
        SubscribeToHeroData();

        if (heroController == null || cardViews.Count == 0)
        {
            return;
        }

        int visibleCount = 0;

        foreach (OwnedHeroData ownedHero in heroController.Heroes)
        {
            if (hasHeroFilter && !visibleHeroIds.Contains(ownedHero.HeroId))
            {
                continue;
            }

            BattleHeroCardView cardView = GetCardView(visibleCount);
            cardView.Bind(
                ownedHero,
                presentationCatalog,
                classIconCatalog,
                selectedHeroIds.Contains(ownedHero.HeroId),
                HandleCardSelected);
            cardView.gameObject.SetActive(true);
            visibleCount++;
        }

        for (int index = visibleCount; index < cardViews.Count; index++)
        {
            cardViews[index].gameObject.SetActive(false);
        }
    }

    public void SetHeroFilter(IEnumerable<string> heroIds)
    {
        hasHeroFilter = true;
        visibleHeroIds.Clear();

        if (heroIds != null)
        {
            foreach (string heroId in heroIds)
            {
                if (!string.IsNullOrEmpty(heroId))
                {
                    visibleHeroIds.Add(heroId);
                }
            }
        }

        RemoveUnavailableSelections();
        Refresh();
    }

    public void ClearHeroFilter()
    {
        hasHeroFilter = false;
        visibleHeroIds.Clear();
        Refresh();
    }

    public void SetSelectedHeroIds(IEnumerable<string> heroIds, bool notify = false)
    {
        selectedHeroIds.Clear();

        if (heroIds != null)
        {
            foreach (string heroId in heroIds)
            {
                if (string.IsNullOrEmpty(heroId) || selectedHeroIds.Contains(heroId))
                {
                    continue;
                }

                selectedHeroIds.Add(heroId);

                if (selectionMode == SelectionMode.Single)
                {
                    break;
                }

                if (selectedHeroIds.Count >= MaximumExpeditionSize)
                {
                    break;
                }
            }
        }

        Refresh();

        if (notify)
        {
            OnSelectionChanged?.Invoke(selectedHeroIds);
        }
    }

    private void HandleCardSelected(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            return;
        }

        if (selectionMode == SelectionMode.Single)
        {
            selectedHeroIds.Clear();
            selectedHeroIds.Add(heroId);
        }
        else if (!selectedHeroIds.Remove(heroId))
        {
            if (selectedHeroIds.Count >= MaximumExpeditionSize)
            {
                OnSelectionDenied?.Invoke("최대 5명까지만 원정대에 편성할 수 있습니다.");
                return;
            }

            selectedHeroIds.Add(heroId);
        }

        Refresh();
        OnSelectionChanged?.Invoke(selectedHeroIds);

        //
        OnHeroClicked?.Invoke(heroId);
        //
    }

    private void SubscribeToHeroData()
    {
        if (heroController != null || HeroManager.Instance == null || !HeroManager.Instance.IsInitialized)
        {
            return;
        }

        heroController = HeroManager.Instance.Controller;
        heroController.OnHeroCollectionChanged += Refresh;
        heroController.OnHeroLevelChanged += HandleHeroLevelChanged;
    }

    private void UnsubscribeFromHeroData()
    {
        if (heroController == null)
        {
            return;
        }

        heroController.OnHeroCollectionChanged -= Refresh;
        heroController.OnHeroLevelChanged -= HandleHeroLevelChanged;
        heroController = null;
    }

    private void HandleHeroLevelChanged(OwnedHeroData _)
    {
        Refresh();
    }

    private void CacheCardViews()
    {
        if (rosterContent == null)
        {
            ScrollRect scrollRect = GetComponentInChildren<ScrollRect>(true);
            rosterContent = scrollRect != null ? scrollRect.content : null;
        }

        if (rosterContent == null || cardViews.Count > 0)
        {
            return;
        }

        rosterContent.GetComponentsInChildren(true, cardViews);
    }

    private BattleHeroCardView GetCardView(int index)
    {
        while (cardViews.Count <= index)
        {
            BattleHeroCardView cardView = Instantiate(cardViews[0], rosterContent);
            cardView.gameObject.SetActive(false);
            cardViews.Add(cardView);
        }

        return cardViews[index];
    }

    private void RemoveUnavailableSelections()
    {
        if (!hasHeroFilter)
        {
            return;
        }

        selectedHeroIds.RemoveAll(heroId => !visibleHeroIds.Contains(heroId));
    }
}
