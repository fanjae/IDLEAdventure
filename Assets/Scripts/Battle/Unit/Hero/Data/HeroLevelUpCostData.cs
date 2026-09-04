using System;
using UnityEngine;

[Serializable]
public sealed class HeroLevelUpCostData
{
    [SerializeField] private int level;
    [SerializeField] private int goldCost;
    [SerializeField] private int expCost;
    [SerializeField] private int upgradeCost;

    public int Level => level;
    public int GoldCost => goldCost;
    public int ExpCost => expCost;
    public int UpgradeCost => upgradeCost;
}
