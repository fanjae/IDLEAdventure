using System.Collections.Generic;
using UnityEngine;

public class SkillProjectile : MonoBehaviour
{
    [Header("스킬 투사체 지속시간")]
    [SerializeField] private float lifetime = 4.0f;

    private BattleUnit owner;
    private Vector3 moveDir;

    private int attackPower;
    private float speed;
    private float currentLifeTime;

    private bool initialized;
    //투사체가 같은 적에게 여러번 데미지를 주는 것을 방지하기 위해 사용
    private readonly HashSet<BattleUnit> hitTargets = new HashSet<BattleUnit>();

    public void Initialize(BattleUnit owner, Vector3 direction, int damage, float speed)
    {
        this.owner = owner;
        this.attackPower = Mathf.Max(0, damage);
        this.speed = Mathf.Max(0.1f, speed);

        moveDir = direction;
        moveDir.y = 0.0f;
        if (moveDir.sqrMagnitude <= 0.001f) moveDir = owner.transform.forward;
        moveDir.Normalize();

        currentLifeTime = 0.0f;
        hitTargets.Clear();

        initialized = true;

        UpdateRotation();
    }

    void Update()
    {
        if (!initialized) return;

        transform.position += moveDir * speed * Time.deltaTime;
        currentLifeTime += Time.deltaTime;
        if (currentLifeTime >= lifetime) Destroy(gameObject);
    }

    private void UpdateRotation()
    {
        if (moveDir.sqrMagnitude <= 0.001f) return;
        //VFX가 X축 방향이 진행 방향 이어서.
        transform.rotation = Quaternion.FromToRotation(Vector3.right, moveDir);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!initialized || owner == null) return;

        BattleUnit target = other.GetComponentInParent<BattleUnit>();
        if (target == null || target == owner || target.IsDead) return;
        if (target.Team == owner.Team) return;//같은 팀도 무시
        if (!hitTargets.Add(target)) return;//동일한 적에게는 딱 한 번만 피해를 입힘

        int finalDamage = DamageCalculator.Calculate(attackPower, target.Defense);
        int appliedDamage = target.TakeDamage(finalDamage, owner);

        Debug.Log($"[관통 스킬] {owner.name} -> {target.name} / 피해량 : {appliedDamage} / 적중 수 : {hitTargets.Count}");
    }
}
