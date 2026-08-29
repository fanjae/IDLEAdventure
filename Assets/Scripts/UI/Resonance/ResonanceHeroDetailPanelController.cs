using System.Collections;
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

    [Header("Hero Skill")]
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private Image skillIcon;
    [SerializeField] private TMP_Text skillDescriptionText;
    [SerializeField] private SkillPanelTransition skillPanelTransition;

    private const float SkillChangeInterval = 5f;

    private Coroutine skillRotationCoroutine;
    private bool showPassiveSkill;

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
        StopSkillRotation();

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
        StopSkillRotation();

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

    // 액티브와 패시브 스킬을 일정 시간마다 교체
    private IEnumerator RotateSkill(SkillDataSO activeSkill, SkillDataSO passiveSkill)
    {
        WaitForSeconds wait = new WaitForSeconds(SkillChangeInterval);

        while (true)
        {
            yield return wait;

            showPassiveSkill = !showPassiveSkill;
            SkillDataSO nextSkill = showPassiveSkill ? passiveSkill : activeSkill;

            if (skillPanelTransition != null)
            {
                skillPanelTransition.PlayChange(() => ApplySkillInfo(nextSkill));
            }
            else
            {
                ApplySkillInfo(nextSkill);
            }
        }
    }

    // 선택된 영웅의 스킬 정보 표시 시작
    private void RefreshHeroSkill(OwnedHeroData hero)
    {
        StopSkillRotation();

        if (hero == null || hero.HeroData == null)
        {
            ApplySkillInfo(null);
            return;
        }

        SkillDataSO activeSkill = hero.HeroData.SkillData;
        SkillDataSO passiveSkill = hero.HeroData.PassiveSkillData;

        showPassiveSkill = false;

        if (activeSkill != null)
        {
            ApplySkillInfo(activeSkill);
        }
        else
        {
            ApplySkillInfo(passiveSkill);
        }

        if (activeSkill != null && passiveSkill != null)
        {
            skillRotationCoroutine = StartCoroutine(RotateSkill(activeSkill, passiveSkill));
        }
    }

    // 스킬 정보 순환 종료
    private void StopSkillRotation()
    {
        if (skillRotationCoroutine != null)
        {
            StopCoroutine(skillRotationCoroutine);
            skillRotationCoroutine = null;
        }

        if (skillPanelTransition != null)
        {
            skillPanelTransition.ResetState();
        }
    }
}