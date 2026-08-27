using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class LaserSkillDamage : MonoBehaviour
{
    private BattleUnit owner;

    private float duration;
    private float hitInterval;
    private float damageRatio;

    private readonly Dictionary<BattleUnit, float> nextHitTimes = new Dictionary<BattleUnit, float>();

    private IObjectPool<LaserSkillDamage> pool;
    private UnitSkill poolOwner;
    private Coroutine laserRoutine;


    public void SetPool(IObjectPool<LaserSkillDamage> pool, UnitSkill poolOwner)
    {
        this.pool = pool;
        this.poolOwner = poolOwner;
    }

    public void Initialize(BattleUnit owner, float duration, float hitInterval, float damageRatio)
    {
        if (owner == null)
        {
            Finish(false);
            return;
        }
        if (laserRoutine != null)
        {
            StopCoroutine(laserRoutine);
            laserRoutine = null;
        }

        this.owner = owner;
        this.duration = duration;
        this.hitInterval = hitInterval;
        this.damageRatio = damageRatio;
        nextHitTimes.Clear();

        laserRoutine = StartCoroutine(LaserRoutine());
    }

    private IEnumerator LaserRoutine()
    {
        float endTime = Time.time + duration;
        while (Time.time < endTime)
        {
            if (owner == null || owner.IsDead || !owner.IsUsingSkill)
            {
                laserRoutine = null;
                Finish(false);
                yield break;
            }
            yield return null;
        }
        if (owner != null && !owner.IsDead && owner.IsUsingSkill) owner.SkillEndEvent();

        laserRoutine = null;
        Finish(false);
    }
    private void Finish(bool stopRoutine)
    {
        if (stopRoutine && laserRoutine != null)
        {
            StopCoroutine(laserRoutine);
            laserRoutine = null;
        }

        owner = null;
        duration = 0.0f;
        hitInterval = 0.0f;
        damageRatio = 0.0f;
        nextHitTimes.Clear();

        if (pool != null && poolOwner != null)
        {
            pool.Release(this);
            return;
        }
        Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (owner == null || owner.IsDead || !owner.IsUsingSkill) return;

        BattleUnit target = other.GetComponentInParent<BattleUnit>();
        if (target == null) return;
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

        target.TakeDamage(finalDamage, owner);
    }
    private void OnTriggerExit(Collider other)
    {
        BattleUnit target = other.GetComponentInParent<BattleUnit>();
        if (target == null) return;
        nextHitTimes.Remove(target);
    }
    private void OnDisable()
    {
        if (laserRoutine != null)
        {
            StopCoroutine(laserRoutine);
            laserRoutine = null;
        }
        nextHitTimes.Clear();
    }
}
