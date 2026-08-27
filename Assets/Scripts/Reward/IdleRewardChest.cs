using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 방치 보상 상자 UI 클릭 기능 클래스.
/// </summary>
public class IdleRewardChest : MonoBehaviour
{
    //[Header("Chest Component")]
    //[SerializeField] private Animator animator;

    [Header("Reward Component")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private TMP_Text goldAmountText;
    [SerializeField] private TMP_Text expAmountText;
    [SerializeField] private TMP_Text upgradeAmountText;
    [SerializeField] private TMP_Text equipAmountText;

    [Header("IdleRewardData")]
    [SerializeField] private IdleReward idleRewardData;

    private bool isOpened = false;

    private readonly string Open = "Open";

    private void Start()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    //
    public void OnClickChestButton()
    {
        if (isOpened) return;
        isOpened = true;

        //if (animator != null)
        //{
        //    animator.SetTrigger(Open);
        //}

        OnChestOpened();
    }
    //
    public void OnChestOpened()
    {
        if (idleRewardData == null) return;

        Dictionary<string, int> getRewards = idleRewardData.GetExpectedRewards();

        idleRewardData.OnClickIdleRewardButton();

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
        }
        if (goldAmountText != null)
        {
            getRewards.TryGetValue("GOLD", out int gold);
            goldAmountText.text = $"Gold: {gold}";
        }
        if (expAmountText != null)
        {
            getRewards.TryGetValue("EXP", out int exp);
            expAmountText.text = $"Exp: {exp}";
        }
        if (upgradeAmountText != null)
        {
            getRewards.TryGetValue("UPGRADE", out int upgrade);
            upgradeAmountText.text = $"Upgrade: {upgrade}";
        }
        if (equipAmountText != null)
        {
            getRewards.TryGetValue("EQUIPBOX", out int equip);
            equipAmountText.text = $"EquipBox: {equip}";
        }

        isOpened = false;
    }
    //
    public void OnClickBackButton()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }
}
