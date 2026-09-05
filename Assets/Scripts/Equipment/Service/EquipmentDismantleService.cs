using System;
using System.Collections.Generic;

// 장비 분해 결과
public readonly struct EquipmentDismantleResult
{
    public int DismantledCount { get; }
    public long Gold { get; }
    public long Exp { get; }
    public long Upgrade { get; }
    public long Gem { get; }

    public EquipmentDismantleResult(int dismantledCount, long gold, long exp, long upgrade, long gem)
    {
        DismantledCount = dismantledCount;
        Gold = gold;
        Exp = exp;
        Upgrade = upgrade;
        Gem = gem;
    }
}

// 미장착 장비의 분해 대상 선정과 분해 보상 계산 처리
public sealed class EquipmentDismantleService
{
    private readonly InventoryController inventoryController;
    private readonly ItemDatabaseSO itemDatabase;
    private readonly EquipmentDismantleRewardDataSO dismantleRewardData;

    public EquipmentDismantleService(InventoryController inventoryController, ItemDatabaseSO itemDatabase, EquipmentDismantleRewardDataSO dismantleRewardData)
    {
        this.inventoryController = inventoryController ?? throw new ArgumentNullException(nameof(inventoryController));
        this.itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));
        this.dismantleRewardData = dismantleRewardData ?? throw new ArgumentNullException(nameof(dismantleRewardData));
    }

    // 현재 장착 중인 장비를 제외한 모든 보유 장비 분해
    public EquipmentDismantleResult DismantleUnequippedEquipment()
    {
        List<OwnedEquipmentData> targets = FindUnequippedEquipment();

        int dismantledCount = 0;
        long totalGold = 0;
        long totalExp = 0;
        long totalUpgrade = 0;
        long totalGem = 0;

        foreach (OwnedEquipmentData ownedEquipment in targets)
        {
            if (!TryDismantleEquipment(ownedEquipment, out EquipmentDismantleReward reward))
            {
                continue;
            }

            dismantledCount++;
            totalGold += reward.Gold;
            totalExp += reward.Exp;
            totalUpgrade += reward.Upgrade;
            totalGem += reward.Gem;
        }

        return new EquipmentDismantleResult(dismantledCount, totalGold, totalExp, totalUpgrade, totalGem);
    }

    // 현재 장착 중이지 않은 보유 장비 목록 조회
    private List<OwnedEquipmentData> FindUnequippedEquipment()
    {
        List<OwnedEquipmentData> targets = new();

        foreach (OwnedEquipmentData ownedEquipment in inventoryController.Equipments)
        {
            if (!inventoryController.IsEquipped(ownedEquipment.InstanceId))
            {
                targets.Add(ownedEquipment);
            }
        }

        return targets;
    }

    // 보유 장비 한 개를 분해하고 획득 보상 반환
    private bool TryDismantleEquipment(OwnedEquipmentData ownedEquipment, out EquipmentDismantleReward reward)
    {
        reward = default;

        if (ownedEquipment == null)
        {
            return false;
        }

        if (!itemDatabase.TryGetItem(ownedEquipment.EquipmentId, out EquipmentSO equipment))
        {
            return false;
        }

        if (!dismantleRewardData.TryRollReward(equipment.CraftLevel, out reward))
        {
            return false;
        }

        return inventoryController.TryRemoveOwnedEquipment(ownedEquipment.InstanceId, out _);
    }
}