using TMPro;
using UnityEngine;

public class QuestRewardView : MonoBehaviour
{
    [Header("UI Component")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text expText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }
    private void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCleared += UpdateUI;
        }
    }
    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCleared -= UpdateUI;
        }
    }

    //
    private void UpdateUI(QuestData data)
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
        }

        int gold = 0;
        int exp = 0;

        if (data.RewardData != null && data.RewardData.CurrencyRewards != null)
        {
            foreach (var rewardInfo in data.RewardData.CurrencyRewards)
            {
                if (rewardInfo.Type == CurrencyType.GOLD)
                {
                    gold = rewardInfo.Amount;
                }
                else if (rewardInfo.Type == CurrencyType.EXP)
                {
                    exp = rewardInfo.Amount;
                }
            }
        }

        if (goldText != null)
        {
            goldText.text = $"Gold: {gold}";
        }
        if (expText != null)
        {
            expText.text = $"Exp: {exp}";
        }
    }

    //
    public void OnClickCloseButton()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }
}
