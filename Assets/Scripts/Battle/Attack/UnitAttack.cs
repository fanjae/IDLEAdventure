using UnityEngine;

public class UnitAttack : MonoBehaviour
{
    private const float AttackHitExtraRange = 0.3f;//공격 타격 판정에 추가로 허용할 거리

    [Header("원거리 공격 설정")]
    [SerializeField] private RangedProjectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("공격 안전 장치 설정")]
    [SerializeField, Min(0.5f)] private float attackSafetyDuration = 3.0f;

    private BattleUnit unit;
    //공격 애니메이션의 공격?타격? 시점까지 공격 대상을 보관?저장?하는 용
    private BattleUnit attackTarget;

    private AttackType attackType;

    private int attackPower;
    private float attackSpeed;

    private float nextAttackAvailableTime;
    private float attackSafetyEndTime;

    //공격 애니메이션이 진행 중인지 체크하는 용
    private bool isAttacking;
    //실제로 Attack Animator State에 진입했는지 체크하는 용
    private bool hasEnteredAttackAnimation;
    //중복 타격 방지용
    private bool hasAppliedDamage;

    private bool hasLoggedMissingProjectile;//원거리 공격 프리팹 누락 로그가 반복 출력되는 것을 방지

    public bool IsAttacking => isAttacking;

    public void Initialize(BattleUnit unit, AttackType attackType, int attackPower, float attackSpeed)
    {
        this.unit = unit;
        this.attackType = attackType;
        this.attackPower = Mathf.Max(0, attackPower);
        this.attackSpeed = Mathf.Max(0.1f, attackSpeed);

        attackTarget = null;

        nextAttackAvailableTime = 0.0f;
        attackSafetyEndTime = 0.0f;

        isAttacking = false;
        hasEnteredAttackAnimation = false;
        hasAppliedDamage = false;

        hasLoggedMissingProjectile = false;
    }

    private void Update()
    {
        if (!isAttacking) return;
        if (unit == null || unit.IsDead)
        {
            CancelAttack();
            return;
        }
        
        bool attackAnimationActive = unit.IsAttackAnimationActive();
        //실제 Attack 애니메이션 상태에 진입했는지 확인
        if (!hasEnteredAttackAnimation)
        {
            if (attackAnimationActive) hasEnteredAttackAnimation = true;
            CheckAttackSafety();
            return;
        }
        //Attack 상태에 들어간 이후, Attack 상태를 완전히 빠져나오면 공격 종료
        if (!attackAnimationActive)
        {
            CompleteAttack();
            return;
        }
        CheckAttackSafety();
    }

    private void CheckAttackSafety()
    {
        if (Time.time < attackSafetyEndTime) return;
        Debug.LogWarning($"{name} : 공격 애니메이션 종료를 확인하지 못해서 공격 상태를 복구함", this);
        CompleteAttack();
    }

    public bool TryAttack(BattleUnit target)
    {
        if (unit == null || target == null) return false;
        if (unit.IsDead || target.IsDead) return false;
        if (isAttacking) return false;
        if (Time.time < nextAttackAvailableTime) return false;
        if (attackType == AttackType.Ranged && projectilePrefab == null)
        {
            LogMissingProjectile();
            return false;
        }
        if (!unit.TryPlayAttackAnimation()) return false;

        attackTarget = target;

        isAttacking = true;     
        hasEnteredAttackAnimation = false;
        hasAppliedDamage = false;

        //공격 시작 시점을 기준으로 다음 공격 가능 시간 계산
        nextAttackAvailableTime = Time.time + GetAttackInterval();
        //AttackEnd 애니메이션 이벤트 누락 대비
        attackSafetyEndTime = Time.time + attackSafetyDuration;
        return true;
    }
    //AttackHit 애니메이션 이벤트에서 호출
    public void ApplyAttackDamage()
    {
        if (!isAttacking) return;
        if (hasAppliedDamage) return;

        BattleUnit target = attackTarget;

        if (unit == null || target == null) return;
        if (unit.IsDead || target.IsDead) return;
        if (!unit.IsTargetInAttackRange(target, AttackHitExtraRange)) return;

        hasAppliedDamage = true;

        int finalDamage = DamageCalculator.Calculate(attackPower, target.Defense);
        if (attackType == AttackType.Ranged)
        {
            FireProjectile(target, finalDamage);
            return;
        }
        int appliedDamage = target.TakeDamage(finalDamage);
        Debug.Log($"{unit.name} -> {target.name} / 피해량 : {appliedDamage}");
    }
    //공격 종료 시 공격 상태 및 대상 정보 정리
    public void CompleteAttack()
    {
        if (!isAttacking) return;

        isAttacking = false;
        hasEnteredAttackAnimation = false;
        hasAppliedDamage = false;

        attackTarget = null;
        attackSafetyEndTime = 0.0f;
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
    //외부 능력치 변경 시 실제 기본 공격력 갱신
    public void SetAttackPower(int newAttackPower)
    {
        attackPower = Mathf.Max(0, newAttackPower);
    }
    //상태 변경 또는 사망 등으로 진행 중인 공격 취소
    public void CancelAttack()
    {
        attackTarget = null;

        isAttacking = false;
        hasEnteredAttackAnimation = false;
        hasAppliedDamage = false;

        attackSafetyEndTime = 0.0f;
    }
    //전투 재시작 또는 오브젝트 재사용 시 공격 상태 초기화
    public void ResetAttack()
    {
        attackTarget = null;

        nextAttackAvailableTime = 0.0f;
        attackSafetyEndTime = 0.0f;

        isAttacking = false;
        hasEnteredAttackAnimation = false;
        hasAppliedDamage = false;
    }
    private float GetAttackInterval()
    {
        return 1.0f / attackSpeed;  //공속 1 = 1초 / 공속 2 = 0.5초
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
