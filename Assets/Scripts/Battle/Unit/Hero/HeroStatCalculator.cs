using System;

public sealed class HeroStatCalculator
{
    private readonly EquipmentStatCalculator equipmentStatCalculator;

    public HeroStatCalculator(EquipmentStatCalculator equipmentStatCalculator)
    {
        this.equipmentStatCalculator = equipmentStatCalculator ?? throw new ArgumentNullException(nameof(equipmentStatCalculator));
    }

    // 영웅의 기본 능력치, 레벨 성장치, 클래스 장비 능력치를 합산
    public HeroStat Calculate(OwnedHeroData hero)
    {
        if (hero == null)
        {
            throw new ArgumentNullException(nameof(hero));
        }

        HeroData heroData = hero.HeroData;
        int levelIncrease = Math.Max(0, hero.Level - 1);

        int maxHp = heroData.MaxHp + heroData.HpPerLevel * levelIncrease;
        int attack = heroData.Attack + heroData.AttackPerLevel * levelIncrease;
        int defense = heroData.Defense + heroData.DefensePerLevel * levelIncrease;

        EquipmentStat equipmentStat = equipmentStatCalculator.Calculate(heroData.ClassType);

        maxHp += equipmentStat.Health;
        attack += equipmentStat.Attack;
        defense += equipmentStat.Defense;

        return new HeroStat(Math.Max(1, maxHp), Math.Max(0, attack), Math.Max(0, defense));
    }
}