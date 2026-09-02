using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 방치 보상 상자 UI 클릭 기능 클래스.
/// </summary>
public class IdleRewardChest : MonoBehaviour
{
    //[Header("Chest Component")]
    //[SerializeField] private Animator animator;

    [Header("Reward Component")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private TMP_Text goldAmountText;       // 아 배열로
    [SerializeField] private TMP_Text expAmountText;        // 처리하고싶다
    [SerializeField] private TMP_Text upgradeAmountText;    // 정말로
    [SerializeField] private TMP_Text equipAmountText;      // 다음에 해야지
    [SerializeField] private Transform equipItemParent;
    [SerializeField] private GameObject equipItemPrefab;

    [Header("IdleRewardData")]
    [SerializeField] private ItemDatabaseSO itemdatabase;
    [SerializeField] private IdleReward idleRewardData;

    private bool isOpened = false;

    //private readonly string Open = "Open";


    private void Start()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    //
    private void OnEnable()
    {
        ItemReward.OnItemRewardIds += CreateItemIcons;
    }
    private void OnDisable()
    {
        ItemReward.OnItemRewardIds -= CreateItemIcons;
    }

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
    private void CreateItemIcons(List<int> itemIds)
    {
        foreach (Transform child in equipItemParent)
        {
            Destroy(child.gameObject);
        }

        foreach (int itemId in itemIds)
        {
            GameObject iconObj = Instantiate(equipItemPrefab, equipItemParent);

            if (iconObj.transform.GetChild(0).TryGetComponent<Image>(out Image iconImage))
            {
                if (itemdatabase != null)
                {
                    itemdatabase.TryGetItem<EquipmentSO>(itemId, out EquipmentSO equipment);
                    iconImage.sprite = equipment.Icon;
                }
            }
        }
    }
}
