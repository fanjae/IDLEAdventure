using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentDismantleRewardData", menuName = "Game Data/Equipment/Dismantle Reward Data")]
public sealed class EquipmentDismantleRewardDataSO : ScriptableObject
{
    [Serializable]
    private sealed class RewardTier
    {
        [SerializeField, Min(1)] private int craftLevel = 10;
        [SerializeField, Min(0)] private int gold;
        [SerializeField, Min(0)] private int exp;
        [SerializeField, Min(0)] private int upgrade;
        [SerializeField, Range(0f, 1f)] private float gemChance;
        [SerializeField, Min(0)] private int minGem;
        [SerializeField, Min(0)] private int maxGem;

        public int CraftLevel => craftLevel;
        public int Gold => gold;
        public int Exp => exp;
        public int Upgrade => upgrade;
        public float GemChance => gemChance;
        public int MinGem => minGem;
        public int MaxGem => maxGem;
    }

    [SerializeField] private List<RewardTier> rewardTiers = new();

    // 정확히 일치하는 제작 레벨을 우선 사용하고, 새 레벨이 추가되면 가장 가까운 하위 구간을 사용한다.
    public bool TryRollReward(int craftLevel, out EquipmentDismantleReward reward)
    {
        RewardTier selectedTier = null;

        foreach (RewardTier tier in rewardTiers)
        {
            if (tier == null || tier.CraftLevel > craftLevel)
            {
                continue;
            }

            if (selectedTier == null || tier.CraftLevel > selectedTier.CraftLevel)
            {
                selectedTier = tier;
            }
        }

        if (selectedTier == null)
        {
            reward = default;
            return false;
        }

        int gem = 0;
        int minGem = Mathf.Min(selectedTier.MinGem, selectedTier.MaxGem);
        int maxGem = Mathf.Max(selectedTier.MinGem, selectedTier.MaxGem);

        if (maxGem > 0 && UnityEngine.Random.value < selectedTier.GemChance)
        {
            gem = UnityEngine.Random.Range(minGem, maxGem + 1);
        }

        reward = new EquipmentDismantleReward(
            selectedTier.Gold,
            selectedTier.Exp,
            selectedTier.Upgrade,
            gem);
        return true;
    }
}

public readonly struct EquipmentDismantleReward
{
    public int Gold { get; }
    public int Exp { get; }
    public int Upgrade { get; }
    public int Gem { get; }

    public EquipmentDismantleReward(int gold, int exp, int upgrade, int gem)
    {
        Gold = gold;
        Exp = exp;
        Upgrade = upgrade;
        Gem = gem;
    }
}
