using System;

// 클래스에 장착된 장비의 전체 능력치 계산
public sealed class EquipmentStatCalculator
{
    private readonly InventoryController inventoryController;

    public EquipmentStatCalculator(InventoryController inventoryController)
    {
        this.inventoryController = inventoryController ?? throw new ArgumentNullException(nameof(inventoryController));
    }

    // 지정한 클래스에 장착된 모든 장비의 능력치 합산
    public EquipmentStat Calculate(HeroClassType heroClass)
    {
        EquipmentStat totalStat = EquipmentStat.Zero;

        // 클래스의 모든 장비 슬롯 확인
        foreach (EquipmentSlotType slotType in Enum.GetValues(typeof(EquipmentSlotType)))
        {
            // 현재 슬롯에 장비가 없는 경우 계산 제외
            if (!TryCalculateSlotStat(heroClass, slotType, out EquipmentStat slotStat))
            {
                continue;
            }

            totalStat += slotStat;
        }

        return totalStat;
    }

    // 지정한 클래스와 슬롯에 장착된 장비의 능력치 계산
    private bool TryCalculateSlotStat(HeroClassType heroClass, EquipmentSlotType slotType, out EquipmentStat equipmentStat)
    {
        equipmentStat = EquipmentStat.Zero;

        // 현재 슬롯에 장착된 보유 장비 정보 조회
        if (!inventoryController.TryGetEquippedOwnedEquipment(heroClass, slotType, out OwnedEquipmentData ownedEquipment))
        {
            return false;
        }

        // 보유 장비와 연결된 원본 EquipmentSO 조회
        if (!inventoryController.TryGetEquippedEquipment(heroClass, slotType, out EquipmentSO equipment))
        {
            return false;
        }

        equipmentStat = CalculateEquipmentStat(equipment, ownedEquipment);
        return true;
    }

    // 장비 원본 데이터와 보유 장비 상태를 이용해 능력치 계산
    private EquipmentStat CalculateEquipmentStat(EquipmentSO equipment, OwnedEquipmentData ownedEquipment)
    {
        int attack = CalculateEnhancedStat(equipment.Attack, ownedEquipment.EnhancementLevel);
        int defense = CalculateEnhancedStat(equipment.Defense, ownedEquipment.EnhancementLevel);
        int health = CalculateEnhancedStat(equipment.Health, ownedEquipment.EnhancementLevel);

        return new EquipmentStat(attack, defense, health);
    }

    // 장비 기본 능력치에 강화 단계 적용
    private int CalculateEnhancedStat(int baseStat, int enhancementLevel)
    {
        return baseStat;
    }
}