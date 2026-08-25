using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ResonanceHeroDetailPanelController : MonoBehaviour
{
    [SerializeField] private ResonancePanelController resonancePanelController;
    [SerializeField] private HeroDetailViewSpawner heroViewSpawner;

    [SerializeField] private GameObject resonanceHeroContentPanel;
    [SerializeField] private GameObject heroPanel;

    [SerializeField] private TMP_Text heroNameText;
    [SerializeField] private TMP_Text levelText;

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

        if (heroPanel != null)
        {
            heroPanel.SetActive(false);
        }

        if (resonanceHeroContentPanel != null)
        {
            resonanceHeroContentPanel.SetActive(true);
        }
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
}