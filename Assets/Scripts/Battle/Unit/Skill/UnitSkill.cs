using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class UnitSkill : MonoBehaviour
{
    [Header("스킬 설정")]
    [SerializeField] private SkillDataSO skillData;

    [Header("투사체 스킬 발사 위치")]
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("시전자 VFX 위치")]
    [SerializeField] private Transform skillVfxPoint;

    #region 런타임
    private BattleUnit unit;
    private BattleUnit skillTarget; //스킬 시작 당시의 대상을 보관하는 용

    private SkillDataSO selectedSkillData; //BT가 사용하라고 지정한 데이터
    private SkillDataSO activeSkillData;   //지금 실제 애니메이션과 함께 실행 중인 데이터
    
    private bool isBattleEventSubscribed;
    private bool useExternalSkillAnimation;

    private float nextSkillAvailableTime;
    private float skillSafetyEndTime;

    private bool isUsingSkill; //스킬 사용 중인지 확인
    private bool hasAppliedSkillEffect; //한 번의 스킬에서 효과가 중복 적용되는 것 방지용

    private bool hasBattleStarted;
    //풀링
    private ObjectPool<SkillProjectile> projectilePool;
    private SkillProjectile projectilePoolPrefab; //현재 만들어진 풀이 어떤 관통 투사체 프리팹용인지 기억하는 용

    private ObjectPool<LaserSkillDamage> laserPool;
    private LaserSkillDamage laserPoolPrefab;

    private ObjectPool<WhirlwindSkillDamage> whirlwindPool;
    private WhirlwindSkillDamage whirlwindPoolPrefab;
    
    private ObjectPool<AreaSkillDamage> areaSkillPool;
    private AreaSkillDamage areaSkillPoolPrefab;

    private ObjectPool<GameObject> buffCastVfxPool;
    private ObjectPool<GameObject> healCastVfxPool;
    private ObjectPool<GameObject> healTargetVfxPool;
    #endregion

    #region 프로퍼티
    public bool IsUsingSkill => isUsingSkill;
    public bool HasSkill => skillData != null;

    public bool HasSelectedSkill => selectedSkillData != null;
    public SkillDataSO ActiveSkillData => activeSkillData;

    public float CooldownRatio
    {
        get
        {
            if (!hasBattleStarted || skillData == null) return 0.0f;
            if (skillData.Cooldown <= 0.0f) return 1.0f;

            float remainingTime = Mathf.Max(0.0f, nextSkillAvailableTime - Time.time);
            return Mathf.Clamp01(1.0f - remainingTime / skillData.Cooldown);
        }
    }
    #endregion


    #region 유니티 라이프사이클
    public void Initialize(BattleUnit unit)
    {
        this.unit = unit;

        selectedSkillData = null;
        activeSkillData = null;
        skillTarget = null;

        isUsingSkill = false;
        hasAppliedSkillEffect = false;

        skillSafetyEndTime = 0.0f;

        hasBattleStarted = false;
        nextSkillAvailableTime = 0.0f;

        SubscribeBattleEvent();
    }

    private void OnEnable()
    {
        SubscribeBattleEvent();
        if (unit != null && BattleManager.Instance != null && BattleManager.Instance.IsBattleRunning) HandleBattleStarted();
    }
    private void OnDisable()
    {
        UnsubscribeBattleEvent();

        selectedSkillData = null;
        activeSkillData = null;

        isUsingSkill = false;
        hasAppliedSkillEffect = false;

        skillTarget = null;
        skillSafetyEndTime = 0.0f;

        hasBattleStarted = false;
    }
    private void OnDestroy()
    {
        UnsubscribeBattleEvent();

        if (projectilePool != null)
        {
            projectilePool.Clear();
            projectilePool = null;
            projectilePoolPrefab = null;
        }
        if (whirlwindPool != null)
        {
            whirlwindPool.Clear();
            whirlwindPool = null;
            whirlwindPoolPrefab = null;
        }
        if (laserPool != null)
        {
            laserPool.Clear();
            laserPool = null;
            laserPoolPrefab = null;
        }
        if (areaSkillPool != null)
        {
            areaSkillPool.Clear();
            areaSkillPool = null;
            areaSkillPoolPrefab = null;
        }
        if (buffCastVfxPool != null)
        {
            buffCastVfxPool.Clear();
            buffCastVfxPool = null;
        }
        if (healCastVfxPool != null)
        {
            healCastVfxPool.Clear();
            healCastVfxPool = null;
        }
        if (healTargetVfxPool != null)
        {
            healTargetVfxPool.Clear();
            healTargetVfxPool = null;
        }
    }
    #endregion

    #region 스킬 컨트롤. 전투시작/스킬 선택/사용 가능 여부 등..
    //전투 시작
    private void HandleBattleStarted()
    {
        hasBattleStarted = true;

        if (skillData == null)
        {
            nextSkillAvailableTime = 0.0f;
            return;
        }
        //전투가 실제로 시작된 순간부터 첫 스킬 쿨타임 시작
        nextSkillAvailableTime = Time.time + skillData.Cooldown;
    }
    
    //보스 BT용 스킬 선택 용
    public void SelectSkill(SkillDataSO data)
    {
        if (data == null) return;
        if (isUsingSkill) return;

        selectedSkillData = data;
    }
    public void SetExternalSkillAnimation(bool useExternal)
    {
        useExternalSkillAnimation = useExternal;
    }

    public bool CanUseSkill()
    {
        SkillDataSO data = selectedSkillData != null ? selectedSkillData : skillData;
        if (!hasBattleStarted) return false;
        if (unit == null || unit.IsDead || data == null || isUsingSkill) return false;
        //BT에서 선택된 스킬은 쿨타임을 BT에서 각각 관리
        if (selectedSkillData == null && Time.time < nextSkillAvailableTime) return false;

        BattleUnit target = GetSkillTarget(data);
        return IsValidTarget(target, data);
    }
    public bool CanUseSkill(SkillDataSO data)
    {
        if (!hasBattleStarted) return false;
        if (unit == null || unit.IsDead || data == null || isUsingSkill) return false;

        BattleUnit target = GetSkillTarget(data);
        return IsValidTarget(target, data);
    }
    public bool UseSkill()
    {
        SkillDataSO data = selectedSkillData != null ? selectedSkillData : skillData;
        if (!CanUseSkill()) return false;

        BattleUnit target = GetSkillTarget(data);
        if (!IsValidTarget(target, data)) return false;
        //애니메이션을 시작할 수 있을 때만 스킬 사용 상태로 진입
        if (!useExternalSkillAnimation && !unit.TryPlaySkillAnimation()) return false;

        bool useSelectedSkill = selectedSkillData != null;

        activeSkillData = data;
        selectedSkillData = null;

        skillTarget = target;

        isUsingSkill = true;
        hasAppliedSkillEffect = false;

        //일반 유닛의 기본 스킬만 기존 UnitSkill 쿨타임 사용
        if (!useSelectedSkill) nextSkillAvailableTime = Time.time + data.Cooldown;
        
        //SkillEnd 애니메이션 이벤트 누락 대비
        skillSafetyEndTime = Time.time + data.SkillSafetyDuration;

        unit.StopMove();
        return true;
    }
    //SkillEnd 이벤트가 누락됐을 때 스킬 상태 복구
    public void UpdateSkill()
    {
        if (!isUsingSkill) return;

        if (Time.time < skillSafetyEndTime) return;

        //확인용
        Debug.LogWarning($"{name} : SkillEnd Event 가 호출되지 않아서 스킬 상태를 복구함.", this);

        CompleteSkill();
    }
    //SkillEnd 애니메이션 이벤트에서 호출
    public void CompleteSkill()
    {
        if (!isUsingSkill) return;

        isUsingSkill = false;
        hasAppliedSkillEffect = false;

        selectedSkillData = null;
        activeSkillData = null;
        skillTarget = null;

        skillSafetyEndTime = 0.0f;
    }
    //상태 변경 또는 사망 등으로 진행 중인 스킬 취소
    public void CancelSkill()
    {
        isUsingSkill = false;
        hasAppliedSkillEffect = false;

        selectedSkillData = null;
        activeSkillData = null;
        skillTarget = null;

        skillSafetyEndTime = 0.0f;
    }
    //전투 재시작 또는 오브젝트 재사용 시 스킬 상태 초기화
    public void ResetSkill()
    {
        isUsingSkill = false;
        hasAppliedSkillEffect = false;

        selectedSkillData = null;
        activeSkillData = null;
        skillTarget = null;

        skillSafetyEndTime = 0.0f;

        nextSkillAvailableTime = skillData != null ? Time.time + skillData.Cooldown : 0.0f;
    }
    #endregion

    #region 스킬 이펙트. 실제 효과 분기
    //SkillActivate 애니메이션 이벤트에서 호출
    public void ApplySkillEffect()
    {
        if (!isUsingSkill || hasAppliedSkillEffect || activeSkillData == null) return;

        //공격 대상이 필요한 스킬인데, 기존 타겟이 죽었으면 스킬 취소
        if (NeedsEnemyTarget(activeSkillData) && !IsValidTarget(skillTarget, activeSkillData))
        {
            CancelSkill();
            return;
        }

        SkillDataSO data = activeSkillData;
        switch (data.EffectType)
        {
            case SkillEffectType.Damage:
                if (!IsValidTarget(skillTarget, data)) return;
                hasAppliedSkillEffect = true;
                unit.FaceTarget();
                ApplyDamage(skillTarget, data);
                break;
            case SkillEffectType.Heal:
                if (!IsValidTarget(skillTarget, data)) return;
                hasAppliedSkillEffect = true;
                ApplyHeal(skillTarget, data);
                break;
            case SkillEffectType.Barrier:
                hasAppliedSkillEffect = true;
                ApplyBarrier(data);
                break;
            case SkillEffectType.ProjectileDamage:
                if (!IsValidTarget(skillTarget, data)) return;
                hasAppliedSkillEffect = true;
                unit.FaceTarget();
                FireProjectile(skillTarget, data);
                break;
            case SkillEffectType.Buff:
                hasAppliedSkillEffect = true;
                ApplyBuff(data);
                break;
            case SkillEffectType.AreaDamage:
                if (!IsValidTarget(skillTarget, data)) return;
                hasAppliedSkillEffect = true;
                unit.FaceTarget();
                ApplyAreaDamage(skillTarget, data);
                break;
                //휠윈드
            case SkillEffectType.Whirlwind:
                if (!IsValidTarget(skillTarget, data)) return;
                hasAppliedSkillEffect = true;
                unit.FaceTarget();
                StartWhirlwind(data);
                break;
            case SkillEffectType.Laser:
                if (!IsValidTarget(skillTarget, data)) return;
                hasAppliedSkillEffect = true;
                unit.FaceTarget();
                StartLaser(data);
                break;
        }
    }
    private void ApplyDamage(BattleUnit target, SkillDataSO data)
    {
        int skillAttack = Mathf.RoundToInt(unit.AttackPower * data.DamageRatio);
        int finalDamage = DamageCalculator.Calculate(skillAttack, target.Defense);

        int appliedDamage = target.TakeDamage(finalDamage, unit);
        //기능 확인 용
        Debug.Log($"{unit.name} 스킬 사용 / {target.name} 피해 : {appliedDamage}");
    }
    private void ApplyHeal(BattleUnit target, SkillDataSO data)
    {
        if (target == null || target.IsDead) return;

        //시전자 발밑 VFX
        PlayHealCastVfx(data);

        int healAmount = Mathf.RoundToInt(unit.AttackPower * data.DamageRatio);
        int appliedHeal = target.Heal(healAmount);

        //회복된 경우에만 대상 힐 VFX 표시
        if (appliedHeal > 0) PlayHealTargetVfx(target, data);
        //기능 확인 용
        Debug.Log($"[힐 스킬] {unit.name} -> {target.name} / 회복량 : {appliedHeal}");
    }
    private void ApplyBarrier(SkillDataSO data)
    {
        unit.ActivateBarrier(data.BlockCount, data.BarrierVfxPrefab);
    }
    private void ApplyBuff(SkillDataSO data)
    {
        if (BattleManager.Instance == null) return;
        //시전자 발밑 VFX
        PlayBuffCastVfx(data);

        var allies = BattleManager.Instance.GetAliveAllies(unit);
        for (int i = 0; i < allies.Count; i++)
        {
            BattleUnit ally = allies[i];
            if (ally == null || ally.IsDead) continue;

            UnitBuff buff = ally.GetComponent<UnitBuff>();
            if (buff == null) continue;
            buff.ApplyAttackBuff(data.AttackBuff, data.BuffDuration, data.BuffVfxPrefab, null);
        }
    }
    //광역 단타 스킬
    private void ApplyAreaDamage(BattleUnit target, SkillDataSO data)
    {
        if (target == null) return;
        if (data.AreaDamagePrefab == null) return;

        Vector3 targetPosition = target.transform.position;
        int skillAttack = Mathf.RoundToInt(unit.AttackPower * data.DamageRatio);

        AreaSkillDamage areaSkill;

        if (areaSkillPool != null && areaSkillPoolPrefab == data.AreaDamagePrefab)
        {
            areaSkill = areaSkillPool.Get();
        }
        else if (areaSkillPool == null)
        {
            CreateAreaSkillPool(data.AreaDamagePrefab);
            areaSkill = areaSkillPool != null ? areaSkillPool.Get() : null;
        }
        else
        {
            //다른 Area Prefab이 들어온 경우 기존 방식으로 안전하게 생성
            areaSkill = Instantiate(data.AreaDamagePrefab);
        }
        if (areaSkill == null) return;

        areaSkill.transform.SetParent(null);
        areaSkill.transform.SetPositionAndRotation(targetPosition, Quaternion.identity);
        areaSkill.gameObject.SetActive(true);
        areaSkill.SetAreaRadius(data.AreaRadius);
        areaSkill.ApplyDamage(unit, skillAttack);
        areaSkill.PlayEffect();
    }
    //풀
    private void CreateAreaSkillPool(AreaSkillDamage prefab)
    {
        if (areaSkillPool != null) return;
        if (prefab == null) return;

        areaSkillPoolPrefab = prefab;
        areaSkillPool = new ObjectPool<AreaSkillDamage>(
                CreateAreaSkill,
                OnGetAreaSkill,
                OnReleaseAreaSkill,
                OnDestroyAreaSkill,
                true,
                2,
                5
            );
    }
    private AreaSkillDamage CreateAreaSkill()
    {
        AreaSkillDamage areaSkill = Instantiate(areaSkillPoolPrefab);
        areaSkill.gameObject.SetActive(false);
        areaSkill.SetPool(areaSkillPool, this);
        return areaSkill;
    }
    private void OnGetAreaSkill(AreaSkillDamage areaSkill)
    {

    }
    private void OnReleaseAreaSkill(AreaSkillDamage areaSkill)
    {
        areaSkill.transform.SetParent(null);
        areaSkill.gameObject.SetActive(false);
    }
    private void OnDestroyAreaSkill(AreaSkillDamage areaSkill)
    {
        if (areaSkill != null) Destroy(areaSkill.gameObject);
    }

    //휠윈드
    private void StartWhirlwind(SkillDataSO data)
    {
        if (data.WhirlwindPrefab == null) return;

        WhirlwindSkillDamage prefab = data.WhirlwindPrefab.GetComponent<WhirlwindSkillDamage>();
        if (prefab == null) return;
        if (whirlwindPool == null) CreateWhirlwindPool(prefab);
        if (whirlwindPool == null) return;

        Transform point = skillVfxPoint != null ? skillVfxPoint : transform;
        WhirlwindSkillDamage whirlwind = whirlwindPool.Get();
        whirlwind.transform.SetParent(point);
        whirlwind.transform.SetPositionAndRotation(point.position, point.rotation);
        whirlwind.gameObject.SetActive(true);
        whirlwind.Initialize(unit, data.WhirlwindDuration, data.WhirlwindHitInterval, data.DamageRatio);
    }
    private void CreateWhirlwindPool(WhirlwindSkillDamage prefab)
    {
        if (whirlwindPool != null) return;
        if (prefab == null) return;

        whirlwindPoolPrefab = prefab;
        whirlwindPool = new ObjectPool<WhirlwindSkillDamage>(
            CreateWhirlwind,
            OnGetWhirlwind,
            OnReleaseWhirlwind,
            OnDestroyWhirlwind,
            true,
            1,
            3);
    }
    private WhirlwindSkillDamage CreateWhirlwind()
    {
        WhirlwindSkillDamage whirlwind = Instantiate(whirlwindPoolPrefab);
        whirlwind.gameObject.SetActive(false);
        whirlwind.SetPool(whirlwindPool, this);
        return whirlwind;
    }
    private void OnGetWhirlwind(WhirlwindSkillDamage whirlwind)
    {

    }
    private void OnReleaseWhirlwind(WhirlwindSkillDamage whirlwind)
    {
        whirlwind.transform.SetParent(null);
        whirlwind.gameObject.SetActive(false);
    }
    private void OnDestroyWhirlwind(WhirlwindSkillDamage whirlwind)
    {
        if (whirlwind != null) Destroy(whirlwind.gameObject);
    }

    //레이저
    private void StartLaser(SkillDataSO data)
    {
        if (skillTarget == null) return;
        if (data.LaserPrefab == null) return;

        LaserSkillDamage prefab = data.LaserPrefab.GetComponent<LaserSkillDamage>();
        if (prefab == null) return;
        if (laserPool == null) CreateLaserPool(prefab);
        if (laserPool == null) return;

        Transform point = skillVfxPoint != null ? skillVfxPoint : transform;
        //생성 위치와 상관없이 유닛 중심에서 타겟 방향 계산
        Vector3 direction = skillTarget.transform.position - unit.transform.position;
        direction.y = 0.0f;
        if (direction.sqrMagnitude <= 0.001f) direction = unit.transform.forward;
        direction.Normalize();

        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);
        LaserSkillDamage laser = laserPool.Get();
        laser.transform.SetParent(null);
        laser.transform.SetPositionAndRotation(point.position, rotation);
        laser.gameObject.SetActive(true);
        laser.Initialize(unit, data.LaserDuration, data.LaserHitInterval, data.DamageRatio);
    }
    private void CreateLaserPool(LaserSkillDamage prefab)
    {
        if (laserPool != null) return;
        if (prefab == null) return;

        laserPoolPrefab = prefab;
        laserPool = new ObjectPool<LaserSkillDamage>(
            CreateLaser,
            OnGetLaser,
            OnReleaseLaser,
            OnDestroyLaser,
            true,
            3,
            10);
    }
    private LaserSkillDamage CreateLaser()
    {
        LaserSkillDamage laser = Instantiate(laserPoolPrefab);
        laser.gameObject.SetActive(false);
        laser.SetPool(laserPool, this);
        return laser;
    }
    private void OnGetLaser(LaserSkillDamage laser)
    {

    }
    private void OnReleaseLaser(LaserSkillDamage laser)
    {
        laser.transform.SetParent(null);
        laser.gameObject.SetActive(false);
    }
    private void OnDestroyLaser(LaserSkillDamage laser)
    {
        if (laser != null) Destroy(laser.gameObject);
    }
    #endregion

    #region 힐, 버프 스킬 VFX
    private void PlayBuffCastVfx(SkillDataSO data)
    {
        if (data.BuffCastVFxPrefab == null) return;
        if (buffCastVfxPool == null) CreateBuffCastVfxPool(data.BuffCastVFxPrefab);
        if (buffCastVfxPool == null) return;

        Transform point = skillVfxPoint != null ? skillVfxPoint : transform;
        GameObject vfx = buffCastVfxPool.Get();
        vfx.transform.SetParent(point);
        vfx.transform.SetPositionAndRotation(point.position, point.rotation);
        vfx.SetActive(true);
        RestartParticles(vfx);
        StartCoroutine(ReleaseVfxRoutine(vfx, buffCastVfxPool, data.BuffCastVfxDuration));
    }
    //힐 관련
    private void PlayHealCastVfx(SkillDataSO data)
    {
        if (data.HealCastVfxPrefab == null) return;
        if (healCastVfxPool == null) CreateHealCastVfxPool(data.HealCastVfxPrefab);
        if (healCastVfxPool == null) return;

        Transform point = skillVfxPoint != null ? skillVfxPoint : transform;
        GameObject vfx = healCastVfxPool.Get();
        vfx.transform.SetParent(point);
        vfx.transform.SetPositionAndRotation(point.position, point.rotation);
        vfx.SetActive(true);
        RestartParticles(vfx);
        StartCoroutine(ReleaseVfxRoutine(vfx, healCastVfxPool, data.HealVfxDuration));
    }
    private void PlayHealTargetVfx(BattleUnit target, SkillDataSO data)
    {
        if (target == null) return;
        if (data.HealTargetVfxPrefab == null) return;
        if (healTargetVfxPool == null) CreateHealTargetVfxPool(data.HealTargetVfxPrefab);
        if (healTargetVfxPool == null) return;

        GameObject vfx = healTargetVfxPool.Get();
        vfx.transform.SetPositionAndRotation(target.transform.position, Quaternion.identity);

        vfx.SetActive(true);
        RestartParticles(vfx);
        StartCoroutine(ReleaseVfxRoutine(vfx, healTargetVfxPool, data.HealVfxDuration));
    }
    private void CreateBuffCastVfxPool(GameObject prefab)
    {
        buffCastVfxPool = new ObjectPool<GameObject>(
            () => CreateVfx(prefab),
            OnGetVfx,
            OnReleaseVfx,
            OnDestroyVfx,
            true,
            3,
            10);
    }
    private void CreateHealCastVfxPool(GameObject prefab)
    {
        healCastVfxPool = new ObjectPool<GameObject>(
            () => CreateVfx(prefab),
            OnGetVfx,
            OnReleaseVfx,
            OnDestroyVfx,
            true,
            3,
            10);
    }
    private void CreateHealTargetVfxPool(GameObject prefab)
    {
        healTargetVfxPool = new ObjectPool<GameObject>(
            () => CreateVfx(prefab),
            OnGetVfx,
            OnReleaseVfx,
            OnDestroyVfx,
            true,
            3,
            10);
    }
    private GameObject CreateVfx(GameObject prefab)
    {
        GameObject vfx = Instantiate(prefab);
        vfx.SetActive(false);
        return vfx;
    }
    private void OnGetVfx(GameObject vfx)
    {
        //위치 설정 후 직접 활성화
    }
    private void OnReleaseVfx(GameObject vfx)
    {
        vfx.SetActive(false);
    }
    private void OnDestroyVfx(GameObject vfx)
    {
        if (vfx != null) Destroy(vfx);
    }
    private void RestartParticles(GameObject vfx)
    {
        ParticleSystem[] particles = vfx.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles[i].Play(true);
        }
    }
    private IEnumerator ReleaseVfxRoutine(GameObject vfx, IObjectPool<GameObject> pool, float duration)
    {
        yield return new WaitForSeconds(Mathf.Max(0.01f, duration));
        if (vfx == null) yield break;
        pool.Release(vfx);
    }
    #endregion

    #region 관통 투사체 스킬
    //투사체 스킬 발사 함수
    private void FireProjectile(BattleUnit target, SkillDataSO data)
    {
        if (target == null || data.ProjectilePrefab == null) return;

        Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position + Vector3.up;
        //생성 위치와 상관없이 유닛 중심에서 타겟 방향 계산
        Vector3 direction = target.transform.position - unit.transform.position;
        direction.y = 0.0f;
        if (direction.sqrMagnitude <= 0.001f) direction = unit.transform.forward;
        direction.Normalize();

        int skillAttack = Mathf.RoundToInt(unit.AttackPower * data.DamageRatio);
        SkillProjectile projectile;
        //현재 풀과 같은 프리팹이면 재사용
        if (projectilePool != null && projectilePoolPrefab == data.ProjectilePrefab)
        {
            projectile = projectilePool.Get();
        }
        else if (projectilePool == null)
        {
            CreateProjectilePool(data.ProjectilePrefab);
            projectile = projectilePool != null ? projectilePool.Get() : null;
        }
        else
        {
            projectile = Instantiate(data.ProjectilePrefab);
        }
        if (projectile == null) return;

        projectile.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        projectile.Initialize(unit, direction, skillAttack, data.ProjectileSpeed);
        projectile.gameObject.SetActive(true);
    }
    private void CreateProjectilePool(SkillProjectile prefab)
    {
        if (projectilePool != null) return;
        if (prefab == null) return;

        projectilePoolPrefab = prefab;
        projectilePool = new ObjectPool<SkillProjectile>(
            CreateSkillProjectile,
            OnGetSkillProjectile,
            OnReleaseSkillProjectile,
            OnDestroySkillProjectile,
            true,
            3,
            10);
    }
    private SkillProjectile CreateSkillProjectile()
    {
        if (projectilePoolPrefab == null) return null;

        SkillProjectile projectile = Instantiate(projectilePoolPrefab);
        projectile.gameObject.SetActive(false);
        projectile.SetPool(projectilePool, this);
        return projectile;
    }
    private void OnGetSkillProjectile(SkillProjectile projectile)
    {
        //위치와 런타임 초기화 추 FireProjectile에서 활성화
    }
    private void OnReleaseSkillProjectile(SkillProjectile projectile)
    {
        projectile.gameObject.SetActive(false);
    }
    private void OnDestroySkillProjectile(SkillProjectile projectile)
    {
        if (projectile != null) Destroy(projectile.gameObject);
    }
    #endregion

    #region 타겟 관련
    private BattleUnit GetSkillTarget(SkillDataSO data)
    {
        if (unit == null || data == null) return null;

        switch (data.EffectType)
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
            case SkillEffectType.AreaDamage:
                return unit.Target;
            case SkillEffectType.Whirlwind:
                return unit.Target;
            case SkillEffectType.Laser:
                return unit.Target;
        }

        return null;
    }
    private bool IsValidTarget(BattleUnit target, SkillDataSO data)
    {
        if (target == null || target.IsDead || !target.gameObject.activeInHierarchy) return false;

        switch (data.EffectType)
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
            case SkillEffectType.AreaDamage:
                return target.Team != unit.Team && unit.IsTargetInAttackRange(target);
            case SkillEffectType.Whirlwind:
                return target.Team != unit.Team && unit.IsTargetInAttackRange(target);
            case SkillEffectType.Laser:
                return target.Team != unit.Team && unit.IsTargetInAttackRange(target);
        }

        return false;
    }
    #endregion

    #region 배틀 이벤트 구독 관련
    private void SubscribeBattleEvent()
    {
        if (isBattleEventSubscribed) return;
        if (BattleManager.Instance == null) return;

        BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
        isBattleEventSubscribed = true;
    }
    private void UnsubscribeBattleEvent()
    {
        if (isBattleEventSubscribed && BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
        }
        isBattleEventSubscribed = false;
    }
    #endregion


    private bool NeedsEnemyTarget(SkillDataSO data)
    {
        if (data == null) return false;

        return data.EffectType == SkillEffectType.Damage ||
            data.EffectType == SkillEffectType.ProjectileDamage ||
            data.EffectType == SkillEffectType.AreaDamage ||
            data.EffectType == SkillEffectType.Whirlwind ||
            data.EffectType == SkillEffectType.Laser;
    }
}
