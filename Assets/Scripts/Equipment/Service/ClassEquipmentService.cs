using System;
using System.Collections.Generic;

// 장비 실패 원인
public enum EquipmentEquipFailureReason
{
    None,
    InvalidItemId,
    EquipmentNotFound,
    ClassMismatch,
    EquipmentSetNotFound
}

// 클래스별 장비 장착 상태와 장착 규칙 관리
public class ClassEquipmentService
{
    // 아이템 ID를 이용해 장비 원본 데이터 조회
    private readonly ItemDatabaseSO itemDatabase;

    // 영웅 클래스별 장비 장착 상태 저장
    private readonly Dictionary<HeroClassType, ClassEquipmentSet> equipmentSets = new();

    public ClassEquipmentService(ItemDatabaseSO itemDatabase)
    {
        this.itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));

        InitializeEquipmentSets();
    }

    // 모든 영웅 클래스의 장비 세트 생성
    private void InitializeEquipmentSets()
    {
        foreach (HeroClassType heroClass in Enum.GetValues(typeof(HeroClassType)))
        {
            equipmentSets.Add(heroClass, new ClassEquipmentSet());
        }
    }

    // 지정한 영웅 클래스의 장비 세트 조회
    private bool TryGetEquipmentSet(HeroClassType heroClass, out ClassEquipmentSet equipmentSet)
    {
        return equipmentSets.TryGetValue(heroClass, out equipmentSet);
    }

    // 지정한 영웅 클래스에 장비 장착
    // 같은 슬롯에 장비가 있으면 기존 장비 ID 반환
    // 장착에 실패시 실패 원인 반환
    public bool TryEquip(HeroClassType heroClass, int itemId, out int replacedItemId, out EquipmentEquipFailureReason failureReason)
    {
        replacedItemId = 0;
        failureReason = EquipmentEquipFailureReason.None;

        // 유효하지 않은 아이템 ID 장착 방지
        if (itemId <= 0)
        {
            failureReason = EquipmentEquipFailureReason.InvalidItemId;
            return false;
        }

        // 존재하지 않거나 장비가 아닌 아이템 장착 방지
        if (!itemDatabase.TryGetItem<EquipmentSO>(itemId, out EquipmentSO equipment))
        {
            failureReason = EquipmentEquipFailureReason.EquipmentNotFound;
            return false;
        }

        // 장비 대상 클래스와 장착 대상 클래스가 다른 경우 장착 방지
        if (equipment.TargetClass != heroClass)
        {
            failureReason = EquipmentEquipFailureReason.ClassMismatch;
            return false;
        }

        if (!TryGetEquipmentSet(heroClass, out ClassEquipmentSet equipmentSet))
        {
            failureReason = EquipmentEquipFailureReason.EquipmentSetNotFound;
            return false;
        }

        EquipmentSlotType slotType = equipment.SlotType;

        // 기존 슬롯에 장착된 장비 ID 보관
        replacedItemId = equipmentSet.GetEquippedItemId(slotType);

        // 장비가 지정한 슬롯에 새로운 아이템 ID 설정
        equipmentSet.SetEquippedItemId(slotType, itemId);

        return true;
    }

    // 지정한 영웅 클래스의 슬롯에서 장비 해제
    // 해제된 장비가 있으면 기존 장비 ID 반환
    public bool TryUnequip(HeroClassType heroClass, EquipmentSlotType slotType, out int removedItemId)
    {
        removedItemId = 0;

        if (!TryGetEquipmentSet(heroClass, out ClassEquipmentSet equipmentSet))
        {
            return false;
        }

        removedItemId = equipmentSet.RemoveEquippedItem(slotType);

        return removedItemId > 0;
    }

    // 지정한 영웅 클래스와 슬롯에 장착된 장비 조회
    public bool TryGetEquippedEquipment(HeroClassType heroClass, EquipmentSlotType slotType, out EquipmentSO equipment)
    {
        equipment = null;

        if (!TryGetEquippedItemId(heroClass, slotType, out int itemId))
        {
            return false;
        }

        return itemDatabase.TryGetItem<EquipmentSO>(itemId, out equipment);
    }

    // 지정한 영웅 클래스와 슬롯에 장착된 아이템 ID 조회.
    public bool TryGetEquippedItemId(HeroClassType heroClass, EquipmentSlotType slotType, out int itemId)
    {
        itemId = 0;

        if (!TryGetEquipmentSet(heroClass, out ClassEquipmentSet equipmentSet))
        {
            return false;
        }

        itemId = equipmentSet.GetEquippedItemId(slotType);
        return itemId > 0;
    }
}