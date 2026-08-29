using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HeroDictionaryPresenter : MonoBehaviour
{
    private enum RosterFilter
    {
        All,
        Owned,
        Unowned
    }

    [SerializeField] private HeroDatabaseSO heroDatabase;
    [SerializeField] private HeroPresentationCatalog presentationCatalog;

    [SerializeField] private Button allTabButton;
    [SerializeField] private Button ownedTabButton;
    [SerializeField] private Button unownedTabButton;
    [SerializeField] private TMP_Text progressLabel;
    [SerializeField] private Image progressFill;

    [SerializeField] private RectTransform rosterContent;
    [SerializeField] private HeroDictionaryCardView cardTemplate;

    [SerializeField] private GameObject detailOverlay;
    [SerializeField] private Image detailPortraitImage;
    [SerializeField] private Image detailSkillIcon;
    [SerializeField] private Button skillIconButton;
    [SerializeField] private Button skillInfoDismissButton;
    [SerializeField] private GameObject skillInfoPanel;
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text skillDescriptionText;
    [SerializeField] private TMP_Text detailText;
    [SerializeField] private Button closeDetailButton;

    private readonly List<HeroDictionaryCardView> cardViews = new();

    private HeroController heroController;
    private RosterFilter currentFilter;
    private SkillDataSO selectedSkillData;

    private void Awake()
    {
        allTabButton?.onClick.AddListener(ShowAll);
        ownedTabButton?.onClick.AddListener(ShowOwned);
        unownedTabButton?.onClick.AddListener(ShowUnowned);
        closeDetailButton?.onClick.AddListener(CloseDetail);
        skillIconButton?.onClick.AddListener(ToggleSkillInfo);
        skillInfoDismissButton?.onClick.AddListener(CloseSkillInfo);

        if (cardTemplate != null)
        {
            cardTemplate.gameObject.SetActive(false);
        }

        CloseDetail();
    }

    private void OnDestroy()
    {
        allTabButton?.onClick.RemoveListener(ShowAll);
        ownedTabButton?.onClick.RemoveListener(ShowOwned);
        unownedTabButton?.onClick.RemoveListener(ShowUnowned);
        closeDetailButton?.onClick.RemoveListener(CloseDetail);
        skillIconButton?.onClick.RemoveListener(ToggleSkillInfo);
        skillInfoDismissButton?.onClick.RemoveListener(CloseSkillInfo);
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

    public void ShowAll()
    {
        currentFilter = RosterFilter.All;
        Refresh();
    }

    public void ShowOwned()
    {
        currentFilter = RosterFilter.Owned;
        Refresh();
    }

    public void ShowUnowned()
    {
        currentFilter = RosterFilter.Unowned;
        Refresh();
    }

    public void Refresh()
    {
        if (heroDatabase == null || rosterContent == null || cardTemplate == null)
        {
            return;
        }

        SubscribeToHeroData();

        int ownedCount = 0;
        int visibleCount = 0;

        foreach (HeroData heroData in heroDatabase.Heroes)
        {
            if (heroData == null)
            {
                continue;
            }

            OwnedHeroData ownedHero = GetOwnedHero(heroData.UnitID);
            bool isOwned = ownedHero != null;

            if (isOwned)
            {
                ownedCount++;
            }

            if (!ShouldShow(isOwned))
            {
                continue;
            }

            HeroDictionaryCardView cardView = GetCardView(visibleCount);
            cardView.Bind(heroData, ownedHero, presentationCatalog, ShowDetail);
            cardView.gameObject.SetActive(true);
            visibleCount++;
        }

        for (int index = visibleCount; index < cardViews.Count; index++)
        {
            cardViews[index].gameObject.SetActive(false);
        }

        UpdateProgress(ownedCount, heroDatabase.Heroes.Count);
    }

    public void CloseDetail()
    {
        CloseSkillInfo();

        if (detailOverlay != null)
        {
            detailOverlay.SetActive(false);
        }

        selectedSkillData = null;
        SetSkillIcon(null);
    }

    private HeroDictionaryCardView GetCardView(int index)
    {
        while (cardViews.Count <= index)
        {
            HeroDictionaryCardView cardView = Instantiate(cardTemplate, rosterContent);
            cardView.gameObject.SetActive(false);
            cardViews.Add(cardView);
        }

        return cardViews[index];
    }

    private OwnedHeroData GetOwnedHero(string heroId)
    {
        if (heroController != null && heroController.TryGetHero(heroId, out OwnedHeroData ownedHero))
        {
            return ownedHero;
        }

        return null;
    }

    private bool ShouldShow(bool isOwned)
    {
        return currentFilter switch
        {
            RosterFilter.Owned => isOwned,
            RosterFilter.Unowned => !isOwned,
            _ => true
        };
    }

    private void UpdateProgress(int ownedCount, int totalCount)
    {
        if (progressLabel != null)
        {
            progressLabel.text = $"{ownedCount} / {totalCount}";
        }

        if (progressFill != null)
        {
            progressFill.fillAmount = totalCount > 0 ? (float)ownedCount / totalCount : 0f;
        }
    }

    private void ShowDetail(HeroData heroData)
    {
        if (heroData == null)
        {
            return;
        }

        OwnedHeroData ownedHero = GetOwnedHero(heroData.UnitID);
        bool isOwned = ownedHero != null;

        if (detailPortraitImage != null)
        {
            detailPortraitImage.sprite = presentationCatalog != null
                ? presentationCatalog.GetDetailIllustration(heroData.UnitID)
                : null;
        }

        if (detailText != null)
        {
            detailText.text = CreateDetailText(heroData, ownedHero, isOwned);
        }

        selectedSkillData = heroData.SkillData;
        SetSkillIcon(selectedSkillData);
        UpdateSkillInfo(selectedSkillData);
        CloseSkillInfo();

        if (detailOverlay != null)
        {
            detailOverlay.SetActive(true);
        }
    }

    private void SetSkillIcon(SkillDataSO skillData)
    {
        if (detailSkillIcon == null)
        {
            return;
        }

        Sprite icon = skillData != null ? skillData.Icon : null;
        detailSkillIcon.sprite = icon;
        detailSkillIcon.preserveAspect = true;
        detailSkillIcon.enabled = icon != null;
    }

    private void ToggleSkillInfo()
    {
        if (skillInfoPanel == null || selectedSkillData == null || detailSkillIcon == null || detailSkillIcon.sprite == null)
        {
            return;
        }

        skillInfoPanel.SetActive(!skillInfoPanel.activeSelf);
    }

    private void CloseSkillInfo()
    {
        if (skillInfoPanel != null)
        {
            skillInfoPanel.SetActive(false);
        }
    }

    private void UpdateSkillInfo(SkillDataSO skillData)
    {
        if (skillNameText != null)
        {
            skillNameText.text = skillData != null && !string.IsNullOrWhiteSpace(skillData.DisplayName)
                ? skillData.DisplayName
                : "스킬";
        }

        if (skillDescriptionText != null)
        {
            skillDescriptionText.text = skillData != null && !string.IsNullOrWhiteSpace(skillData.Description)
                ? skillData.Description
                : "스킬 설명이 아직 등록되지 않았습니다.";
        }
    }

    private static string CreateDetailText(HeroData heroData, OwnedHeroData ownedHero, bool isOwned)
    {
        if (!isOwned)
        {
            return $"{heroData.UnitName}\n미보유 영웅";
        }

        int levelOffset = ownedHero.Level - 1;
        int maxHp = heroData.MaxHp + heroData.HpPerLevel * levelOffset;
        int attack = heroData.Attack + heroData.AttackPerLevel * levelOffset;
        int defense = heroData.Defense + heroData.DefensePerLevel * levelOffset;

        return $"{heroData.UnitName}\nLv. {ownedHero.Level}\n{heroData.ClassType} / {heroData.Role}\nHP {maxHp}  ATK {attack}  DEF {defense}";
    }
}
