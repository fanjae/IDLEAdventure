public readonly struct HeroStat
{
    public int MaxHp { get; }
    public int Attack { get; }
    public int Defense { get; }

    public HeroStat(int maxHp, int attack, int defense)
    {
        MaxHp = maxHp;
        Attack = attack;
        Defense = defense;
    }
}