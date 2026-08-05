using UnityEngine;

[RequireComponent(typeof(UnitHealth))]
[RequireComponent(typeof(UnitMovement))]
[RequireComponent(typeof(UnitAttack))]
public class BattleUnit : MonoBehaviour
{
    [Header("유닛 설정")]
    [SerializeField] private UnitDataSO unitData;
    [SerializeField] private UnitTeam team;
    [Header("회전 설정")]
    [SerializeField, Min(0.0f)] private float rotateSpeed = 720f;
    [Header("애니메이터")]
    [SerializeField] private Animator animator;
    [SerializeField] private string moveParameter = "Move";
    [SerializeField] private string attackParameter = "Attack";
    [SerializeField] private string deadParameter = "Dead";
    [Header("사망 애니메이션 실행딜레이")]
    //추후에 에셋을 추가하고 연결한 이후에 딜레이를 조절할 예정
    //지금은 죽으면 바로 파괴
    [SerializeField, Min(0.0f)] private float destroyDelay = 0.0f;

    private UnitHealth health;
    private UnitMovement movement;
    private UnitAttack attack;
    private UnitStateMachine stateMachine;
    //실제 전투에서 사용할 런타임 능력치
    //지금은 UnitDataSO로 초기화하고 있지만, 추후에 EquipmentController나 레벨 시스템이 갱신
    private int maxHp;
    private int attackPower;
    private int defense;

    public UnitDataSO UnitData => unitData;
    public UnitTeam Team => team;
    public int MaxHp => maxHp;
    public int AttackPower => attackPower;
    public int Defense => defense;

    public BattleUnit Target { get; private set; }
    
    public bool IsDead
    {
        get
        {
            return health == null || health.IsDead;
        }
    }
    public bool CanBattle
    {
        get
        {
            return !IsDead && BattleManager.Instance != null && BattleManager.Instance.IsBattleRunning;
        }
    }
    public UnitState CurrentState
    {
        get
        {
            if (stateMachine == null) return UnitState.Idle;
            return stateMachine.CurrentState;
        }
    }


