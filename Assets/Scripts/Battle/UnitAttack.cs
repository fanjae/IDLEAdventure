using UnityEngine;

public class UnitAttack : MonoBehaviour
{
    [Header("공격 설정")]
    [SerializeField] private bool useAnimationEvent;

    [Header("원거리 공격 설정")]
    [SerializeField] private RangedProjectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    private BattleUnit unit;
    //공격 애니메이션의 공격?타격? 시점까지 공격 대상을 보관?저장?하는 용
    private BattleUnit pendingTarget;

    private AttackType attackType;

    private int attackPower;
    private float attackSpeed;
    private float nextAttackTime;
    private bool hasLoggedMissingProjectile;//프리펩 누락 오류가 매 프레임 출력되는 것을 방지하는 용

    public void Initialize(BattleUnit unit, AttackType attackType, int attackPower, float attackSpeed)
    {
        this.unit = unit;
        this.attackType = attackType;
        this.attackPower = Mathf.Max(0, attackPower);
        this.attackSpeed = Mathf.Max(0.1f, attackSpeed);

        pendingTarget = null;
        nextAttackTime = 0.0f;
        hasLoggedMissingProjectile = false;
    }


    public bool TryAttack(BattleUnit target)
    {
        if (unit == null || target == null) return false;
        if (unit.IsDead || target.IsDead) return false;
        if (attackType == AttackType.Ranged && projectilePrefab == null)
        {
            LogMissingProjectile();
            return false;
        }
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
        if (attackType == AttackType.Ranged)
        {
            FireProjectile(target, finalDamage);
            return;
        }
        int appliedDamage = target.TakeDamage(finalDamage);
        Debug.Log($"{unit.name} -> {target.name} / " + $"피해량 : {appliedDamage}");
    }
    private void FireProjectile(BattleUnit target, int damage)
    {
        if (projectilePrefab == null)
        {
            LogMissingProjectile();
            return;
        }
        //발사 위치를 지정했으면 해당 위치에서 발사하고, 지정하지 않았으면 유닛 몸 중심쯤에서..
        Vector3 spawnPosition = projectileSpawnPoint != null ? 
                                projectileSpawnPoint.position : transform.position + Vector3.up;
        Quaternion spawnRotation = projectileSpawnPoint != null ?
                                   projectileSpawnPoint.rotation : transform.rotation;
        RangedProjectile projectile = Instantiate(projectilePrefab, spawnPosition, spawnRotation);

        projectile.Initialize(target, damage);
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

    //TryAttack이 매 프레임마다 호출되다보니
    //같은 오류가 콘솔에 계속 쌓이지 않고 한번만 출력되게 하는 용
    private void LogMissingProjectile()
    {
        if (hasLoggedMissingProjectile) return;

        hasLoggedMissingProjectile = true;

        Debug.LogError($"{name} : 원거리 공격용 프리팹이 연결되지 않음.", this);
    }
}
