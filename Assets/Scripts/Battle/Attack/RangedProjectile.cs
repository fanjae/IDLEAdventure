using UnityEngine;
using UnityEngine.Pool;

public class RangedProjectile : MonoBehaviour
{
    [Header("이동 관련 설정")]
    [SerializeField, Min(0.1f)] private float moveSpeed = 11.0f;
    //타겟 명중 판정 거리
    [SerializeField, Min(0.01f)] private float hitDistance = 0.2f;
    [SerializeField, Min(0.1f)] private float lifeTime = 5.0f;
    
    [Header("타겟 위치 설정")]
    //높이 보정 용
    //히어로 크기, 몬스터 크기로 인한 도착점, 사용할 VFX의 목적성을 고려해서 추가하게 됨.
    //(슬라임, 고블린, 오크의 크기가 다름으로 인해서 이상한 곳에 맞을 수도 있어서)
    //(인스펙터에서 수치 조정만으로 해결하기 위해서 추가함)
    //발사 위치에서 일직선으로 날아가게 할거면 0으로 두면 됨
    [SerializeField] private float targetHeight = 0.0f;

    private BattleUnit target;
    private BattleUnit owner;
    private int damage;
    private float remainingLifeTime; // 투사체의 남은 생존시간
    private bool isFinished;

    private IObjectPool<RangedProjectile> pool;
    private UnitAttack poolOwner;


    public void SetPool(IObjectPool<RangedProjectile> pool, UnitAttack poolOwner)
    {
        this.pool = pool;
        this.poolOwner = poolOwner;
    }

    public void Initialize(BattleUnit owner, BattleUnit target, int damage)
    {
        this.owner = owner;
        this.target = target;
        this.damage = Mathf.Max(0, damage);

        remainingLifeTime = lifeTime;
        isFinished = false;

        FaceTarget();
    }

    void Update()
    {
        if (isFinished) return;
        //명중하기 전에 타겟이 제거되거나 다른 공격으로 사망처리가 됐으면, 피해를 적용하지 않고 투사체만 제거
        if (target == null || target.IsDead)
        {
            Finish();
            return;
        }

        remainingLifeTime -= Time.deltaTime;
        if (remainingLifeTime <= 0.0f)
        {
            Finish();
            return;
        }

        MoveToTarget();
    }

    private void MoveToTarget()
    {
        Vector3 targetPosition = target.transform.position + Vector3.up * targetHeight;
        Vector3 dir = targetPosition - transform.position;
        float moveDistance = moveSpeed * Time.deltaTime;
        //투사체가 빠른 경우에 한 프레임 사이에 타겟을 통과하는 문제가 발생할 수 있음.
        //이번 프레임 이동 거리를 명중 거리에도 포함시켜서 위와 같은 문제를 방지.
        float currentHitDistance = Mathf.Max(hitDistance, moveDistance);

        if (dir.sqrMagnitude <= currentHitDistance * currentHitDistance)
        {
            HitTarget();
            return;
        }

        Vector3 moveDir = dir.normalized;
        transform.position += moveDir * moveDistance;
        transform.rotation = Quaternion.LookRotation(moveDir);
    }
    private void FaceTarget()
    {
        if (target == null) return;

        Vector3 targetPosition = target.transform.position + Vector3.up * targetHeight;
        Vector3 dir = targetPosition - transform.position;
        if (dir.sqrMagnitude <= 0.01f) return;

        transform.rotation = Quaternion.LookRotation(dir.normalized);
    }
    private void HitTarget()
    {
        //이동 중에 타겟이 먼저 사망했는지, 명중 직전에 재확인
        if (target != null && !target.IsDead)
        {
            int appliedDamage = target.TakeDamage(damage, owner);
            if (appliedDamage > 0) BattleSoundController.Instance?.PlayBasicHit();
        }
        Finish();
    }
    private void Finish()
    {
        if (isFinished) return;
        isFinished = true;
        
        owner = null;
        target = null;
        damage = 0;
        remainingLifeTime = 0.0f;
        //풀을 만든 UnitAttack이 아직 존재하면 풀로 반환
        if (pool != null && poolOwner != null)
        {
            pool.Release(this);
            return;
        }
        //풀의 주인이 이미 제거된 경우 그냥 제거
        Destroy(gameObject);
    }
}
