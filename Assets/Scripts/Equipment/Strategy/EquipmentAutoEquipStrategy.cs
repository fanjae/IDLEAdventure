using System;
using System.Collections.Generic;

// 클래스에 장착할 장비 후보를 비교하고 슬롯별 최적 장비를 선택
public sealed class EquipmentAutoEquipStrategy
{
    private readonly Inventory inventory;
    private readonly ItemDatabaseSO itemDatabase;

    public EquipmentAutoEquipStrategy(Inventory inventory, ItemDatabaseSO itemDatabase)
    {
        this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        this.itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));
    }

    // 지정한 클래스에 장착 가능한 미장착 장비가 있는지 확인
    public bool HasEquippableEquipment(HeroClassType heroClass, Func<string, bool> isEquipped)
    {
        foreach (OwnedEquipmentData ownedEquipment in inventory.Equipments)
        {
            if (TryGetCandidateEquipment(heroClass, ownedEquipment, isEquipped, out _))
            {
                return true;
            }
        }

        return false;
    }

    // 지정한 클래스에서 슬롯별 가장 높은 제작 레벨의 미장착 장비 조회
    public Dictionary<EquipmentSlotType, OwnedEquipmentData> FindBestEquipmentBySlot(HeroClassType heroClass, Func<string, bool> isEquipped)
    {
        Dictionary<EquipmentSlotType, OwnedEquipmentData> bestEquipmentBySlot = new();

        foreach (OwnedEquipmentData ownedEquipment in inventory.Equipments)
        {
            if (!TryGetCandidateEquipment(heroClass, ownedEquipment, isEquipped, out EquipmentSO candidateEquipment))
            {
                continue;
            }

            if (!bestEquipmentBySlot.TryGetValue(candidateEquipment.SlotType, out OwnedEquipmentData currentBest))
            {
                bestEquipmentBySlot.Add(candidateEquipment.SlotType, ownedEquipment);
                continue;
            }

            if (!TryGetEquipmentData(currentBest, out EquipmentSO currentBestEquipment))
            {
                bestEquipmentBySlot[candidateEquipment.SlotType] = ownedEquipment;
                continue;
            }

            if (IsBetterEquipment(candidateEquipment, currentBestEquipment))
            {
                bestEquipmentBySlot[candidateEquipment.SlotType] = ownedEquipment;
            }
        }

        return bestEquipmentBySlot;
    }

    // 후보 장비가 현재 장착 장비보다 좋은 장비인지 확인
    public bool IsBetterEquipment(EquipmentSO candidateEquipment, EquipmentSO equippedEquipment)
    {
        if (candidateEquipment == null)
        {
            return false;
        }

        // 현재 슬롯이 비어 있으면 후보 장비를 바로 장착 가능
        if (equippedEquipment == null)
        {
            return true;
        }

        return candidateEquipment.CraftLevel > equippedEquipment.CraftLevel;
    }

    // 지정한 클래스의 자동 장착 후보로 사용할 수 있는 장비인지 확인
    private bool TryGetCandidateEquipment(HeroClassType heroClass, OwnedEquipmentData ownedEquipment, Func<string, bool> isEquipped, out EquipmentSO equipment)
    {
        equipment = null;

        if (ownedEquipment == null || isEquipped(ownedEquipment.InstanceId))
        {
            return false;
        }

        if (!TryGetEquipmentData(ownedEquipment, out equipment))
        {
            return false;
        }

        return equipment.TargetClass == heroClass;
    }

    // 보유 장비와 연결된 원본 EquipmentSO 조회
    private bool TryGetEquipmentData(OwnedEquipmentData ownedEquipment, out EquipmentSO equipment)
    {
        equipment = null;

        if (ownedEquipment == null)
        {
            return false;
        }

        return itemDatabase.TryGetItem<EquipmentSO>(ownedEquipment.EquipmentId, out equipment);
    }
}