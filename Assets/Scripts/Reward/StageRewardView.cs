using TMPro;
using UnityEngine;

public class StageRewardView : MonoBehaviour
{
    [Header("UI Component")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text upgradeText;
    [SerializeField] private TMP_Text gemText;

    [Header("StageClearRewardData")]
    [SerializeField] private StageClearRewardTest rewardData;

    private void OnEnable()
    {
        if (rewardData != null)
        {
            rewardData.OnStageRewardGiven += UpdateUI;
        }
    }
    private void OnDisable()
    {
        if (rewardData != null)
        {
            rewardData.OnStageRewardGiven -= UpdateUI;
        }
    }

    //
    private void UpdateUI(int stageId, StageRewardData rewardData)
    {
        if (goldText != null)
        {
            int gold = 0;
            // "GOLD" 키가 존재한다면 꺼내서 int로 형변환
            if (rewardData.Rewards.TryGetValue("GOLD", out IReward goldReward))
            {
                gold = (int)goldReward.RewardValue;
            }
            goldText.text = $"Gold: {gold}";
        }

        if (expText != null)
        {
            int exp = 0;
            if (rewardData.Rewards.TryGetValue("EXP", out IReward expReward))
            {
                exp = (int)expReward.RewardValue;
            }
            expText.text = $"Exp: {exp}";
        }

        if (upgradeText != null)
        {
            int upgrade = 0;
            if (rewardData.Rewards.TryGetValue("UPGRADE", out IReward upgradeReward))
            {
                upgrade = (int)upgradeReward.RewardValue;
            }
            upgradeText.text = $"Upgrade: {upgrade}";
        }

        if (gemText != null)
        {
            int equip = 0;
            if (rewardData.Rewards.TryGetValue("EQUIPBOX", out IReward equipReward))
            {
                equip = (int)equipReward.RewardValue;
            }
            gemText.text = $"Gem: {equip}";
        }
    }
}