    private void Awake()
    {
        health = GetComponent<UnitHealth>();
        movement = GetComponent<UnitMovement>();
        attack = GetComponent<UnitAttack>();

        //animator를 직접 연결하지 않은 경우를 고려해서 작성하였음
        //추후에 변경할 가능성 있음. 현재 단계에선 크게 고려하지 않았음.
        //(이전 작업 때 에셋에 따라 사용방식이 달라지는 경우가 있었음.)
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }
    void Start()
    {
        if (unitData == null)
        {
            enabled = false;
            return;
        }
        if (BattleManager.Instance == null)
        {
            enabled = false;
            return;
        }
        //장비와 레벨쪽이 연결되기 전이므로
        //SO의 기본 능력치를 사용하는 것으로 함.(이것도 나중에 변경 예정)
        maxHp = Mathf.Max(1, unitData.MaxHp);
        attackPower = Mathf.Max(0, unitData.Attack);
        defense = Mathf.Max(0, unitData.Defense);

        health.Initialize(maxHp);
        movement.Initialize(unitData.MoveSpeed, unitData.AttackRange);
        attack.Initialize(this, unitData.AttackType, attackPower, unitData.AttackSpeed);

        //체력이 0이 되면 상태 변경 및 승패 판정(이것도 나중에 HandleDead를 수정할 예정)(일단 전투 프로세스 구현에 초점을 맞춤)
        health.OnDead += HandleDead;
        //필요한 컴포넌트 초기화 후 상태 머신 시작
        stateMachine = new UnitStateMachine(this);
        stateMachine.Start();
        //영웅 또는 적 목록 등록
        BattleManager.Instance.RegisterUnit(this);
    }
    void Update()
    {
        if (stateMachine == null) return;
        //현재 상태에 맞는 전투 행동 실행
        stateMachine.Update();
        UpdateMoveAnimation();
    }
    //파괴된 오브젝트 이벤트 구독 해제 및 목록 제거
    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDead -= HandleDead;
        }
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.UnregisterUnit(this);
        }
    }

    public void ApplyStats(int newMaxHp, int newAttackPower, int newDefense, bool addChangedHp = true)
    {
        //EquipmentController나 레벨 시스템 쪽에서 모든 계산이 끝난 최종 능력치를 전달 받음.
        maxHp = Mathf.Max(1, newMaxHp);
        attackPower = Mathf.Max(0, newAttackPower);
        defense = Mathf.Max(0, newDefense);
        //
        if (!health.IsInitialized) return;

        health.SetMaxHp(maxHp, addChangedHp);
        attack.SetAttackPower(attackPower);
    }
    public void FindTarget()
    {
        if (BattleManager.Instance == null)
        {
            ClearTarget();
            return;
        }
        //가장 가까운 대상 찾기
        Target = BattleManager.Instance.GetClosestTarget(this);
    }
    //유효 타겟 지정
    public bool HasValidTarget()
    {
        return Target != null && Target.gameObject.activeInHierarchy && !Target.IsDead && Target.Team != team;
    }
    public void ClearTarget()
    {
        Target = null;
    }
    public bool IsTargetInAttackRange(float extraRange = 0.0f)
    {
        return IsTargetInAttackRange(Target, extraRange);
    }
    public bool IsTargetInAttackRange(BattleUnit target, float extraRange = 0.0f)
    {
        if (target == null || unitData == null) return false;

        Vector3 dir = target.transform.position - transform.position;
        dir.y = 0.0f;//수평 거리만 확인하기 위해서 y는 0
        float range = unitData.AttackRange + Mathf.Max(0.0f, extraRange);
        //제곱 거리 비교로 계산. (불필요한 제곱근 계산 X)
        return dir.sqrMagnitude <= range * range;
    }
    public void MoveToTarget()
    {
        if (!HasValidTarget()) return;
        //근딜이나 원딜 둘 다 같은 방식으로 타겟에게 이동
        //SO에서 설정한 사거리에 따라 멈추는 거리가 달라짐
        movement.MoveTo(Target.transform.position);
    }
    public void StopMove()
    {
        movement.Stop();
    }
    public void FaceTarget()
    {
        if (!HasValidTarget()) return;

        movement.FaceTarget(Target.transform, rotateSpeed);
    }
    public void TryAttack()
    {
        if (!HasValidTarget()) return;

        attack.TryAttack(Target);
    }
    public void CancelAttack()
    {
        attack.CancelAttack();
    }
    public int TakeDamage(int damage)
    {
        //실제로 받은 데미지 반환(감소한 체력량)
        return health.TakeDamage(damage);
    }
    public int Heal(int amount)
    {
        //실제로 적용된 회복량 반환
        return health.Heal(amount);
    }
    public void PlayAttackAnimation()
    {
        if (animator == null) return;

        animator.SetTrigger(attackParameter);
    }
    private void HandleDead()
    {
        //사망 상태로 변경하면서 이동, 공격을 정리
        stateMachine?.ChangeState(UnitState.Dead);
        
        if (animator != null)
        {
            animator.SetBool(moveParameter, false);
            animator.SetTrigger(deadParameter);
        }
        //오브젝트 삭제 전에 판정
        BattleManager.Instance?.NotifyUnitDead(this);

        Debug.Log($"{name} 사망");
        //현재 단계에서는 0초로 두어 즉시 파괴되게 해두었음.
        //추후에 에셋이 추가되고 나면 딜레이를 주어서 사망 모션이 재생되게끔 할 예정.
        Destroy(gameObject, destroyDelay); 
    }
    private void UpdateMoveAnimation()
    {
        if (animator == null) return;

        bool isMoving = CurrentState == UnitState.Move && movement.IsMoving && CanBattle;
        animator.SetBool(moveParameter, isMoving);
    }
}
