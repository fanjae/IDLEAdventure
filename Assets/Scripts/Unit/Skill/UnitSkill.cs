using UnityEngine;

public class UnitSkill : MonoBehaviour
{
    [Header("스킬 설정")]
    [SerializeField] private SkillDataSO skillData;

    [Header("투사체 스킬 발사 위치")]
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("시전자 VFX 위치")]
    [SerializeField] private Transform skillVfxPoint;

    private BattleUnit unit;
    private BattleUnit pendingTarget; //스킬 시작 당시의 대상을 보관하는 용

    private UnitBuff unitBuff;

    private float nextSkillTime;
    private float skillEndTime;

    private bool isUsingSkill; //스킬 사용 중인지 확인
    private bool effectApplied; //한 번의 스킬에서 효과가 중복 적용되는 것 방지용

    public bool IsUsingSkill => isUsingSkill;

    public void Initialize(BattleUnit unit)
    {
        this.unit = unit;

        unitBuff = GetComponent<UnitBuff>();

        pendingTarget = null;
        isUsingSkill = false;
        effectApplied = false;
        skillEndTime = 0.0f;

        if (skillData == null)
        {
            nextSkillTime = 0.0f;
            return;
        }
        nextSkillTime = Time.time + skillData.Cooldown;
    }

    public bool CanUseSkill()
    {
        if (unit == null || unit.IsDead || skillData == null || isUsingSkill) return false;
        if (Time.time < nextSkillTime) return false;

        BattleUnit target = FindSkillTarget();
        return IsValidTarget(target);
    }
    public bool UseSkill()
    {
        if (!CanUseSkill()) return false;

        BattleUnit target = FindSkillTarget();
        if (!IsValidTarget(target)) return false;
        //애니메이션을 시작할 수 없으면, 스킬 사용 상태도 시작하지 않음
        if (!unit.TryPlaySkillAnimation()) return false;

        pendingTarget = target;

        isUsingSkill = true;
        effectApplied = false;

        nextSkillTime = Time.time + skillData.Cooldown;
        //End 이벤트 누락 대비용
        skillEndTime = Time.time + skillData.ActionDuration;

        unit.StopMove();
        return true;
    }
    public void UpdateSkill()
    {
        if (!isUsingSkill) return;

        if (Time.time < skillEndTime) return;

        //확인용
        Debug.LogWarning($"{name} : SkillEnd Event 가 호출되지 않아서 스킬 상태를 복구함.", this);

        CompleteSkill();
    }
    //SkillActivate 애니메이션 이벤트에서 호출하는 용
    public void ApplySkillEffect()
    {
        if (!isUsingSkill || effectApplied || skillData == null) return;

        switch (skillData.EffectType)
        {
            case SkillEffectType.Damage:
                if (!IsValidTarget(pendingTarget)) return;
                effectApplied = true;
                unit.FaceTarget();
                ApplyDamage(pendingTarget);
                break;
            case SkillEffectType.Heal:
                if (!IsValidTarget(pendingTarget)) return;
                effectApplied = true;
                ApplyHeal(pendingTarget);
                break;
            case SkillEffectType.Barrier:
                effectApplied = true;
                ApplyBarrier();
                break;
            case SkillEffectType.ProjectileDamage:
                if (!IsValidTarget(pendingTarget)) return;
                effectApplied = true;
                unit.FaceTarget();
                FireProjectile(pendingTarget);
                break;
            case SkillEffectType.Buff:
                effectApplied = true;
                ApplyBuff();
                break;
        }
    }
    //SkillEnd 애니메이션 이벤트에서 호출하는 용
    public void CompleteSkill()
    {
        if (!isUsingSkill) return;

        isUsingSkill = false;
        effectApplied = false;
        pendingTarget = null;
        skillEndTime = 0.0f;
    }

    public void CancelSkill()
    {
        isUsingSkill = false;
        effectApplied = false;
        pendingTarget = null;
        skillEndTime = 0.0f;
    }
    public void ResetSkill()
    {
        isUsingSkill = false;
        effectApplied = false;
        pendingTarget = null;
        skillEndTime = 0.0f;

        if (skillData == null)
        {
            nextSkillTime = 0.0f;
            return;
        }

        nextSkillTime = Time.time + skillData.Cooldown;
    }

    private BattleUnit FindSkillTarget()
    {
        if (unit == null || skillData == null) return null;

        switch (skillData.EffectType)
        {
            case SkillEffectType.Damage:
                return unit.Target;
            case SkillEffectType.Heal:
                if (BattleManager.Instance == null) return null;
                return BattleManager.Instance.GetLowestHpAlly(unit);
            case SkillEffectType.Barrier:
                return unit;
            case SkillEffectType.ProjectileDamage:
                return unit.Target;
            case SkillEffectType.Buff:
                return unit;
        }

        return null;
    }
    private bool IsValidTarget(BattleUnit target)
    {
        if (target == null || target.IsDead || !target.gameObject.activeInHierarchy) return false;

        switch (skillData.EffectType)
        {
            case SkillEffectType.Damage:
                //피해를 입히는 스킬은 적대 진영에게만 사용
                return target.Team != unit.Team && unit.IsTargetInAttackRange(target);
            case SkillEffectType.Heal:
                //회복 스킬은 같은 진영의 체력이 감소한 유닛에게만 사용
                return target.Team == unit.Team && target.CurrentHp < target.MaxHp;
            case SkillEffectType.Barrier:
                return target == unit;
            case SkillEffectType.ProjectileDamage://사거리는 공격 사거리와 동일하게 설정하였음.(굳이 다를 필요가 없을 것 같음)
                return target.Team != unit.Team && unit.IsTargetInAttackRange(target);
            case SkillEffectType.Buff:
                return target == unit;
        }

        return false;
    }
    private void ApplyDamage(BattleUnit target)
    {
        int skillAttack = Mathf.RoundToInt(unit.AttackPower * skillData.DamageRatio);
        int finalDamage = DamageCalculator.Calculate(skillAttack, target.Defense);

        int appliedDamage = target.TakeDamage(finalDamage);
        //기능 확인 용
        Debug.Log($"{unit.name} 스킬 사용 / {target.name} 피해 : {appliedDamage}");
    }
    private void ApplyHeal(BattleUnit target)
    {
        if (target == null || target.IsDead) return;

        //시전자 발밑 VFX
        PlayHealCastVfx();

        int healAmount = Mathf.RoundToInt(unit.AttackPower * skillData.DamageRatio);
        int appliedHeal = target.Heal(healAmount);

        //회복된 경우에만 대상 힐 VFX 표시
        if (appliedHeal > 0) PlayHealTargetVfx(target);
        //기능 확인 용
        Debug.Log($"[힐 스킬] {unit.name} -> {target.name} / 회복량 : {appliedHeal}");
    }
    private void ApplyBarrier()
    {
        unit.ActivateBarrier(skillData.BlockCount, skillData.BarrierVfxPrefab);
    }
    private void ApplyBuff()
    {
        if (unitBuff == null) return;

        unitBuff.ApplyAttackBuff(skillData.AttackBuff, skillData.BuffDuration, skillData.BuffVfxPrefab, skillVfxPoint);
    }
    //투사체 스킬 발사 함수
    private void FireProjectile(BattleUnit target)
    {
        if (target == null || skillData.ProjectilePrefab == null) return;

        Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position + Vector3.up;
        //발사하는 순간 타겟이 있던 방향으로
        Vector3 direction = target.transform.position - spawnPosition;
        direction.y = 0.0f;
        if (direction.sqrMagnitude <= 0.001f) direction = unit.transform.forward;
        direction.Normalize();

        int skillAttack = Mathf.RoundToInt(unit.AttackPower * skillData.DamageRatio);
        SkillProjectile projectile = Instantiate(skillData.ProjectilePrefab, spawnPosition, Quaternion.identity);
        projectile.Initialize(unit, direction, skillAttack, skillData.ProjectileSpeed);
    }
    //힐 관련
    private void PlayHealCastVfx()
    {
        if (skillData.HealCastVfxPrefab == null) return;

        Transform point = skillVfxPoint != null ? skillVfxPoint : transform;
        GameObject vfx = Instantiate(skillData.HealCastVfxPrefab, point.position, point.rotation, point);

        Destroy(vfx, skillData.HealVfxDuration);
    }
    private void PlayHealTargetVfx(BattleUnit target)
    {
        if (target == null) return;
        if (skillData.HealTargetVfxPrefab == null) return;

        GameObject vfx = Instantiate(skillData.HealTargetVfxPrefab, target.transform.position, Quaternion.identity, target.transform);

        Destroy(vfx, skillData.HealVfxDuration);
    }
}
