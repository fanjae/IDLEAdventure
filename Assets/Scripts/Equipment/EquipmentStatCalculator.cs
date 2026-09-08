using System;
using System.Collections.Generic;

// 클래스에 장착된 장비의 전체 능력치 계산

public sealed class EquipmentStatCalculator
{
    private const double TwoSetMultiplier = 1.20;
    private const double FourSetMultiplier = 1.40;
    private const double SixSetMultiplier = 1.60;

    private readonly InventoryController inventoryController;

    private struct EquipmentSetStat
    {
        public int Count;
        public EquipmentStat Stat;

        public EquipmentSetStat(int count, EquipmentStat stat)
        {
            Count = count;
            Stat = stat;
        }
    }

    public EquipmentStatCalculator(InventoryController inventoryController)
    {
        this.inventoryController = inventoryController ?? throw new ArgumentNullException(nameof(inventoryController));
    }

    // 지정한 클래스에 장착된 모든 장비의 능력치 합산
    public EquipmentStat Calculate(HeroClassType heroClass)
    {
        EquipmentStat totalStat = EquipmentStat.Zero;
        Dictionary<EquipmentSetType, EquipmentSetStat> setStats = new();

        // 클래스의 모든 장비 슬롯 확인
        foreach (EquipmentSlotType slotType in Enum.GetValues(typeof(EquipmentSlotType)))
        {
            // 현재 슬롯에 장비가 없는 경우 계산 제외
            if (!TryCalculateSlotStat(heroClass, slotType, out EquipmentStat slotStat, out EquipmentSetType setType))
            {
                continue;
            }

            // 세트에 포함되지 않는 일반 장비는 바로 전체 능력치에 합산
            if (setType == EquipmentSetType.None)
            {
                totalStat += slotStat;
                continue;
            }

            // 세트 장비는 같은 세트끼리 별도로 합산
            AddSetStat(setStats, setType, slotStat);
        }

        // 세트별 장착 개수에 맞는 효과 적용 후 전체 능력치에 합산
        foreach (EquipmentSetStat setStat in setStats.Values)
        {
            double multiplier = GetSetMultiplier(setStat.Count);
            totalStat += ApplySetMultiplier(setStat.Stat, multiplier);
        }

        return totalStat;
    }

    // 지정한 클래스와 슬롯에 장착된 장비의 능력치와 세트 정보 계산
    private bool TryCalculateSlotStat(HeroClassType heroClass, EquipmentSlotType slotType, out EquipmentStat equipmentStat, out EquipmentSetType setType)
    {
        equipmentStat = EquipmentStat.Zero;
        setType = EquipmentSetType.None;

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
        setType = equipment.SetType;

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

    // 강화 능력치는 현재 적용하지 않으므로 기본 능력치 반환 (더미 데이터)
    private int CalculateEnhancedStat(int baseStat, int enhancementLevel)
    {
        return baseStat;
    }

    // 동일 세트 장비의 장착 개수와 능력치 합산
    private void AddSetStat(Dictionary<EquipmentSetType, EquipmentSetStat> setStats, EquipmentSetType setType, EquipmentStat equipmentStat)
    {
        if (setStats.TryGetValue(setType, out EquipmentSetStat currentSetStat))
        {
            currentSetStat.Count++;
            currentSetStat.Stat += equipmentStat;
            setStats[setType] = currentSetStat;
            return;
        }

        setStats.Add(setType, new EquipmentSetStat(1, equipmentStat));
    }

    // 동일 세트 장착 개수에 따른 능력치 배율 반환
    private double GetSetMultiplier(int setCount)
    {
        if (setCount >= 6)
        {
            return SixSetMultiplier;
        }

        if (setCount >= 4)
        {
            return FourSetMultiplier;
        }

        if (setCount >= 2)
        {
            return TwoSetMultiplier;
        }

        return 1.0;
    }

    // 동일 세트 장비의 전체 능력치에 세트 효과 적용
    private EquipmentStat ApplySetMultiplier(EquipmentStat equipmentStat, double multiplier)
    {
        if (multiplier <= 1.0)
        {
            return equipmentStat;
        }

        int attack = CalculateSetBonusStat(equipmentStat.Attack, multiplier);
        int defense = CalculateSetBonusStat(equipmentStat.Defense, multiplier);
        int health = CalculateSetBonusStat(equipmentStat.Health, multiplier);

        return new EquipmentStat(attack, defense, health);
    }

    // 세트 효과 적용 후 정수 능력치 계산
    private int CalculateSetBonusStat(int stat, double multiplier)
    {
        return (int)Math.Round(stat * multiplier, MidpointRounding.AwayFromZero);
    }
}