using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ResonanceHeroDetailPanelController : MonoBehaviour
{
    private const string EquipmentStatColor = "#FFD54F";

    [Header("패널 연출")]
    [SerializeField] private UIPanelTransition heroPanelTransition;

    [SerializeField] private ResonancePanelController resonancePanelController;
    [SerializeField] private HeroDetailViewSpawner heroViewSpawner;

    [SerializeField] private GameObject resonanceHeroContentPanel;
    [SerializeField] private GameObject heroPanel;

    [SerializeField] private TMP_Text heroNameText;
    [SerializeField] private TMP_Text levelText;

    [Header("Hero Stat")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text levelUpStatText;

    [SerializeField] private Button backButton;
    [SerializeField] private Button levelButton;

    [SerializeField] private Image classIcon;
    [SerializeField] private HeroClassIconCatalog classIconCatalog;

    // 현재 선택된 영웅 ID
    private string selectedHeroId;

    private void OnEnable()
    {
        if (resonancePanelController != null)
        {
            resonancePanelController.OnHeroDetailRequested += HandleHeroDetailRequested;
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(HandleBackButtonClicked);
        }

        if (levelButton != null)
        {
            levelButton.onClick.AddListener(HandleLevelButtonClicked);
        }

        if (HeroManager.Instance != null && HeroManager.Instance.IsInitialized)
        {
            HeroManager.Instance.Controller.OnHeroStatChanged += HandleHeroStatChanged;
        }
    }

    private void OnDisable()
    {
        if (resonancePanelController != null)
        {
            resonancePanelController.OnHeroDetailRequested -= HandleHeroDetailRequested;
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(HandleBackButtonClicked);
        }

        if (levelButton != null)
        {
            levelButton.onClick.RemoveListener(HandleLevelButtonClicked);
        }

        if (HeroManager.Instance != null && HeroManager.Instance.IsInitialized)
        {
            HeroManager.Instance.Controller.OnHeroStatChanged -= HandleHeroStatChanged;
        }
    }

    private void HandleHeroDetailRequested(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            return;
        }

        if (!HeroManager.Instance.IsInitialized)
        {
            return;
        }

        if (!HeroManager.Instance.Controller.TryGetHero(heroId, out OwnedHeroData hero))
        {
            return;
        }

        // 현재 선택된 영웅 저장
        selectedHeroId = heroId;

        RefreshHeroInfo(hero);

        if (resonanceHeroContentPanel != null)
        {
            resonanceHeroContentPanel.SetActive(false);
        }

        if (heroPanel != null)
        {
            heroPanel.SetActive(true);
            heroPanelTransition?.PlayOpen();
        }

        if (heroViewSpawner != null)
        {
            heroViewSpawner.Show(heroId);
        }
    }

    private void HandleBackButtonClicked()
    {
        if (heroViewSpawner != null)
        {
            heroViewSpawner.Clear();
        }

        // 선택된 영웅 정보 초기화
        selectedHeroId = null;

        if (heroPanelTransition == null)
        {
            CloseHeroPanel();
            return;
        }

        heroPanelTransition.PlayClose(CloseHeroPanel);
    }

    // 선택된 영웅 정보 갱신
    private void RefreshHeroInfo(OwnedHeroData hero)
    {
        if (hero == null || hero.HeroData == null)
        {
            return;
        }

        if (heroNameText != null)
        {
            heroNameText.text = hero.HeroData.UnitName;
        }

        if (levelText != null)
        {
            levelText.text = $"Lv. {hero.Level}";
        }

        if (classIcon != null)
        {
            classIcon.sprite = classIconCatalog != null ? classIconCatalog.GetIcon(hero.HeroData.ClassType) : null;
        }

        RefreshHeroStat(hero);
        RefreshLevelUpStat(hero);
    }

    // 현재 레벨의 영웅 스탯과 장비 증가량 표시
    private void RefreshHeroStat(OwnedHeroData hero)
    {
        if (!HeroManager.Instance.Controller.TryGetHeroStat(hero.HeroId, out HeroStat totalStat))
        {
            return;
        }

        HeroData heroData = hero.HeroData;
        int levelIncrease = Mathf.Max(0, hero.Level - 1);

        int heroHp = heroData.MaxHp + heroData.HpPerLevel * levelIncrease;
        int heroAttack = heroData.Attack + heroData.AttackPerLevel * levelIncrease;
        int heroDefense = heroData.Defense + heroData.DefensePerLevel * levelIncrease;

        int equipmentHp = Mathf.Max(0, totalStat.MaxHp - heroHp);
        int equipmentAttack = Mathf.Max(0, totalStat.Attack - heroAttack);
        int equipmentDefense = Mathf.Max(0, totalStat.Defense - heroDefense);

        if (hpText != null)
        {
            hpText.text = FormatStat(heroHp, equipmentHp);
        }

        if (attackText != null)
        {
            attackText.text = FormatStat(heroAttack, equipmentAttack);
        }

        if (defenseText != null)
        {
            defenseText.text = FormatStat(heroDefense, equipmentDefense);
        }
    }

    // 다음 레벨에서 증가하는 영웅 기본 스탯 표시
    private void RefreshLevelUpStat(OwnedHeroData hero)
    {
        if (levelUpStatText == null || hero == null || hero.HeroData == null)
        {
            return;
        }

        HeroData heroData = hero.HeroData;
        levelUpStatText.text = $"HP +{heroData.HpPerLevel}  ATK +{heroData.AttackPerLevel}  DEF +{heroData.DefensePerLevel}";
    }

    // 영웅 기본 스탯과 장비 증가량을 구분하여 표시
    private string FormatStat(int heroStat, int equipmentStat)
    {
        return $"{heroStat} <color={EquipmentStatColor}>+ ({equipmentStat})</color>";
    }

    // 영웅 최종 스탯 변경 시 현재 상세 정보 갱신
    private void HandleHeroStatChanged()
    {
        if (string.IsNullOrEmpty(selectedHeroId))
        {
            return;
        }

        if (!HeroManager.Instance.Controller.TryGetHero(selectedHeroId, out OwnedHeroData hero))
        {
            return;
        }

        RefreshHeroInfo(hero);
    }

    // 선택된 영웅 레벨 증가
    private void HandleLevelButtonClicked()
    {
        if (string.IsNullOrEmpty(selectedHeroId))
        {
            return;
        }

        if (!HeroManager.Instance.IsInitialized)
        {
            return;
        }

        if (!HeroManager.Instance.Controller.TryGetHero(selectedHeroId, out OwnedHeroData hero))
        {
            return;
        }

        int nextLevel = hero.Level + 1;

        if (!HeroManager.Instance.Controller.TrySetHeroLevel(selectedHeroId, nextLevel))
        {
            return;
        }

        RefreshHeroInfo(hero);
    }

    // 영웅 상세 패널 종료 처리
    private void CloseHeroPanel()
    {
        if (heroPanel != null)
        {
            heroPanel.SetActive(false);
        }

        if (resonanceHeroContentPanel != null)
        {
            resonanceHeroContentPanel.SetActive(true);
            resonancePanelController?.PlayOpenAnimation();
        }
    }
}