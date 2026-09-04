using System.Collections.Generic;
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

    [Header("Hero Level Up")]
    [SerializeField] private TMP_Text levelUpCostText;
    [SerializeField] private HeroLevelUpCostDatabaseSO levelUpCostDatabase;

    [SerializeField] private Image classIcon;
    [SerializeField] private HeroClassIconCatalog classIconCatalog;

    [Header("Hero Skill")]
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private Image skillIcon;
    [SerializeField] private TMP_Text skillDescriptionText;
    [SerializeField] private SkillPanelTransition skillPanelTransition;

    [Header("Hero Skill List")]
    [SerializeField] private HeroSkillSlotUI skillSlot1;
    [SerializeField] private HeroSkillSlotUI skillSlot2;

    private HeroSkillSlotUI selectedSkillSlot;

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

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
        }

        if (HeroManager.Instance != null && HeroManager.Instance.IsInitialized)
        {
            HeroManager.Instance.Controller.OnHeroStatChanged += HandleHeroStatChanged;
        }
    }

    private void OnDisable()
    {
        ClearSelectedSkill();

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

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
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
        RefreshHeroSkill(hero);

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
        ClearSelectedSkill();

        if (heroViewSpawner != null)
        {
            heroViewSpawner.Clear();
        }

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
        RefreshLevelUpCost(hero);
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

    // 재화 변동 시 비용 부족 상태와 레벨업 버튼 상태 갱신
    private void HandleCurrencyChanged(CurrencyType _, int __)
    {
        if (string.IsNullOrEmpty(selectedHeroId) || HeroManager.Instance == null ||
            !HeroManager.Instance.IsInitialized)
        {
            return;
        }

        if (HeroManager.Instance.Controller.TryGetHero(selectedHeroId, out OwnedHeroData hero))
        {
            RefreshLevelUpCost(hero);
        }
    }

    // 선택된 영웅 레벨 증가
    private void HandleLevelButtonClicked()
    {
        if (levelButton != null && !levelButton.interactable)
        {
            return;
        }

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

        if (levelUpCostDatabase == null ||
            !levelUpCostDatabase.TryGetCost(hero.Level, out HeroLevelUpCostData cost))
        {
            return;
        }

        CurrencyManager currencyManager = CurrencyManager.Instance;

        if (currencyManager.GetCurrency(CurrencyType.GOLD) < cost.GoldCost ||
            currencyManager.GetCurrency(CurrencyType.EXP) < cost.ExpCost ||
            currencyManager.GetCurrency(CurrencyType.UPGRADE) < cost.UpgradeCost)
        {
            return;
        }

        int nextLevel = hero.Level + 1;

        if (!HeroManager.Instance.Controller.TrySetHeroLevel(selectedHeroId, nextLevel))
        {
            return;
        }

        currencyManager.UseCurrency(CurrencyType.GOLD, cost.GoldCost);
        currencyManager.UseCurrency(CurrencyType.EXP, cost.ExpCost);

        if (cost.UpgradeCost > 0)
        {
            currencyManager.UseCurrency(CurrencyType.UPGRADE, cost.UpgradeCost);
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

    // 스킬 이름과 아이콘, 설명 표시
    private void ApplySkillInfo(SkillDataSO skillData)
    {
        if (skillNameText != null)
        {
            skillNameText.text = skillData != null ? $"[스킬명 : {skillData.DisplayName}]" : string.Empty;
        }

        if (skillIcon != null)
        {
            skillIcon.sprite = skillData != null ? skillData.Icon : null;
            skillIcon.preserveAspect = true;
            skillIcon.enabled = skillData != null && skillData.Icon != null;
        }

        if (skillDescriptionText != null)
        {
            skillDescriptionText.text = skillData != null ? skillData.Description : string.Empty;
        }
    }

    // 선택된 영웅의 보유 스킬 목록 갱신
    private void RefreshHeroSkill(OwnedHeroData hero)
    {
        ClearSelectedSkill();

        if (hero == null || hero.HeroData == null)
        {
            skillSlot1?.Bind(null, null);
            skillSlot2?.Bind(null, null);
            ApplySkillInfo(null);
            return;
        }

        SkillDataSO activeSkill = hero.HeroData.SkillData;
        SkillDataSO passiveSkill = hero.HeroData.PassiveSkillData;

        skillSlot1?.Bind(activeSkill, HandleSkillSelected);
        skillSlot2?.Bind(passiveSkill, HandleSkillSelected);

        if (activeSkill != null)
        {
            SelectSkill(skillSlot1, activeSkill);
            return;
        }

        if (passiveSkill != null)
        {
            SelectSkill(skillSlot2, passiveSkill);
            return;
        }

        ApplySkillInfo(null);
    }

    // 보유 스킬 슬롯 선택 처리
    private void HandleSkillSelected(HeroSkillSlotUI slot, SkillDataSO skillData)
    {
        SelectSkill(slot, skillData);
    }

    // 선택된 스킬 슬롯 표시 및 하단 상세 정보 갱신
    private void SelectSkill(HeroSkillSlotUI slot, SkillDataSO skillData)
    {
        if (slot == null || skillData == null)
        {
            return;
        }

        if (selectedSkillSlot == slot)
        {
            return;
        }

        if (selectedSkillSlot != null)
        {
            selectedSkillSlot.SetSelected(false);
        }

        selectedSkillSlot = slot;
        selectedSkillSlot.SetSelected(true);

        if (skillPanelTransition != null)
        {
            skillPanelTransition.PlayChange(() => ApplySkillInfo(skillData));
        }
        else
        {
            ApplySkillInfo(skillData);
        }
    }

    // 현재 선택된 스킬 슬롯 상태 초기화
    private void ClearSelectedSkill()
    {
        if (selectedSkillSlot != null)
        {
            selectedSkillSlot.SetSelected(false);
            selectedSkillSlot = null;
        }
    }

    // 현재 레벨에서 다음 레벨로 증가할 때 필요한 재화 표시
    private void RefreshLevelUpCost(OwnedHeroData hero)
    {
        if (levelButton != null)
        {
            levelButton.interactable = false;
        }

        if (levelUpCostText == null || levelUpCostDatabase == null || hero == null)
        {
            return;
        }

        if (hero.Level >= levelUpCostDatabase.MaxLevel)
        {
            levelUpCostText.text = "최대 레벨에 도달하였습니다.";
            return;
        }

        if (!levelUpCostDatabase.TryGetCost(hero.Level, out HeroLevelUpCostData cost))
        {
            levelUpCostText.text = "레벨업 비용 정보를 찾을 수 없습니다.";
            return;
        }

        string upgradeCost = cost.UpgradeCost > 0
            ? $"  Upgrade : {cost.UpgradeCost:N0}"
            : string.Empty;

        string costText = $"Coin : {cost.GoldCost:N0}  EXP : {cost.ExpCost:N0}{upgradeCost}";
        List<string> insufficientCurrencies = new List<string>();
        CurrencyManager currencyManager = CurrencyManager.Instance;

        if (currencyManager == null || currencyManager.GetCurrency(CurrencyType.GOLD) < cost.GoldCost)
        {
            insufficientCurrencies.Add("Coin이 부족합니다.");
        }

        if (currencyManager == null || currencyManager.GetCurrency(CurrencyType.EXP) < cost.ExpCost)
        {
            insufficientCurrencies.Add("EXP가 부족합니다.");
        }

        if (cost.UpgradeCost > 0 &&
            (currencyManager == null || currencyManager.GetCurrency(CurrencyType.UPGRADE) < cost.UpgradeCost))
        {
            insufficientCurrencies.Add("Upgrade가 부족합니다.");
        }

        if (insufficientCurrencies.Count > 0)
        {
            levelUpCostText.text = $"{costText}\n{string.Join(" / ", insufficientCurrencies)}";
            return;
        }

        levelUpCostText.text = costText;

        if (levelButton != null)
        {
            levelButton.interactable = true;
        }
    }
}
