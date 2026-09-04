using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BattleUnit))]
[RequireComponent(typeof(FieldBossAnimator))]
public class FieldBossBehaviorTree : MonoBehaviour
{
    [Header("보스 스킬")]
    [SerializeField] private SkillDataSO projectileSkill;
    [SerializeField] private SkillDataSO laserSkill;

    [Header("페이즈")]
    [SerializeField, Range(0.0f, 1.0f)] private float laserHpRatio = 0.7f;
    [SerializeField, Range(0.0f, 1.0f)] private float rageHpRatio = 0.3f;

    [Header("3페이즈 공격력 증가")]
    [SerializeField, Min(1)] private int rageAttackBuff = 30;
    [SerializeField] private GameObject rageVfxPrefab;
    [SerializeField] private Transform rageVfxPoint;

    [Header("광폭화 안전 종료")]
    [SerializeField, Min(0.5f)] private float rageSafetyDuration = 3.0f;

    [Header("광폭화 SFX")]
    [SerializeField] private AudioClip rageSfx;
    [SerializeField, Range(0.0f, 1.0f)] private float rageSfxVolume = 0.6f;

    private BattleUnit unit;
    private FieldBossAnimator bossAnimator;

    private BTNode root;

    private Coroutine subscribeRoutine;

    private float nextProjectileTime;
    private float nextLaserTime;

    //실제 스킬이 시작됐을 때 쿨타임을 돌리기 위한 용도
    private SkillDataSO pendingCooldownSkill;

    private bool rageActivated;
    private bool rageBuffApplied;
    private bool isRaging;

    private float rageSafetyEndTime;

    private GameObject rageVfx;

