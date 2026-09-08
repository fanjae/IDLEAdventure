using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

// 인벤토리 보유 아이템 목록 UI 관리
public sealed class InventoryPanelPresenter : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private InventoryItemSlotView itemSlotPrefab;
    [SerializeField] private ItemDatabaseSO itemDatabase;
    [SerializeField] private HeroClassIconCatalog classIconCatalog;
    [SerializeField] private EquipmentDismantleRewardDataSO dismantleRewardData;

    private readonly List<InventoryItemSlotView> slotViews = new();

    private InventoryController inventoryController;
    private EquipmentDismantleService dismantleService;
    private Sequence slotSequence;

    private void Start()
    {
        if (!InventoryManager.TryGetExistingInstance(out InventoryManager inventoryManager) || !inventoryManager.IsInitialized)
        {
            Debug.LogWarning("[InventoryPanelPresenter] InventoryManager가 초기화되지 않았습니다.");
            return;
        }

        inventoryController = inventoryManager.Controller;

        if (itemDatabase != null && dismantleRewardData != null)
        {
            dismantleService = new EquipmentDismantleService(inventoryController, itemDatabase, dismantleRewardData);
        }

        inventoryController.OnInventoryChanged += Refresh;
        inventoryController.OnEquipmentChanged += Refresh;

        Refresh();
    }

    private void OnDestroy()
    {
        slotSequence?.Kill();

        if (inventoryController != null)
        {
            inventoryController.OnInventoryChanged -= Refresh;
            inventoryController.OnEquipmentChanged -= Refresh;
        }
    }

    // 현재 보유 중인 인벤토리 데이터를 기준으로 슬롯 목록 갱신
    public void Refresh()
    {
        if (inventoryController == null)
        {
            return;
        }

        int slotIndex = 0;

        slotIndex = RefreshItemSlots(slotIndex);
        slotIndex = RefreshEquipmentSlots(slotIndex);

        HideUnusedSlots(slotIndex);
    }

    // 생성된 슬롯을 하나씩 순서대로 표시
    public void PlaySlotAnimations()
    {
        slotSequence?.Kill();

        slotSequence = DOTween.Sequence();

        foreach (InventoryItemSlotView slotView in slotViews)
        {
            if (slotView == null || !slotView.gameObject.activeSelf)
            {
                continue;
            }

            slotView.PrepareHiddenState();
            slotSequence.Append(slotView.CreateShowTween());
        }
    }

    // 현재 장착 중인 장비를 제외한 모든 보유 장비 분해
    public void DecomposeUnequippedEquipment()
    {
        if (dismantleService == null)
        {
            Debug.LogWarning("[InventoryPanelPresenter] 장비 분해 서비스가 준비되지 않았습니다.");
            return;
        }

        if (!CurrencyManager.TryGetExistingInstance(out CurrencyManager currencyManager))
        {
            Debug.LogWarning("[InventoryPanelPresenter] CurrencyManager를 찾을 수 없어 장비를 분해하지 않습니다.");
            return;
        }

        EquipmentDismantleResult result = dismantleService.DismantleUnequippedEquipment();

        if (result.DismantledCount == 0)
        {
            Debug.Log("[InventoryPanelPresenter] 분해할 미장착 장비가 없습니다.");
            return;
        }

        GrantCurrency(currencyManager, CurrencyType.GOLD, result.Gold);
        GrantCurrency(currencyManager, CurrencyType.EXP, result.Exp);
        GrantCurrency(currencyManager, CurrencyType.UPGRADE, result.Upgrade);
        GrantCurrency(currencyManager, CurrencyType.GEM, result.Gem);

        if (SaveManager.TryGetExistingInstance(out SaveManager saveManager) && saveManager.CurrentData != null)
        {
            saveManager.Save();
        }

        Debug.Log($"[장비 분해] {result.DismantledCount}개 | GOLD +{result.Gold}, EXP +{result.Exp}, UPGRADE +{result.Upgrade}, GEM +{result.Gem}");
    }

    private static void GrantCurrency(CurrencyManager currencyManager, CurrencyType type, long amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int currentAmount = currencyManager.GetCurrency(type);
        int safeAmount = (int)System.Math.Min(amount, int.MaxValue - (long)currentAmount);
        currencyManager.AddCurrency(type, safeAmount);
    }

    // 현재 보유 중인 일반 아이템 슬롯 갱신
    private int RefreshItemSlots(int slotIndex)
    {
        foreach (InventoryItemData ownedItem in inventoryController.Items)
        {
            if (!itemDatabase.TryGetItem(ownedItem.ItemId, out ItemSO item))
            {
                Debug.LogWarning($"[InventoryPanelPresenter] ItemDatabase에서 아이템을 찾을 수 없습니다. ItemId: {ownedItem.ItemId}");
                continue;
            }

            InventoryItemSlotView slotView = GetSlotView(slotIndex);
            slotView.BindItem(item, ownedItem.Quantity);
            slotView.gameObject.SetActive(true);

            slotIndex++;
        }

        return slotIndex;
    }

    // 현재 보유 중인 미장착 장비를 종류별로 묶어 슬롯 갱신
    private int RefreshEquipmentSlots(int slotIndex)
    {
        Dictionary<int, int> equipmentCounts = new();

        // 동일한 미장착 장비의 보유 수량 계산
        foreach (OwnedEquipmentData ownedEquipment in inventoryController.Equipments)
        {
            // 현재 장착 중인 장비는 인벤토리 슬롯에서 제외
            if (inventoryController.IsEquipped(ownedEquipment.InstanceId))
            {
                continue;
            }

            if (equipmentCounts.TryGetValue(ownedEquipment.EquipmentId, out int count))
            {
                equipmentCounts[ownedEquipment.EquipmentId] = count + 1;
            }
            else
            {
                equipmentCounts.Add(ownedEquipment.EquipmentId, 1);
            }
        }

        // 장비 종류별로 슬롯 하나씩 표시
        foreach (KeyValuePair<int, int> equipmentData in equipmentCounts)
        {
            if (!itemDatabase.TryGetItem(equipmentData.Key, out EquipmentSO equipment))
            {
                Debug.LogWarning($"[InventoryPanelPresenter] ItemDatabase에서 장비를 찾을 수 없습니다. EquipmentId: {equipmentData.Key}");
                continue;
            }

            InventoryItemSlotView slotView = GetSlotView(slotIndex);
            slotView.BindEquipment(equipment, equipmentData.Value, classIconCatalog);
            slotView.gameObject.SetActive(true);

            slotIndex++;
        }

        return slotIndex;
    }

    // 필요한 수만큼 인벤토리 슬롯 생성
    private InventoryItemSlotView GetSlotView(int index)
    {
        while (slotViews.Count <= index)
        {
            InventoryItemSlotView slotView = Instantiate(itemSlotPrefab, content);
            slotView.gameObject.SetActive(false);
            slotViews.Add(slotView);
        }

        return slotViews[index];
    }

    // 현재 사용하지 않는 슬롯 숨김
    private void HideUnusedSlots(int visibleCount)
    {
        for (int index = visibleCount; index < slotViews.Count; index++)
        {
            if (slotViews[index] != null)
            {
                slotViews[index].gameObject.SetActive(false);
            }
        }
    }
}
