using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhirlwindSkillDamage : MonoBehaviour
{
    private BattleUnit unit;

    private Coroutine whirlwindCoroutine;
    private GameObject whirlwindVfx;

    public void Initialize(BattleUnit unit)
    {
        this.unit = unit;
    }

    public void StartWhirlwind(float duration, float hitInterval, float radius, float damageRatio, GameObject vfxPrefab, Transform vfxPoint)
    {
        if (unit == null || unit.IsDead) return;
        //만약 기존 실행이 남아있다면 중복 실행 방지
        StopWhirlwind();
        PlayVfx(vfxPrefab, vfxPoint);

        whirlwindCoroutine = StartCoroutine(WhirlwindRoutine(duration, hitInterval, radius, damageRatio));
    }
    public void StopWhirlwind()
    {
        if (whirlwindCoroutine != null)
        {
            StopCoroutine(whirlwindCoroutine);
            whirlwindCoroutine = null;
        }
        StopVfx();
    }
    
    private IEnumerator WhirlwindRoutine(float duration, float hitInterval, float radius, float damageRatio)
    {
        float endTime = Time.time + duration;
        while (Time.time < endTime)
        {
            if (unit == null || unit.IsDead)
            {
                whirlwindCoroutine = null;
                StopVfx();
                yield break;
            }
            ApplyDamage(radius, damageRatio);
            yield return new WaitForSeconds(hitInterval);
        }
        whirlwindCoroutine = null;
        StopVfx();

        unit.SkillEndEvent();
    }
    private void ApplyDamage(float radius, float damageRatio)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        HashSet<BattleUnit> hitUnits = new HashSet<BattleUnit>();
        for (int i = 0; i < hits.Length; i++)
        {
            BattleUnit target = hits[i].GetComponentInParent<BattleUnit>();

            if (target == null) continue;
            if (target == unit) continue;
            if (target.IsDead) continue;
            if (target.Team == unit.Team) continue;
            if (!hitUnits.Add(target)) continue;

            int skillAttack = Mathf.RoundToInt(unit.AttackPower * damageRatio);
            int finalDamage = DamageCalculator.Calculate(skillAttack, target.Defense);
            int appliedDamage = target.TakeDamage(finalDamage);
            //확인용
            Debug.Log($"[휠윈드] {unit.name} -> {target.name} / 피해량 : {appliedDamage}");
        }
    }
    private void PlayVfx(GameObject vfxprefab, Transform vfxPoint)
    {
        if (vfxprefab == null) return;

        Transform point = vfxPoint != null ? vfxPoint : transform;
        whirlwindVfx = Instantiate(vfxprefab, point.position, point.rotation, point);
    }
    private void StopVfx()
    {
        if (whirlwindVfx == null) return;
        Destroy(whirlwindVfx);
        whirlwindVfx = null;
    }


    private void OnDrawGizmosSelected()
    {
        if (unit == null) return;
    }
}
