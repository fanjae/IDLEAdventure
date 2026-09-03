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
    [SerializeField] private TMP_Text goldAmountText;       // 아 배열로
    [SerializeField] private TMP_Text expAmountText;        // 처리하고싶다
    [SerializeField] private TMP_Text upgradeAmountText;    // 정말로
    [SerializeField] private TMP_Text equipAmountText;      // 다음에 해야지
    [SerializeField] private Transform equipItemParent;
    [SerializeField] private InventoryItemSlotView equipItemPrefab;
    [SerializeField] private HeroClassIconCatalog classIconCatalog;


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

    // 0903 수정 (인벤토리 처리 방식과 방치 보상 구조를 일치)
    private void CreateItemIcons(List<int> itemIds)
    {
        foreach (Transform child in equipItemParent)
        {
            Destroy(child.gameObject);
        }

        Dictionary<int, int> equipmentCounts = new();

        foreach (int itemId in itemIds)
        {
            if (equipmentCounts.TryGetValue(itemId, out int count))
            {
                equipmentCounts[itemId] = count + 1;
            }
            else
            {
                equipmentCounts.Add(itemId, 1);
            }
        }

        foreach (KeyValuePair<int, int> equipmentData in equipmentCounts)
        {
            if (!itemdatabase.TryGetItem(equipmentData.Key, out EquipmentSO equipment))
            {
                Debug.LogWarning($"[IdleRewardChest] ItemDatabase에서 장비를 찾을 수 없습니다. EquipmentId: {equipmentData.Key}");
                continue;
            }

            InventoryItemSlotView slotView = Instantiate(equipItemPrefab, equipItemParent);
            slotView.BindEquipment(equipment, equipmentData.Value, classIconCatalog);
        }
    }
}
