using System;
using System.Collections.Generic;

// Inventory와 장비 장착 서비스를 함께 관리하는 클래스
// 외부에서는 InventoryController를 통해 인벤토리 기능에 접근을 의도
public sealed class InventoryController : ISaveDataWriter
{
    private readonly Inventory inventory;
    private readonly ClassEquipmentService equipmentService;

    public event Action OnInventoryChanged; // 인벤토리 교체 이벤트 (아이템 추가 제거 및 장비 획득 제거)
    public event Action OnEquipmentChanged; // 장비 장착·교체·해제 이벤트

    // 현재 보유 중인 일반 아이템 목록 반환
    public IReadOnlyCollection<InventoryItemData> Items => inventory.Items;

    // 현재 보유 중인 장비 목록 반환
    public IReadOnlyCollection<OwnedEquipmentData> Equipments => inventory.Equipments;

    public InventoryController(ItemDatabaseSO itemDatabase)
    {
        if (itemDatabase == null)
        {
            throw new ArgumentNullException(nameof(itemDatabase));
        }

        inventory = new Inventory(itemDatabase);
        equipmentService = new ClassEquipmentService(inventory, itemDatabase);

        inventory.OnInventoryChanged += HandleInventoryChanged;
    }

    // 일반 아이템을 지정한 수량만큼 추가
    public bool TryAddItem(int itemId, int quantity)
    {
        return inventory.TryAddItem(itemId, quantity);
    }

    // 보유 중인 일반 아이템을 지정한 수량만큼 제거
    public bool TryRemoveItem(int itemId, int quantity)
    {
        return inventory.TryRemoveItem(itemId, quantity);
    }

    // 장비 원본 데이터를 기준으로 새로운 보유 장비 생성
    public bool TryAcquireEquipment(int equipmentId, out string instanceId)
    {
        return inventory.TryAcquireEquipment(equipmentId, out instanceId);
    }

    // InstanceId에 해당하는 보유 장비 조회
    public bool TryGetEquipment(string instanceId, out OwnedEquipmentData equipment)
    {
        return inventory.TryGetEquipment(instanceId, out equipment);
    }

    // 장비 소유 여부 확인
    public bool ContainsEquipment(string instanceId)
    {
        return inventory.ContainsEquipment(instanceId);
    }

    // 일반 아이템의 현재 보유 수량 반환
    public int GetQuantity(int itemId)
    {
        return inventory.GetQuantity(itemId);
    }

    // 지정한 일반 아이템을 보유하고 있는지 확인
    public bool ContainsItem(int itemId)
    {
        return inventory.ContainsItem(itemId);
    }

    // 지정한 영웅 클래스에 보유 장비 장착
    // 같은 슬롯에 장비가 있으면 기존 장비의 InstanceId 반환
    public bool TryEquip(HeroClassType heroClass, string instanceId, out string replacedInstanceId, out EquipmentEquipFailureReason failureReason)
    {
        bool result = equipmentService.TryEquip(heroClass, instanceId, out replacedInstanceId, out failureReason);

        if (result)
        {
            OnEquipmentChanged?.Invoke();
        }

        return result;
    }

    // 지정한 영웅 클래스의 슬롯에서 장비 해제
    // 해제된 장비는 Inventory에 계속 남아 있음
    public bool TryUnequip(HeroClassType heroClass, EquipmentSlotType slotType, out string removedInstanceId)
    {
        bool result = equipmentService.TryUnequip(heroClass, slotType, out removedInstanceId);

        if (result)
        {
            OnEquipmentChanged?.Invoke();
        }

        return result;
    }

    // 지정한 클래스와 슬롯에 장착된 보유 장비 조회
    public bool TryGetEquippedOwnedEquipment(HeroClassType heroClass, EquipmentSlotType slotType, out OwnedEquipmentData ownedEquipment)
    {
        return equipmentService.TryGetEquippedOwnedEquipment(heroClass, slotType, out ownedEquipment);
    }

    // 지정한 클래스와 슬롯에 장착된 장비 원본 조회
    public bool TryGetEquippedEquipment(HeroClassType heroClass, EquipmentSlotType slotType, out EquipmentSO equipment)
    {
        return equipmentService.TryGetEquippedEquipment(heroClass, slotType, out equipment);
    }

    // 지정한 클래스와 슬롯에 장착된 InstanceId 조회
    public bool TryGetEquippedInstanceId(HeroClassType heroClass, EquipmentSlotType slotType, out string instanceId)
    {
        return equipmentService.TryGetEquippedInstanceId(heroClass, slotType, out instanceId);
    }

    // 해당 장비 인스턴스가 어떤 클래스에서든 장착 중인지 확인
    public bool IsEquipped(string instanceId)
    {
        return equipmentService.IsEquipped(instanceId);
    }

    // 장착하지 않은 보유 장비를 Inventory에서 제거
    // 이후 장비 분해나 판매 기능에서 사용
    public bool TryRemoveOwnedEquipment(string instanceId, out EquipmentRemoveFailureReason failureReason)
    {
        return equipmentService.TryRemoveOwnedEquipment(instanceId, out failureReason);
    }

    // Inventory 내부 변경 이벤트 호출
    private void HandleInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    // 현재 인벤토리 상태를 저장 데이터에 반영
    public void WriteSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        saveData.Inventory = inventory.CreateSaveData();
        saveData.Equipment = equipmentService.CreateSaveData();
    }

    // 저장 데이터를 기준으로 인벤토리와 장비 장착 상태 복원
    public void LoadSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        // 저장 데이터가 비어 있는 경우 기본 데이터 사용
        saveData.Inventory ??= new InventorySaveData();
        saveData.Equipment ??= new EquipmentSaveData();

        // 장착 상태에서 보유 장비의 InstanceId를 참조하므로 인벤토리를 먼저 복원
        inventory.LoadSaveData(saveData.Inventory);
        equipmentService.LoadSaveData(saveData.Equipment);

        OnInventoryChanged?.Invoke();
        OnEquipmentChanged?.Invoke();
    }

    // 지정한 클래스에 장착 가능한 미장착 장비가 있는지 확인
    public bool HasEquippableEquipment(HeroClassType heroClass)
    {
        return equipmentService.HasEquippableEquipment(heroClass);
    }

    // 지정한 클래스에 현재 장착 장비보다 좋은 미장착 장비가 있는지 확인
    public bool HasBetterEquippableEquipment(HeroClassType heroClass)
    {
        return equipmentService.HasBetterEquippableEquipment(heroClass);
    }

    // 지정한 클래스에 현재 장착 장비보다 좋은 장비 일괄 장착
    public bool TryAutoEquipBetterEquipment(HeroClassType heroClass)
    {
        bool result = equipmentService.TryAutoEquipBetterEquipment(heroClass);

        if (result)
        {
            OnEquipmentChanged?.Invoke();
        }

        return result;
    }
}
