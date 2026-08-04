using System;

// 특정 영웅 클래스가 공유하는 장비 슬롯별 장착 상태
[Serializable]
public class ClassEquipmentSet
{
    // 각 장비 슬롯에 장착된 아이템의 ID
    private int weaponItemId;
    private int handsItemId;
    private int accessoryItemId;
    private int headItemId;
    private int bodyItemId;
    private int legsItemId;

    // 지정한 장비 슬롯에 현재 장착된 아이템 ID를 반환
    public int GetEquippedItemId(EquipmentSlotType slotType)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Weapon:
                return weaponItemId;

            case EquipmentSlotType.Hands:
                return handsItemId;

            case EquipmentSlotType.Accessory:
                return accessoryItemId;

            case EquipmentSlotType.Head:
                return headItemId;

            case EquipmentSlotType.Body:
                return bodyItemId;

            case EquipmentSlotType.Legs:
                return legsItemId;

            default:
                throw new ArgumentOutOfRangeException(nameof(slotType));
        }
    }

    // 지정한 장비 슬롯에 아이템 ID 설정
    // itemId가 0이면 해당 슬롯을 빈 상태로 변경
    public void SetEquippedItemId(EquipmentSlotType slotType, int itemId)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Weapon:
                weaponItemId = itemId;
                break;

            case EquipmentSlotType.Hands:
                handsItemId = itemId;
                break;

            case EquipmentSlotType.Accessory:
                accessoryItemId = itemId;
                break;

            case EquipmentSlotType.Head:
                headItemId = itemId;
                break;

            case EquipmentSlotType.Body:
                bodyItemId = itemId;
                break;

            case EquipmentSlotType.Legs:
                legsItemId = itemId;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(slotType));
        }
    }

    // 지정한 슬롯의 장비 해제, 기존 장착되어 있던 아이템 ID 반환
    public int RemoveEquippedItem(EquipmentSlotType slotType)
    {
        int removedItemId = GetEquippedItemId(slotType);

        SetEquippedItemId(slotType, 0);

        return removedItemId;
    }
}