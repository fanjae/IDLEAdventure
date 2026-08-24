using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhirlwindSkillDamage : MonoBehaviour
{
    private BattleUnit owner;

    private float duration;
    private float hitInterval;
    private float damageRatio;

    private readonly Dictionary<BattleUnit, float> nextHitTimes = new Dictionary<BattleUnit, float>();

    public void Initialize(BattleUnit owner, float duration, float hitInterval, float damageRatio)
    {
        if (owner == null)
        {
            Destroy(gameObject);
            return;
        }

        this.owner = owner;
        this.duration = duration;
        this.hitInterval = hitInterval;
        this.damageRatio = damageRatio;

        StartCoroutine(WhirlwindRoutine());
    }

    private IEnumerator WhirlwindRoutine()
    {
        float endTime = Time.time + duration;
        while (Time.time < endTime)
        {
            if (owner == null || owner.IsDead || !owner.IsUsingSkill)
            {
                Destroy(gameObject);
                yield break;
            }
            yield return null;
        }
        if (owner != null && !owner.IsDead && owner.IsUsingSkill) owner.SkillEndEvent();

        Destroy(gameObject);
    }
    private void OnTriggerStay(Collider other)
    {
        if (owner == null || owner.IsDead || !owner.IsUsingSkill) return;

        BattleUnit target = other.GetComponentInParent<BattleUnit>();
        if (target ==  null) return;
        if (target == owner) return;
        if (target.IsDead) return;
        if (target.Team == owner.Team) return;
        if (nextHitTimes.TryGetValue(target, out float nextHitTime))
        {
            if (Time.time < nextHitTime) return;
        }

        nextHitTimes[target] = Time.time + hitInterval;
        int skillAttack = Mathf.RoundToInt(owner.AttackPower * damageRatio);
        int finalDamage = DamageCalculator.Calculate(skillAttack, target.Defense);
        target.TakeDamage(finalDamage);
    }
    private void OnTriggerExit(Collider other)
    {
        BattleUnit target = other.GetComponentInParent<BattleUnit>();
        if (target == null) return;

        nextHitTimes.Remove(target);
    }
}
