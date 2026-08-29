using UnityEngine;

[RequireComponent(typeof(UnitHealth))]
[RequireComponent(typeof(UnitMovement))]
[RequireComponent(typeof(UnitAttack))]
public class BattleUnit : MonoBehaviour
{
    [Header("유닛 설정")]
    [SerializeField] private UnitDataSO unitData;
    [SerializeField] private UnitTeam team;
    [Header("레벨 설정")]
    [SerializeField, Min(1)] private int level = 1;
    [Header("회전 설정")]
    [SerializeField, Min(0.0f)] private float rotateSpeed = 720f;
    [Header("사망 애니메이션 실행딜레이")]
    //추후에 에셋을 추가하고 연결한 이후에 딜레이를 조절할 예정
    //지금은 죽으면 바로 파괴
    [SerializeField, Min(0.0f)] private float destroyDelay = 2.0f;

    private UnitHealth health;
    private UnitMovement movement;
    private UnitAttack attack;
    private UnitSkill skill;
    private UnitStateMachine stateMachine;

    private UnitAnimator unitAnimator;

    private UnitBarrier barrier;
    
    //실제 전투에서 사용할 런타임 능력치
    //지금은 UnitDataSO로 초기화하고 있지만, 추후에 EquipmentController나 레벨 시스템이 갱신
    private int maxHp;
    private int attackPower;
    private int defense;
    //버프 등.. 전투 중 임시로 적용되는 공격력 보정 값
    private int attackPowerModifier;

    private bool isInitialized;

    private bool usesExternalDecision; //외부(BT)에서 행동 결정을 받는지 체크하는 용

    public UnitDataSO UnitData => unitData;
    public UnitTeam Team => team;
    public int Level => level;
    public int MaxHp => maxHp;
    public int AttackPower => Mathf.Max(0, attackPower + attackPowerModifier);
    public int Defense => defense;

    public bool UsesExternalDecision => usesExternalDecision;

    public BattleUnit Target { get; private set; }
    public int CurrentHp
    {
        get
        {
            return health != null ? health.CurrentHp : 0;
        }
    }
    
    public bool IsAttacking
    {
        get
        {
            return attack != null && attack.IsAttacking;
        }
    }
    public bool IsUsingSkill
    {
        get
        {
            return skill != null && skill.IsUsingSkill;
        }
    }
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
            return isInitialized && !IsDead && BattleManager.Instance != null && BattleManager.Instance.IsBattleRunning;
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
        //스킬이 없는 유닛이 있을 수도 있음. (잡몹)
        skill = GetComponent<UnitSkill>();

        //애니메이션 처리는 UnitAnimator에서 하는걸로
        unitAnimator = GetComponent<UnitAnimator>();