    private void Awake()
    {
        unit = GetComponent<BattleUnit>();
        bossAnimator = GetComponent<FieldBossAnimator>();
        //보스의 행동 판단은 BT가 담당
        unit.SetExternalDecision(true);
        CreateTree();
    }
    private void Start()
    {
        unit.SetExternalSkillAnimation(true);
    }
    private void OnEnable()
    {
        subscribeRoutine = StartCoroutine(SubscribeRoutine());
    }
    private void OnDisable()
    {
        if (subscribeRoutine != null)
        {
            StopCoroutine(subscribeRoutine);
            subscribeRoutine = null;
        }
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
        }
        ResetRuntime();
    }
    void Update()
    {
        if (unit == null || !unit.CanBattle) return;
        //선택한 스킬이 실제로 시작됐으면 해당 스킬 쿨타임 시작
        UpdateSkillCooldown();
        UpdateRageSafety();
        root?.Run();
    }

    private IEnumerator SubscribeRoutine()
    {
        while (BattleManager.Instance == null) yield return null;
        //중복 구독 방지
        BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
        BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
        subscribeRoutine = null;
        //추후 풀에서 전투 중 다시 활성화되는 경우 고려
        if (BattleManager.Instance.IsBattleRunning) HandleBattleStarted();
    }
    private void HandleBattleStarted()
    {
        ResetRuntime();
        //각 스킬은 자기 쿨타임을 독립적으로 가짐
        nextProjectileTime = projectileSkill != null ? Time.time + projectileSkill.Cooldown : 0.0f;
        nextLaserTime = laserSkill != null ? Time.time + laserSkill.Cooldown : 0.0f;
    }
    private void CreateTree()
    {
        root = new BTSelector(
            //광폭화 시전 중에는 다른 행동 금지
            new BTSequence(new BTCondition(() => isRaging), ChangeStateNode(UnitState.Idle)),
            //진행 중인 스킬 유지
            new BTSequence(new BTCondition(() => unit.IsUsingSkill), ChangeStateNode(UnitState.Skill)),
            //진행 중인 기본 공격 유지
            new BTSequence(new BTCondition(() => unit.IsAttacking), ChangeStateNode(UnitState.Attack)),
            //선택된 스킬 시작 대기
            new BTSequence(new BTCondition(() => unit.HasSelectedSkill), ChangeStateNode(UnitState.Skill)),
            //타겟 탐색
            new BTSequence(new BTCondition(() => !unit.HasValidTarget()), new BTAction(FindTarget)),
            //HP 30% 미만이면 광폭화 1회
            new BTSequence(new BTCondition(CanActivateRage), new BTAction(StartRage)),
            //HP 70% 미만부터 레이저
            new BTSequence(new BTCondition(CanUseLaser), new BTAction(StartLaser)),
            //기본 관통형 투사체 스킬
            new BTSequence(new BTCondition(CanUseProjectile), new BTAction(StartProjectile)),
            //그 외 원거리 기본 공격
            new BTSequence(
                new BTCondition(() => unit.HasValidTarget()), 
                new BTCondition(() => unit.IsTargetInAttackRange()),
                ChangeStateNode(UnitState.Attack)),
            //고정형 보스라 Move State는 선택하지 않음
            ChangeStateNode(UnitState.Idle)
            );
    }

    private bool CanUseProjectile()
    {
        if (projectileSkill == null) return false;
        if (Time.time < nextProjectileTime) return false;

        //해당 관통 스킬 자체의 사용 가능 여부 검사
        return unit.CanUseSkill(projectileSkill);
    }
    private BTStatus StartProjectile()
    {
        if (!bossAnimator.PlayProjectileSkill()) return BTStatus.Running;

        unit.SelectSkill(projectileSkill);
        //아직 쿨타임을 시작하지 않고, 실제로 스킬이 시작된 뒤 UpdateSkillCooldown에서 시작
        pendingCooldownSkill = projectileSkill;
        unit.ChangeState(UnitState.Skill);

        return BTStatus.Success;
    }

    private bool CanUseLaser()
    {
        if (laserSkill == null) return false;
        //70% 이상이면 아직 레이저 사용 X
        if (GetHpRatio() >= laserHpRatio) return false;
        if (Time.time < nextLaserTime) return false;
        //해당 레이저 스킬 자체의 사용 가능 여부 검사
        return unit.CanUseSkill(laserSkill);
    }
    private BTStatus StartLaser()
    {
        if (!bossAnimator.PlayLaserSkill()) return BTStatus.Running;

        unit.SelectSkill(laserSkill);
        pendingCooldownSkill = laserSkill;
        unit.ChangeState(UnitState.Skill);

        return BTStatus.Success;
    }

    private void UpdateSkillCooldown()
    {
        if (pendingCooldownSkill == null) return;
        //애니메이션까지 정상적으로 시작된 뒤에만 쿨타임 시작
        if (!unit.IsUsingSkill) return;
        if (unit.ActiveSkillData != pendingCooldownSkill) return;
        //실제로 스킬 시작 성공 후 쿨타임 시작
        if (pendingCooldownSkill == projectileSkill)
        {
            nextProjectileTime = Time.time + projectileSkill.Cooldown;
        }
        else if (pendingCooldownSkill == laserSkill)
        {
            nextLaserTime = Time.time + laserSkill.Cooldown;
        }

        pendingCooldownSkill = null;
    }
    private bool CanActivateRage()
    {
        if (rageActivated) return false;
        if (isRaging) return false;

        return GetHpRatio() < rageHpRatio;
    }
    private BTStatus StartRage()
    {
        if (!bossAnimator.PlayRage()) return BTStatus.Running;

        rageActivated = true;
        isRaging = true;

        rageSafetyEndTime = Time.time + rageSafetyDuration;
        
        unit.StopMove();
        unit.ChangeState(UnitState.Idle);

        return BTStatus.Success;
    }
    //Rage 애니메이션 이벤트에서 호출
    public void RageActivateEvent()
    {
        if (!isRaging) return;
        if (rageBuffApplied) return;

        rageBuffApplied = true;
        BattleSoundController.Instance?.PlayCombatSfx(rageSfx, rageSfxVolume);
        unit.SetAttackModifier(rageAttackBuff);
        if (rageVfxPrefab != null)
        {
            Transform point = rageVfxPoint != null ? rageVfxPoint : transform;
            rageVfx = Instantiate(rageVfxPrefab, point.position, point.rotation, point);
        }
        //확인용
        Debug.Log($"[보스 3페이즈] {unit.name} / 공격력 +{rageAttackBuff}");
    }
    //Rage 애니메이션 마지막 이벤트
    public void RageEndEvent()
    {
        if (!isRaging) return;

        isRaging = false;
        rageSafetyEndTime = 0.0f;
    }
    private void UpdateRageSafety()
    {
        if (!isRaging) return;
        if (Time.time < rageSafetyEndTime) return;
        //RageEnd 이벤트 누락 대비용
        isRaging = false;
        rageSafetyEndTime = 0.0f;

        Debug.LogWarning($"{name} : RageEnd Event가 호출되지 않아 광폭화 상태를 복구함", this);
    }
    private float GetHpRatio()
    {
        if (unit == null || unit.MaxHp <= 0) return 0.0f;
        return (float)unit.CurrentHp / unit.MaxHp;
    }
    private void ResetRuntime()
    {
        nextProjectileTime = 0.0f;
        nextLaserTime = 0.0f;

        pendingCooldownSkill = null;

        rageActivated = false;
        rageBuffApplied = false;
        isRaging = false;

        rageSafetyEndTime = 0.0f;
        //재사용 시 이전 광폭화 공격력 제거
        if (unit != null) unit.ClearAttackModifier();
        //재사용 시 이전 광폭화 VFX 제거
        if (rageVfx != null)
        {
            Destroy(rageVfx);
            rageVfx = null;
        }
    }
    private BTNode ChangeStateNode(UnitState state)
    {
        return new BTAction(
            () =>
            {
                unit.ChangeState(state);
                return BTStatus.Success;
            });
    }
    private BTStatus FindTarget()
    {
        unit.FindTarget();
        return unit.HasValidTarget() ? BTStatus.Success : BTStatus.Failure;
    }
}
