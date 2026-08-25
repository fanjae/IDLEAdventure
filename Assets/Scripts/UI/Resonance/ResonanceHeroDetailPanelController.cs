using UnityEngine;
using UnityEngine.UI;

public sealed class ResonanceHeroDetailPanelController : MonoBehaviour
{
    [SerializeField] private ResonancePanelController resonancePanelController;
    [SerializeField] private HeroDetailViewSpawner heroViewSpawner;
    [SerializeField] private GameObject resonanceHeroContentPanel;
    [SerializeField] private GameObject heroPanel;
    [SerializeField] private Button backButton;

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
    }

    private void HandleHeroDetailRequested(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            return;
        }

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

        if (heroPanel != null)
        {
            heroPanel.SetActive(false);
        }

        if (resonanceHeroContentPanel != null)
        {
            resonanceHeroContentPanel.SetActive(true);
        }
    }
}