        barrier = GetComponent<UnitBarrier>();
    }
    void Start()
    {
        if (isInitialized) return;

        Initialize(level);
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

        if (team == UnitTeam.Hero && HeroManager.Instance != null && HeroManager.Instance.IsInitialized)
        {
            HeroManager.Instance.Controller.OnHeroStatChanged -= HandleHeroStatChanged;
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.UnregisterUnit(this);
        }
    }


    public void Initialize(int unitLevel)
    {
        if (isInitialized) return;
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

        level = Mathf.Max(1, unitLevel);

        // 영웅은 보유 데이터 기준 최종 능력치 적용(0812 추가)
        if (!TryApplyOwnedHeroStats())
        {
            CalculateBaseLevelStats();
        }

        health.Initialize(maxHp);
        movement.Initialize(unitData.MoveSpeed, unitData.AttackRange);
        attack.Initialize(this, unitData.AttackType, AttackPower, unitData.AttackSpeed);
        
        unitAnimator?.SetAttackSpeed(unitData.AttackSpeed);//공격속도에 맞춰서 애니메이션 속도도 설정

        skill?.Initialize(this);

        health.OnDead += HandleDead;
        stateMachine = new UnitStateMachine(this);
        stateMachine.Start();
        BattleManager.Instance.RegisterUnit(this);
        isInitialized = true;

        // 영웅 능력치 변경 이벤트 구독
        SubscribeHeroStatChanged();
    }
    // 영웅 최종 능력치 변경 이벤트 구독 
    private void SubscribeHeroStatChanged()
    {
        if (team != UnitTeam.Hero)
        {
            return;
        }

        if (unitData is not HeroData)
        {
            return;
        }

        if (HeroManager.Instance == null || !HeroManager.Instance.IsInitialized)
        {
            return;
        }

        HeroManager.Instance.Controller.OnHeroStatChanged += HandleHeroStatChanged;
    }

    // 영웅 최종 능력치 변경 시 전투 능력치 갱신
    private void HandleHeroStatChanged()
    {
        if (unitData is not HeroData heroData)
        {
            return;
        }

        if (HeroManager.Instance == null || !HeroManager.Instance.IsInitialized)
        {
            return;
        }

        if (!HeroManager.Instance.Controller.TryGetHeroStat(heroData.UnitID, out HeroStat heroStat))
        {
            return;
        }

        ApplyStats(heroStat.MaxHp, heroStat.Attack, heroStat.Defense);
    }


    // 보유 영웅 데이터 기준으로 최종 능력치 적용 (0812 추가)
    private bool TryApplyOwnedHeroStats()
    {
        if (team != UnitTeam.Hero)
        {
            return false;
        }

        if (unitData is not HeroData heroData)
        {
            return false;
        }

        if (HeroManager.Instance == null || !HeroManager.Instance.IsInitialized)
        {
            return false;
        }

        if (!HeroManager.Instance.Controller.TryGetHero(heroData.UnitID, out OwnedHeroData ownedHero))
        {
            return false;
        }

        if (!HeroManager.Instance.Controller.TryGetHeroStat(heroData.UnitID, out HeroStat heroStat))
        {
            return false;
        }

        level = ownedHero.Level;
        maxHp = heroStat.MaxHp;
        attackPower = heroStat.Attack;
        defense = heroStat.Defense;

        return true;
    }

    public void ApplyStats(int newMaxHp, int newAttackPower, int newDefense, bool addChangedHp = true)
    {
        //EquipmentController나 레벨 시스템 쪽에서 모든 계산이 끝난 최종 능력치를 전달 받음.
        maxHp = Mathf.Max(1, newMaxHp);
        attackPower = Mathf.Max(0, newAttackPower);
        defense = Mathf.Max(0, newDefense);
        //
        if (!isInitialized || !health.IsInitialized) return;

        health.SetMaxHp(maxHp, addChangedHp);
        attack.SetAttackPower(AttackPower);
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

        Vector3 direction = target.transform.position - transform.position;
        //높이 차이는 제외하고 수평 거리만 비교
        direction.y = 0.0f;
        float attackRange = unitData.AttackRange + Mathf.Max(0.0f, extraRange);
        //제곱 거리 비교로 계산. (불필요한 제곱근 계산 X)
        return direction.sqrMagnitude <= attackRange * attackRange;
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
    //기본 공격 이벤트 연결
    public void AttackHitEvent()
    {
        if (!IsAttacking) return;
        
        attack.ApplyAttackDamage();
    }
    
    //스킬 이벤트 연결
    public void SkillActivateEvent()
    {
        if (skill == null || !IsUsingSkill) return;
        //실제 스킬 애니메이션 중에 발생한 이벤트만 가능
        skill.ApplySkillEffect();
    }
    public void SkillEndEvent()
    {
        if (skill == null || !IsUsingSkill) return;

        skill.CompleteSkill();
    }
    //스킬~
    public bool CanUseSkill()
    {
        return skill != null && skill.CanUseSkill();
    }
    //보스 BT에서 특정 스킬의 사용 가능 여부 확인
    public bool CanUseSkill(SkillDataSO data)
    {
        return skill != null && skill.CanUseSkill(data);
    }
    public bool UseSkill()
    {
        return skill != null && skill.UseSkill();
    }
    //보스 BT에서 다음에 사용할 스킬 지정
    public void SelectSkill(SkillDataSO data)
    {
        skill?.SelectSkill(data);
    }
    public void SetExternalSkillAnimation(bool useExternal)
    {
        skill?.SetExternalSkillAnimation(useExternal);
    }
    //보스 BT가 선택해둔 스킬이 있는지 확인
    public bool HasSelectedSkill
    {
        get
        {
            return skill != null && skill.HasSelectedSkill;
        }
    }
    //현재 실제로 실행 중인 스킬 데이터
    public SkillDataSO ActiveSkillData
    {
        get
        {
            return skill != null ? skill.ActiveSkillData : null;
        }
    }
    public void UpdateSkill()
    {
        skill?.UpdateSkill();
    }
    public void CancelSkill()
    {
        skill?.CancelSkill();
    }

    public void ChangeState(UnitState newState)
    {
        stateMachine?.ChangeState(newState);
    }

    public int TakeDamage(int damage)
    {
        int appliedDamage = health.TakeDamage(damage);
        //피격 애니메이션 추가
        if (appliedDamage <= 0 || IsDead) return appliedDamage;
        //공격/스킬 진행 중이거나 해당 State에 이미 진입한 경우, 피격 애니메이션이 끼어들지 않도록 함
        bool isActionState = CurrentState == UnitState.Attack || CurrentState == UnitState.Skill;
        if (!IsAttacking && !IsUsingSkill && !isActionState) unitAnimator?.PlayDamaged();

        //실제로 받은 데미지 반환(감소한 체력량)
        return appliedDamage;
    }
    public int TakeDamage(int damage, BattleUnit attacker)
    {
        int appliedDamage = TakeDamage(damage);
        if (appliedDamage > 0 && attacker != null) DpsManager.Instance?.AddDamage(attacker, appliedDamage);
        return appliedDamage;
    }
    public int Heal(int amount)
    {
        //실제로 적용된 회복량 반환
        return health.Heal(amount);
    }
    public bool ActivateBarrier(int blockCount, GameObject vfxPrefab)
    {
        if (barrier == null) return false;

        barrier.Activate(blockCount, vfxPrefab);
        return true;
    }
    public bool TryPlayAttackAnimation()
    {
        if (unitAnimator == null) return false;

        return unitAnimator.TryPlayAttack();
    }
    //BT나 UnitAttack이 UnitAnimator를 직접 찾지 않게 하기 위해서 만든 전달용 함수
    public bool IsAttackAnimationActive()
    {
        return unitAnimator != null && unitAnimator.IsAttackAnimationActive();
    }
    public bool TryPlaySkillAnimation()
    {
        if (unitAnimator == null) return false;

        return unitAnimator.TryPlaySkill();
    }
    private void HandleDead()
    {
        //사망 애니메이션으로 넘어가기 전에 진행 중인 공격/스킬부터 정리
        attack?.CancelAttack();
        skill?.CancelSkill();

        //사망 상태로 변경하면서 이동, 공격을 정리
        stateMachine?.ChangeState(UnitState.Dead);

        unitAnimator?.PlayDead();

        //오브젝트 삭제 전에 판정
        BattleManager.Instance?.NotifyUnitDead(this);

        Debug.Log($"{name} 사망");
        //현재 단계에서는 0초로 두어 즉시 파괴되게 해두었음.
        //추후에 에셋이 추가되고 나면 딜레이를 주어서 사망 모션이 재생되게끔 할 예정.
        Destroy(gameObject, destroyDelay); 
    }
    private void UpdateMoveAnimation()
    {
        bool isMoving = CurrentState == UnitState.Move && movement.IsMoving && CanBattle;

        unitAnimator?.SetMove(isMoving);
    }

    //UnitDataSO + 레벨 성장치만 계산하는 함수
    private void CalculateBaseLevelStats()
    {
        level = Mathf.Max(1, level);
        int levelIncrease = level - 1;
        
        maxHp = Mathf.Max(1, unitData.MaxHp + unitData.HpPerLevel * levelIncrease);
        attackPower = Mathf.Max(0, unitData.Attack + unitData.AttackPerLevel *  levelIncrease);
        defense = Mathf.Max(0, unitData.Defense +  unitData.DefensePerLevel * levelIncrease);
    }

    public void SetLevel(int newLevel)
    {
        level = Mathf.Max(1, newLevel);

        //아직 전투 초기화 전이라면, 이후에 Initialize에서 레벨능력치 계산
        if (!isInitialized) return;

        ApplyBaseLevelStats();
    }
    private void ApplyBaseLevelStats()
    {
        //기본 능력치와 레벨 성장값만 계산하게 해두었습니다.
        //추후에 장비 시스템 연결 후에는 최종 능력치 계산을 통해 재계산해야할 것 같습니다.
        CalculateBaseLevelStats();

        health.SetMaxHp(maxHp, true);
        attack.SetAttackPower(AttackPower);
    }

    public void SetAttackModifier(int value)
    {
        attackPowerModifier = value;
        //UnitAttack이 실제로 사용하는 공격력도 갱신
        attack?.SetAttackPower(AttackPower);
    }
    public void ClearAttackModifier()
    {
        attackPowerModifier = 0;
        //원래대로 복구
        attack?.SetAttackPower(AttackPower);
    }

    public void SetExternalDecision(bool useExternalDecision)
    {
        usesExternalDecision = useExternalDecision;
    }
}
