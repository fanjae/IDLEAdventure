using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AreaSkillDamage : MonoBehaviour
{
    private float areaRadius;

    private IObjectPool<AreaSkillDamage> pool;
    private UnitSkill poolOwner;

    private ParticleSystem[] particles;
    private Coroutine effectRoutine;


    private void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>(true);
    }
    private void OnDisable()
    {
        if (effectRoutine != null)
        {
            StopCoroutine(effectRoutine);
            effectRoutine = null;
        }
    }


    public void SetPool(IObjectPool<AreaSkillDamage> pool, UnitSkill poolOwner)
    {
        this.pool = pool;
        this.poolOwner = poolOwner;
    }
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
            int appliedDamage = target.TakeDamage(finalDamage, owner);
            //확인용
            Debug.Log($"[광역 스킬] {owner.name} -> {target.name} / 피해량 : {appliedDamage}");
        }
    }
    public void PlayEffect()
    {
        if (effectRoutine != null)
        {
            StopCoroutine(effectRoutine);
            effectRoutine = null;
        }

        RestartParticles();
        effectRoutine = StartCoroutine(EffectRoutine());
    }
    private void RestartParticles()
    {
        if (particles == null) return;

        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i] == null) continue;
            particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles[i].Play(true);
        }
    }
    private IEnumerator EffectRoutine()
    {
        //재생이 실제로 시작될 한 프레임 확보
        yield return null;

        while (HasAliveParticle()) yield return null;
        effectRoutine = null;
        Finish();
    }
    private bool HasAliveParticle()
    {
        if (particles == null || particles.Length == 0) return false;

        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i] != null && particles[i].IsAlive(true)) return true;
        }
        return false;
    }
    private void Finish()
    {
        areaRadius = 0.0f;
        if (pool != null && poolOwner != null)
        {
            pool.Release(this);
            return;
        }
        Destroy(gameObject);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, areaRadius);
    }
}
