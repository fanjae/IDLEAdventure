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

    private readonly List<InventoryItemSlotView> slotViews = new();

    private InventoryController inventoryController;
    private Sequence slotSequence;

    private void Start()
    {
        if (!InventoryManager.TryGetExistingInstance(out InventoryManager inventoryManager) || !inventoryManager.IsInitialized)
        {
            Debug.LogWarning("[InventoryPanelPresenter] InventoryManager가 초기화되지 않았습니다.");
            return;
        }

        inventoryController = inventoryManager.Controller;
        inventoryController.OnInventoryChanged += Refresh;

        Refresh();
    }

    private void OnDestroy()
    {
        slotSequence?.Kill();

        if (inventoryController != null)
        {
            inventoryController.OnInventoryChanged -= Refresh;
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

    // 현재 보유 중인 장비 슬롯 갱신
    private int RefreshEquipmentSlots(int slotIndex)
    {
        foreach (OwnedEquipmentData ownedEquipment in inventoryController.Equipments)
        {
            if (!itemDatabase.TryGetItem(ownedEquipment.EquipmentId, out EquipmentSO equipment))
            {
                Debug.LogWarning($"[InventoryPanelPresenter] ItemDatabase에서 장비를 찾을 수 없습니다. EquipmentId: {ownedEquipment.EquipmentId}");
                continue;
            }

            InventoryItemSlotView slotView = GetSlotView(slotIndex);
            slotView.BindEquipment(equipment, ownedEquipment, classIconCatalog);
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