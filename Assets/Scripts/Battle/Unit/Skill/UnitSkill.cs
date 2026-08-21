using UnityEngine;

public class UnitSkill : MonoBehaviour
{
    [Header("스킬 설정")]
    [SerializeField] private SkillDataSO skillData;
    [SerializeField] private SkillDataSO secondarySkillData;

    [Header("투사체 스킬 발사 위치")]
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("시전자 VFX 위치")]
    [SerializeField] private Transform skillVfxPoint;

    private BattleUnit unit;
    private BattleUnit skillTarget; //스킬 시작 당시의 대상을 보관하는 용

    private SkillDataSO activeSkillData;

    private float nextSkillAvailableTime;
    private float nextSecondarySkillAvailableTime;
    private float skillSafetyEndTime;

    private bool isUsingSkill; //스킬 사용 중인지 확인
    private bool hasAppliedSkillEffect; //한 번의 스킬에서 효과가 중복 적용되는 것 방지용

    private bool hasBattleStarted;

    public bool IsUsingSkill => isUsingSkill;

    public bool HasSkill => skillData != null || secondarySkillData != null;

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

    public void Initialize(BattleUnit unit)
    {
        this.unit = unit;

        activeSkillData = null;
        skillTarget = null;

        isUsingSkill = false;
        hasAppliedSkillEffect = false;

        skillSafetyEndTime = 0.0f;

        hasBattleStarted = false;

        nextSkillAvailableTime = 0.0f;
        nextSecondarySkillAvailableTime = 0.0f;

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
        }
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
        }
    }

    //전투 시작
    private void HandleBattleStarted()
    {
        hasBattleStarted = true;

        if (skillData != null)
        {
            //전투가 실제로 시작된 순간부터 첫 스킬 쿨타임 시작
            nextSkillAvailableTime = Time.time + skillData.Cooldown;
        }
        else
        {
            nextSkillAvailableTime = 0.0f;
        }

        if (secondarySkillData != null)
        {
            nextSecondarySkillAvailableTime = Time.time + secondarySkillData.Cooldown;
        }
        else
        {
            nextSecondarySkillAvailableTime = 0.0f;
        }
    }
    private SkillDataSO GetAvailableSkill()
    {
        //1순위 스킬
        if (IsSkillAvailable(skillData, nextSkillAvailableTime)) return skillData;
        //2순위 스킬
        if (IsSkillAvailable(secondarySkillData, nextSecondarySkillAvailableTime)) return secondarySkillData;

        return null;
    }
    private bool IsSkillAvailable(SkillDataSO data, float nextAvailableTime)
    {
        if (data == null) return false;
        if (Time.time < nextAvailableTime) return false;

        BattleUnit target = GetSkillTarget(data);
        return IsValidTarget(target, data);
    }
    public bool CanUseSkill()
    {
        if (!hasBattleStarted) return false;
        if (unit == null || unit.IsDead || isUsingSkill) return false;

        return GetAvailableSkill() != null;
    }
    public bool UseSkill()
    {
        if (!CanUseSkill()) return false;

        SkillDataSO selectedSkill = GetAvailableSkill();
        if (selectedSkill == null) return false;

        BattleUnit target = GetSkillTarget(selectedSkill);
        if (!IsValidTarget(target, selectedSkill)) return false;
        //애니메이션을 시작할 수 있을 때만 스킬 사용 상태로 진입
        if (!unit.TryPlaySkillAnimation()) return false;

        activeSkillData = selectedSkill;
        skillTarget = target;

        isUsingSkill = true;
        hasAppliedSkillEffect = false;

        //실제로 사용한 스킬만 쿨타임 시작
        if (selectedSkill == skillData)
        {
            //스킬 시작 시점을 기준으로 다음 사용 가능 시간 계산
            nextSkillAvailableTime = Time.time + selectedSkill.Cooldown;
        }
        else if (selectedSkill == secondarySkillData)
        {
            nextSecondarySkillAvailableTime = Time.time + selectedSkill.Cooldown;
        }
        //SkillEnd 애니메이션 이벤트 누락 대비
        skillSafetyEndTime = Time.time + selectedSkill.SkillSafetyDuration;

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
    //SkillActivate 애니메이션 이벤트에서 호출
    public void ApplySkillEffect()
    {
        if (!isUsingSkill || hasAppliedSkillEffect || activeSkillData == null) return;

        switch (activeSkillData.EffectType)
        {
            case SkillEffectType.Damage:
                if (!IsValidTarget(skillTarget, activeSkillData)) return;
                hasAppliedSkillEffect = true;
                unit.FaceTarget();
                ApplyDamage(skillTarget, activeSkillData);
                break;
            case SkillEffectType.Heal:
                if (!IsValidTarget(skillTarget, activeSkillData)) return;
                hasAppliedSkillEffect = true;
                ApplyHeal(skillTarget, activeSkillData);
                break;
            case SkillEffectType.Barrier:
                hasAppliedSkillEffect = true;
                ApplyBarrier(activeSkillData);
                break;
            case SkillEffectType.ProjectileDamage:
                if (!IsValidTarget(skillTarget, activeSkillData)) return;
                hasAppliedSkillEffect = true;
                unit.FaceTarget();
                FireProjectile(skillTarget, activeSkillData);
                break;
            case SkillEffectType.Buff:
                hasAppliedSkillEffect = true;
                ApplyBuff(activeSkillData);
                break;
            case SkillEffectType.AreaDamage:
                if (!IsValidTarget(skillTarget, activeSkillData)) return;
                hasAppliedSkillEffect = true;
                unit.FaceTarget();
                ApplyAreaDamage(skillTarget, activeSkillData);
                break;
                //휠윈드
            case SkillEffectType.Whirlwind:
                if (!IsValidTarget(skillTarget, activeSkillData)) return;
                hasAppliedSkillEffect = true;
                unit.FaceTarget();
                StartWhirlwind(activeSkillData);
                break;
            case SkillEffectType.Laser:
                if (!IsValidTarget(skillTarget, activeSkillData)) return;
                hasAppliedSkillEffect = true;
                unit.FaceTarget();
                StartLaser(activeSkillData);
                break;
        }
    }
    //SkillEnd 애니메이션 이벤트에서 호출
    public void CompleteSkill()
    {
        if (!isUsingSkill) return;

        isUsingSkill = false;
        hasAppliedSkillEffect = false;

        activeSkillData = null;
        skillTarget = null;

        skillSafetyEndTime = 0.0f;
    }
    //상태 변경 또는 사망 등으로 진행 중인 스킬 취소
    public void CancelSkill()
    {
        isUsingSkill = false;
        hasAppliedSkillEffect = false;

        activeSkillData = null;
        skillTarget = null;

        skillSafetyEndTime = 0.0f;
    }
    //전투 재시작 또는 오브젝트 재사용 시 스킬 상태 초기화
    public void ResetSkill()
    {
        isUsingSkill = false;
        hasAppliedSkillEffect = false;

        activeSkillData = null;
        skillTarget = null;

        skillSafetyEndTime = 0.0f;

        nextSkillAvailableTime = skillData != null ? Time.time + skillData.Cooldown : 0.0f;
        nextSecondarySkillAvailableTime = secondarySkillData != null ? Time.time + secondarySkillData.Cooldown : 0.0f;
    }

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
    private void ApplyDamage(BattleUnit target, SkillDataSO data)
    {
        int skillAttack = Mathf.RoundToInt(unit.AttackPower * data.DamageRatio);
        int finalDamage = DamageCalculator.Calculate(skillAttack, target.Defense);

        int appliedDamage = target.TakeDamage(finalDamage);
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
    private void PlayBuffCastVfx(SkillDataSO data)
    {
        if (data.BuffCastVFxPrefab == null) return;

        Transform point = skillVfxPoint != null ? skillVfxPoint : transform;
        GameObject vfx = Instantiate(data.BuffCastVFxPrefab, point.position, point.rotation, point);
        Destroy(vfx, data.BuffCastVfxDuration);
    }
    //투사체 스킬 발사 함수
    private void FireProjectile(BattleUnit target, SkillDataSO data)
    {
        if (target == null || data.ProjectilePrefab == null) return;

        Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position + Vector3.up;
        //발사하는 순간 타겟이 있던 방향 저장
        Vector3 direction = target.transform.position - spawnPosition;
        direction.y = 0.0f;
        if (direction.sqrMagnitude <= 0.001f) direction = unit.transform.forward;
        direction.Normalize();

        int skillAttack = Mathf.RoundToInt(unit.AttackPower * data.DamageRatio);
        SkillProjectile projectile = Instantiate(data.ProjectilePrefab, spawnPosition, Quaternion.identity);
        projectile.Initialize(unit, direction, skillAttack, data.ProjectileSpeed);
    }
    //힐 관련
    private void PlayHealCastVfx(SkillDataSO data)
    {
        if (data.HealCastVfxPrefab == null) return;

        Transform point = skillVfxPoint != null ? skillVfxPoint : transform;
        GameObject vfx = Instantiate(data.HealCastVfxPrefab, point.position, point.rotation, point);

        Destroy(vfx, data.HealVfxDuration);
    }
    private void PlayHealTargetVfx(BattleUnit target, SkillDataSO data)
    {
        if (target == null) return;
        if (data.HealTargetVfxPrefab == null) return;

        GameObject vfx = Instantiate(data.HealTargetVfxPrefab, target.transform.position, Quaternion.identity, target.transform);

        Destroy(vfx, data.HealVfxDuration);
    }

    //광역 단타 스킬
    private void ApplyAreaDamage(BattleUnit target, SkillDataSO data)
    {
        if (target == null) return;
        if (data.AreaDamagePrefab == null) return;

        Vector3 targetPosition = target.transform.position;
        int skillAttack = Mathf.RoundToInt(unit.AttackPower * data.DamageRatio);
        AreaSkillDamage areaSkill = Instantiate(data.AreaDamagePrefab, targetPosition, Quaternion.identity);
        areaSkill.SetAreaRadius(data.AreaRadius);
        areaSkill.ApplyDamage(unit, skillAttack);
    }
    //휠윈드
    private void StartWhirlwind(SkillDataSO data)
    {
        if (data.WhirlwindPrefab == null) return;

        Transform point = skillVfxPoint != null ? skillVfxPoint : transform;
        GameObject whirlwindObject = Instantiate(data.WhirlwindPrefab, point.position, point.rotation, point);
        WhirlwindSkillDamage whirlwind = whirlwindObject.GetComponent<WhirlwindSkillDamage>();
        if (whirlwind == null)
        {
            Destroy(whirlwindObject);
            return;
        }
        whirlwind.Initialize(unit, data.WhirlwindDuration, data.WhirlwindHitInterval, data.DamageRatio);
    }
    //레이저
    private void StartLaser(SkillDataSO data)
    {
        if (skillTarget == null) return;
        if (data.LaserPrefab == null) return;

        Transform point = skillVfxPoint != null ? skillVfxPoint : transform;
        Vector3 direction = skillTarget.transform.position - point.position;
        direction.y = 0.0f;
        if (direction.sqrMagnitude <= 0.001f) direction = unit.transform.forward;
        direction.Normalize();

        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);
        GameObject laserObject = Instantiate(data.LaserPrefab, point.position, rotation);
        LaserSkillDamage laser = laserObject.GetComponent<LaserSkillDamage>();
        if (laser == null)
        {
            Destroy(laserObject);
            return;
        }
        
        laser.Initialize(unit, data.LaserDuration, data.LaserHitInterval, data.DamageRatio);
    }

}
