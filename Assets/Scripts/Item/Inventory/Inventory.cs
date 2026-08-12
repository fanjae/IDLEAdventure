using System;
using System.Collections.Generic;

// 플레이어가 보유한 일반 아이템과 장비를 관리하는 클래스
// 장착 중인 장비도 소유 데이터이므로 equipments에 계속 보관
public sealed class Inventory
{
    private readonly ItemDatabaseSO itemDatabase;

    // 일반 아이템은 같은 ItemId의 수량을 합쳐서 관리
    private readonly Dictionary<int, InventoryItemData> items = new();

    // 장비는 같은 EquipmentId라도 개별 상태가 다를 수 있으므로
    // InstanceId를 기준으로 각각 관리
    private readonly Dictionary<string, OwnedEquipmentData> equipments = new(StringComparer.Ordinal);

    public IReadOnlyCollection<InventoryItemData> Items => items.Values;
    public IReadOnlyCollection<OwnedEquipmentData> Equipments => equipments.Values;

    // 일반 아이템 또는 보유 장비가 변경됐을 때 호출
    public event Action OnInventoryChanged;

    public Inventory(ItemDatabaseSO itemDatabase)
    {
        this.itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));
    }

    // 일반 아이템을 지정한 수량만큼 추가
    // 장비 추가는 TryAcquireEquipment를 사용
    public bool TryAddItem(int itemId, int quantity)
    {
        if (!CanAddItem(itemId, quantity))
        {
            return false;
        }

        int newQuantity = GetQuantity(itemId) + quantity;
        items[itemId] = new InventoryItemData(itemId, newQuantity);

        OnInventoryChanged?.Invoke();
        return true;
    }

    // 보유 중인 일반 아이템을 지정한 수량만큼 제거
    public bool TryRemoveItem(int itemId, int quantity)
    {
        if (itemId <= 0 || quantity <= 0)
        {
            return false;
        }

        if (!items.TryGetValue(itemId, out InventoryItemData itemData))
        {
            return false;
        }

        if (quantity > itemData.Quantity)
        {
            return false;
        }

        int remainingQuantity = itemData.Quantity - quantity;

        // 수량이 남아 있지 않으면 보유 목록에서도 제거
        if (remainingQuantity == 0)
        {
            items.Remove(itemId);
        }
        else
        {
            items[itemId] = new InventoryItemData(itemId, remainingQuantity);
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    // 장비 원본 데이터를 기준으로 새로운 보유 장비 생성
    // 같은 장비를 여러 번 획득해도 각각 다른 InstanceId를 가지게 됨
    public bool TryAcquireEquipment(int equipmentId, out string instanceId)
    {
        instanceId = string.Empty;

        // 전달받은 ID가 실제 장비 데이터인지 확인
        if (!itemDatabase.TryGetItem<EquipmentSO>(equipmentId, out _))
        {
            return false;
        }

        // 중복된 InstanceId가 등록되는 것을 방지
        do
        {
            instanceId = Guid.NewGuid().ToString("N");
        }
        while (equipments.ContainsKey(instanceId));

        OwnedEquipmentData equipment = new(instanceId, equipmentId);
        equipments.Add(instanceId, equipment);

        OnInventoryChanged?.Invoke();
        return true;
    }

    // InstanceId에 해당하는 보유 장비 조회
    public bool TryGetEquipment(string instanceId, out OwnedEquipmentData equipment)
    {
        if (string.IsNullOrEmpty(instanceId))
        {
            equipment = null;
            return false;
        }

        return equipments.TryGetValue(instanceId, out equipment);
    }

    // 장비 소유 여부 확인
    public bool ContainsEquipment(string instanceId)
    {
        return !string.IsNullOrEmpty(instanceId) && equipments.ContainsKey(instanceId);
    }

    // 장착 여부 확인을 거친 장비를 보유 목록에서 제거
    // 외부에서는 ClassEquipmentService를 통해 호출
    internal bool TryRemoveEquipment(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
        {
            return false;
        }

        if (!equipments.Remove(instanceId))
        {
            return false;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    // 일반 아이템의 현재 보유 수량 반환
    public int GetQuantity(int itemId)
    {
        return items.TryGetValue(itemId, out InventoryItemData itemData) ? itemData.Quantity : 0;
    }

    // 지정한 일반 아이템을 보유하고 있는지 확인
    public bool ContainsItem(int itemId)
    {
        return items.ContainsKey(itemId);
    }

    // 일반 아이템을 추가할 수 있는 상태인지 확인
    private bool CanAddItem(int itemId, int quantity)
    {
        if (itemId <= 0 || quantity <= 0)
        {
            return false;
        }

        if (!itemDatabase.TryGetItem(itemId, out ItemSO item))
        {
            return false;
        }

        if (item.Category == ItemCategory.Equipment)
        {
            return false;
        }

        int currentQuantity = GetQuantity(itemId);
        return quantity <= int.MaxValue - currentQuantity;
    }

    // 현재 보유 중인 일반 아이템과 장비를 저장 데이터로 생성
    public InventorySaveData CreateSaveData()
    {
        InventorySaveData saveData = new();

        // 보유 중인 일반 아이템 저장
        foreach (InventoryItemData item in items.Values)
        {
            saveData.Items.Add(new InventoryItemSaveData
            {
                ItemId = item.ItemId,
                Quantity = item.Quantity
            });
        }

        // 보유 중인 장비 저장
        foreach (OwnedEquipmentData equipment in equipments.Values)
        {
            saveData.Equipments.Add(new OwnedEquipmentSaveData
            {
                InstanceId = equipment.InstanceId,
                EquipmentId = equipment.EquipmentId,
                EnhancementLevel = equipment.EnhancementLevel
            });
        }

        return saveData;
    }

    // 저장 데이터를 기준으로 일반 아이템과 보유 장비 상태 복원
    public void LoadSaveData(InventorySaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        items.Clear();
        equipments.Clear();

        // 저장된 일반 아이템 복원
        if (saveData.Items != null)
        {
            foreach (InventoryItemSaveData itemData in saveData.Items)
            {
                if (!CanLoadItem(itemData))
                {
                    continue;
                }

                items.Add(itemData.ItemId, new InventoryItemData(itemData.ItemId, itemData.Quantity));
            }
        }

        // 저장된 보유 장비 복원
        if (saveData.Equipments != null)
        {
            foreach (OwnedEquipmentSaveData equipmentData in saveData.Equipments)
            {
                if (!CanLoadEquipment(equipmentData))
                {
                    continue;
                }

                OwnedEquipmentData equipment = new(equipmentData.InstanceId, equipmentData.EquipmentId, equipmentData.EnhancementLevel);
                equipments.Add(equipment.InstanceId, equipment);
            }
        }
    }

    // 저장된 일반 아이템을 복원할 수 있는 상태인지 확인
    private bool CanLoadItem(InventoryItemSaveData itemData)
    {
        if (itemData == null || itemData.ItemId <= 0 || itemData.Quantity <= 0)
        {
            return false;
        }

        if (items.ContainsKey(itemData.ItemId))
        {
            return false;
        }

        if (!itemDatabase.TryGetItem(itemData.ItemId, out ItemSO item))
        {
            return false;
        }

        return item.Category != ItemCategory.Equipment;
    }

    // 저장된 장비를 복원할 수 있는 상태인지 확인
    private bool CanLoadEquipment(OwnedEquipmentSaveData equipmentData)
    {
        if (equipmentData == null || string.IsNullOrEmpty(equipmentData.InstanceId))
        {
            return false;
        }
        
        if (equipmentData.EnhancementLevel < 0)
        {
            return false;
        }

        if (equipments.ContainsKey(equipmentData.InstanceId))
        {
            return false;
        }

        return itemDatabase.TryGetItem<EquipmentSO>(equipmentData.EquipmentId, out _);
    }
}