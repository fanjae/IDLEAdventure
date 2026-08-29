using UnityEngine;
//최종 피해 계산
public static class DamageCalculator
{
    public static int Calculate(int attack, int defense, float damageRatio = 1.0f)
    {
        attack = Mathf.Max(0, attack);
        defense = Mathf.Max(0, defense);
        damageRatio = Mathf.Max(0.0f, damageRatio);
        if (attack <= 0 || damageRatio <= 0.0f) return 0;

        float attackDamage = attack * damageRatio;
        int finalDamage = Mathf.RoundToInt(attackDamage - defense);
        return Mathf.Max(1, finalDamage);   //최종 피해 = 공격력 * 피해 계수 - 방어력
                                            //최소 피해 = 1
    }
}
