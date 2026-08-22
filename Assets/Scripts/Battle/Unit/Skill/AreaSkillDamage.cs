using System.Collections.Generic;
using UnityEngine;

public class AreaSkillDamage : MonoBehaviour
{
    private float areaRadius;

    public void SetAreaRadius(float radius)
    {
        areaRadius = Mathf.Max(0.1f, radius);
    }

    public void ApplyDamage(BattleUnit owner, int skillAttack)
    {
        if (owner == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, areaRadius);
        HashSet<BattleUnit> hitUnits = new HashSet<BattleUnit>();
        for (int i = 0; i < hits.Length; i++)
        {
            BattleUnit target = hits[i].GetComponentInParent<BattleUnit>();
            if (target == null) continue;
            if (target == owner) continue;
            if (target.IsDead) continue;
            if (target.Team == owner.Team) continue;
            //같은 유닛의 Collider가 여러 개 잡혀도 한 번만 피해
            if (!hitUnits.Add(target)) continue;

            int finalDamage = DamageCalculator.Calculate(skillAttack, target.Defense);
            int appliedDamage = target.TakeDamage(finalDamage);
            //확인용
            Debug.Log($"[광역 스킬] {owner.name} -> {target.name} / 피해량 : {appliedDamage}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, areaRadius);
    }
}
