using UnityEngine;

public class UnitAttack : MonoBehaviour
{
    [Header("공격 설정")]
    [SerializeField] private bool useAnimationEvent;

    private BattleUnit unit;
    //공격 애니메이션의 공격?타격? 시점까지 공격 대상을 보관?저장?하는 용
    private BattleUnit pendingTarget;

    private int attackPower;
    private float attackSpeed;
    private float nextAttackTime;

    public void Initialize(BattleUnit unit, int attackPower, float attackSpeed)
    {
        this.unit = unit;
        this.attackPower = Mathf.Max(0, attackPower);
        this.attackSpeed = Mathf.Max(0.1f, attackSpeed);

        pendingTarget = null;
        nextAttackTime = 0.0f;
    }


    public bool TryAttack(BattleUnit target)
    {
        if (unit == null || target == null) return false;
        if (unit.IsDead || target.IsDead) return false;
        if (Time.time < nextAttackTime) return false;

        nextAttackTime = Time.time + GetAttackInterval();
        pendingTarget = target;

        unit.PlayAttackAnimation();
        if (!useAnimationEvent) ApplyAttackDamage();

        return true;
    }
    //공격 애니메이션의 실제 타격 프레임에서 호출
    public void ApplyAttackDamage()
    {
        BattleUnit target = pendingTarget;
        pendingTarget = null;

        if (unit == null || target == null)
        {
            Debug.LogWarning($"{name} : 공격자 또는 대상이 없음.", this);
            return;
        }
        if (unit.IsDead || target.IsDead) return;
        if (!unit.IsTargetInAttackRange(target, 0.3f))
        {
            Debug.Log($"{unit.name} : {target.name}이 공격 범위를 벗어남.");
            return;
        }

        int finalDamage = DamageCalculator.Calculate(attackPower, target.Defense);
        int appliedDamage = target.TakeDamage(finalDamage);
        Debug.Log($"{unit.name} -> {target.name} / " + $"피해량 : {appliedDamage}");
    }
    //추후에 만드신다는 EquipmentController나 레벨 시스템이 계산할 때
    //최종 공격력을 적용할 때 사용(필요없으시다면 안 써도 됩니다.)
    public void SetAttackPower(int newAttackPower)
    {
        attackPower = Mathf.Max(0, newAttackPower);
    }
    //상태가 변경되거나 사망 시 예약된 공격 취소
    public void CancelAttack()
    {
        pendingTarget = null;
    }
    //전투 재시작하거나 오브젝트 풀 재사용 시 공격 대상과 쿨 초기화
    public void ResetAttack()
    {
        pendingTarget = null;
        nextAttackTime = 0.0f;
    }
    private float GetAttackInterval()
    {
        return 1.0f / attackSpeed;  //공속이 1이면 1초마다 공격.  2면 0.5초마다 공격.
    }
}
