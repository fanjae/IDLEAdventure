using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 이미지 없이도 동작하는 업적 한 줄 UI임
public sealed class AchievementRowView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite fallbackIcon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text actionText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button claimButton;

    private AchievementDefinitionSO definition;
    private AchievementPanelPresenter owner;

    public void Bind(AchievementDefinitionSO achievementDefinition, AchievementPanelPresenter panelOwner)
    {
        definition = achievementDefinition;
        owner = panelOwner;

        if (iconImage != null)
        {
            iconImage.sprite = definition.Icon != null ? definition.Icon : fallbackIcon;
            iconImage.enabled = iconImage.sprite != null;
        }

        titleText.text = definition.DisplayName;
        descriptionText.text = definition.Description;
        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(Claim);
        Refresh();
    }

    public void Refresh()
    {
        if (definition == null || !AchievementManager.Instance.IsInitialized)
        {
            return;
        }

        AchievementController controller = AchievementManager.Instance.Controller;
        AchievementProgress progress = controller.GetProgress(definition);
        progressText.text = progress.Current + " / " + progress.Target;
        progressSlider.minValue = 0f;
        progressSlider.maxValue = progress.Target;
        progressSlider.value = progress.Current;

        if (progress.IsClaimed)
        {
            rewardText.text = "수령 완료";
            actionText.text = "완료";
            claimButton.interactable = false;
            return;
        }

        if (controller.CanClaim(definition) && definition.HasReward)
        {
            rewardText.text = "+" + definition.RewardAmount;
            actionText.text = "받기";
            claimButton.interactable = true;
            return;
        }

        rewardText.text = definition.HasReward ? "+" + definition.RewardAmount : "보상 미설정";
        actionText.text = controller.CanClaim(definition) ? "보상 설정 필요" : "진행 중";
        claimButton.interactable = false;
    }

    private void Claim()
    {
        if (AchievementManager.Instance.TryClaim(definition, out CurrencyType rewardCurrency, out int rewardAmount))
        {
            owner.PlayRewardClaimSfx();
            owner.ShowRewardToast(new AchievementClaimReward(definition, rewardCurrency, rewardAmount));
            owner.Refresh();
        }
    }
}
