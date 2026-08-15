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

    private readonly List<InventoryItemSlotView> createdSlots = new();

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
        if (inventoryController != null)
        {
            inventoryController.OnInventoryChanged -= Refresh;
        }
    }

    // 현재 보유 중인 인벤토리 데이터를 기준으로 슬롯 목록 갱신
    public void Refresh()
    {
        ClearSlots();

        CreateItemSlots();
        CreateEquipmentSlots();
    }

    // 생성된 슬롯을 하나씩 순서대로 표시
    public void PlaySlotAnimations()
    {
        slotSequence?.Kill();

        slotSequence = DOTween.Sequence();

        foreach (InventoryItemSlotView slot in createdSlots)
        {
            slotSequence.Append(slot.CreateShowTween());
        }
    }

    // 현재 보유 중인 일반 아이템 슬롯 생성
    private void CreateItemSlots()
    {
        foreach (InventoryItemData ownedItem in inventoryController.Items)
        {
            if (!itemDatabase.TryGetItem(ownedItem.ItemId, out ItemSO item))
            {
                Debug.LogWarning($"[InventoryPanelPresenter] ItemDatabase에서 아이템을 찾을 수 없습니다. ItemId: {ownedItem.ItemId}");
                continue;
            }

            InventoryItemSlotView slot = Instantiate(itemSlotPrefab, content);
            slot.BindItem(item, ownedItem.Quantity);
            slot.PrepareHiddenState();

            createdSlots.Add(slot);
        }
    }

    // 현재 보유 중인 장비 슬롯 생성
    private void CreateEquipmentSlots()
    {
        foreach (OwnedEquipmentData ownedEquipment in inventoryController.Equipments)
        {
            if (!itemDatabase.TryGetItem(ownedEquipment.EquipmentId, out EquipmentSO equipment))
            {
                Debug.LogWarning($"[InventoryPanelPresenter] ItemDatabase에서 장비를 찾을 수 없습니다. EquipmentId: {ownedEquipment.EquipmentId}");
                continue;
            }

            InventoryItemSlotView slot = Instantiate(itemSlotPrefab, content);
            slot.BindEquipment(equipment, ownedEquipment, classIconCatalog);
            slot.PrepareHiddenState();

            createdSlots.Add(slot);
        }
    }

    // 현재 생성되어 있는 인벤토리 슬롯 제거
    private void ClearSlots()
    {
        createdSlots.Clear();

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    // 슬롯 순서에 따른 등장 애니메이션 지연 시간 계산
    private static float GetSlotAnimationDelay(int slotIndex)
    {
        return slotIndex * 0.03f;
    }
}