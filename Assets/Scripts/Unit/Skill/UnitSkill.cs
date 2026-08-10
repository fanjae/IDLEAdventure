using UnityEngine;

public class UnitSkill : MonoBehaviour
{
    [Header("스킬 설정")]
    [SerializeField] private SkillDataSO skillData;

    [Header("투사체 스킬 발사 위치")]
    [SerializeField] private Transform projectileSpawnPoint;

    private BattleUnit unit;
    private BattleUnit pendingTarget; //스킬 시작 당시의 대상을 보관하는 용

    private float nextSkillTime;
    private float skillEndTime;

    private bool isUsingSkill; //스킬 사용 중인지 확인
    private bool effectApplied; //한 번의 스킬에서 효과가 중복 적용되는 것 방지용

    public bool IsUsingSkill => isUsingSkill;

    public void Initialize(BattleUnit unit)
    {
        this.unit = unit;

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
        skillEndTime = Time.time + skillData.ActionDuration;

        unit.StopMove();
        return true;
    }
    public void UpdataSkill()
    {
        if (!isUsingSkill) return;

        if (Time.time >= skillEndTime)
        {
            CompleteSkill();
        }
    }
    //SkillActivate 애니메이션 이벤트에서 호출하는 용
    public void ApplySkillEffect()
    {
        if (!isUsingSkill || effectApplied || skillData == null) return;
        //같은 스킬 Event가 중복되어도 한 번만 적용
        effectApplied = true;

        switch (skillData.EffectType)
        {
            case SkillEffectType.Damage:
                if (!IsValidTarget(pendingTarget)) return;
                unit.FaceTarget();
                ApplyDamage(pendingTarget);
                break;
            case SkillEffectType.Heal:
                if (!IsValidTarget(pendingTarget)) return;
                ApplyHeal(pendingTarget);
                break;
            case SkillEffectType.Barrier:
                ApplyBarrier();
                break;
            case SkillEffectType.ProjectileDamage:
                if (!IsValidTarget(pendingTarget)) return;
                unit.FaceTarget();
                FireProjectile(pendingTarget);
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
        }

        return false;
    }
    private void ApplyDamage(BattleUnit target)
    {
        int skillAttack = Mathf.RoundToInt(unit.AttackPower * skillData.DamageRatio);
        int finalDamage = DamageCalculator.Calculate(skillAttack, target.Defense);

        int appliedDamage = target.TakeDamage(finalDamage);
        //기능 확인 용
        Debug.Log($"{unit.name} 스킬 사용 / " + $"{target.name} 피해 : {appliedDamage}");
    }
    private void ApplyHeal(BattleUnit target)
    {
        int healAmount = Mathf.RoundToInt(unit.AttackPower * skillData.DamageRatio);
        
        int appliedHeal = target.Heal(healAmount);
        //기능 확인 용
        Debug.Log($"{unit.name} 회복 스킬 / " + $"{target.name} 회복 : {appliedHeal}");
    }
    private void ApplyBarrier()
    {
        unit.ActivateBarrier(skillData.BlockCount, skillData.BarrierVfxPrefab);
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

}
