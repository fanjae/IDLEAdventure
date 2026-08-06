using UnityEngine;

public class UnitSkill : MonoBehaviour
{
    [Header("스킬 설정")]
    [SerializeField] private SkillDataSO skillData;

    private BattleUnit unit;
    private float nextSkillTime;
    private bool HasSkill => skillData != null;

    public void Initialize(BattleUnit unit)
    {
        this.unit = unit;
        nextSkillTime = 0.0f;
    }

    public bool CanUseSkill()
    {
        if (unit == null || unit.IsDead || skillData == null) return false;
        if (Time.time < nextSkillTime) return false;

        BattleUnit target = FindSkillTarget();
        return IsValidTarget(target);
    }
    public bool UseSkill()
    {
        if (!CanUseSkill()) return false;

        BattleUnit target = FindSkillTarget();
        if (!IsValidTarget(target)) return false;

        nextSkillTime = Time.time + skillData.Cooldown;

        unit.StopMove();
        unit.CancelAttack();

        switch (skillData.EffectType)
        {
            case SkillEffectType.Damage:
                unit.FaceTarget();
                ApplyDamage(target);
                break;
            case SkillEffectType.Heal:
                ApplyHeal(target);
                break;
        }

        return true;
    }
    public void ResetSkill()
    {
        nextSkillTime = 0.0f;
    }

    private BattleUnit FindSkillTarget()
    {
        if (skillData == null) return null;

        switch (skillData.EffectType)
        {
            case SkillEffectType.Damage:
                return unit.Target;
            case SkillEffectType.Heal:
                if (BattleManager.Instance == null) return null;
                return BattleManager.Instance.GetLowestHpAlly(unit);
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
}
