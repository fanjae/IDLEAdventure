using System;
using UnityEngine;

// 특정 영웅 클래스가 공유하는 장비 슬롯별 장착 상태
// 보유 장비의 InstanceId만 참조하고 실제 장비 데이터는 Inventory에서 관리
[Serializable]
public sealed class ClassEquipmentSet
{
    // 각 장비 슬롯에 장착된 보유 장비의 InstanceId
    [SerializeField] private string weaponInstanceId = string.Empty;
    [SerializeField] private string handsInstanceId = string.Empty;
    [SerializeField] private string accessoryInstanceId = string.Empty;
    [SerializeField] private string headInstanceId = string.Empty;
    [SerializeField] private string bodyInstanceId = string.Empty;
    [SerializeField] private string legsInstanceId = string.Empty;

    // 지정한 장비 슬롯에 현재 장착된 InstanceId 반환
    public string GetEquippedInstanceId(EquipmentSlotType slotType)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Weapon:
                return weaponInstanceId;

            case EquipmentSlotType.Hands:
                return handsInstanceId;

            case EquipmentSlotType.Accessory:
                return accessoryInstanceId;

            case EquipmentSlotType.Head:
                return headInstanceId;

            case EquipmentSlotType.Body:
                return bodyInstanceId;

            case EquipmentSlotType.Legs:
                return legsInstanceId;

            default:
                throw new ArgumentOutOfRangeException(nameof(slotType));
        }
    }

    // 지정한 장비 슬롯에 InstanceId 설정
    // 빈 문자열을 전달하면 해당 슬롯을 빈 상태로 변경
    internal void SetEquippedInstanceId(EquipmentSlotType slotType, string instanceId)
    {
        instanceId ??= string.Empty;

        switch (slotType)
        {
            case EquipmentSlotType.Weapon:
                weaponInstanceId = instanceId;
                break;

            case EquipmentSlotType.Hands:
                handsInstanceId = instanceId;
                break;

            case EquipmentSlotType.Accessory:
                accessoryInstanceId = instanceId;
                break;

            case EquipmentSlotType.Head:
                headInstanceId = instanceId;
                break;

            case EquipmentSlotType.Body:
                bodyInstanceId = instanceId;
                break;

            case EquipmentSlotType.Legs:
                legsInstanceId = instanceId;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(slotType));
        }
    }

    // 지정한 슬롯의 장비를 해제하고 기존 InstanceId 반환
    internal string RemoveEquippedInstance(EquipmentSlotType slotType)
    {
        string removedInstanceId = GetEquippedInstanceId(slotType);

        SetEquippedInstanceId(slotType, string.Empty);

        return removedInstanceId;
    }

    // 해당 장비 인스턴스가 현재 장비 세트에 포함됐는지 확인
    public bool ContainsInstance(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
        {
            return false;
        }

        return string.Equals(weaponInstanceId, instanceId, StringComparison.Ordinal)
            || string.Equals(handsInstanceId, instanceId, StringComparison.Ordinal)
            || string.Equals(accessoryInstanceId, instanceId, StringComparison.Ordinal)
            || string.Equals(headInstanceId, instanceId, StringComparison.Ordinal)
            || string.Equals(bodyInstanceId, instanceId, StringComparison.Ordinal)
            || string.Equals(legsInstanceId, instanceId, StringComparison.Ordinal);
    }
}