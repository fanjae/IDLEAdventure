using UnityEngine;
//최종 피해 계산
public static class DamageCalculator
{
    public static int Calculate(int attack, int defense, float damageRatio = 1.0f)
    {
        float attackDamage = attack * damageRatio;
        int finalDamage = Mathf.RoundToInt(attackDamage - defense);
        return Mathf.Max(1, finalDamage);   //최종 피해 = 공격력 * 피해 계수 - 방어력
                                            //최소 피해 = 1
    }
}
