using System;

// 장비 능력치
[Serializable]
public readonly struct EquipmentStat
{
    public static EquipmentStat Zero => new (0, 0, 0);

    public int Attack { get; }
    public int Defense { get; }
    public int Health { get; }

    public EquipmentStat(int attack, int defense, int health)
    {
        Attack = Math.Max(0, attack);
        Defense = Math.Max(0, defense);
        Health = Math.Max(0, health);
    }

    public static EquipmentStat operator +(EquipmentStat left, EquipmentStat right)
    {
        return new EquipmentStat(left.Attack + right.Attack, left.Defense + right.Defense, left.Health + right.Health);
    }
}
