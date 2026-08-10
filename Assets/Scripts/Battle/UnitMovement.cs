using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : MonoBehaviour
{
    private NavMeshAgent unit;

    private Vector3 lastDestination;
    private bool hasDestination;

    public bool CanMove
    {
        get
        {
            return unit != null && unit.enabled && unit.isOnNavMesh;
        }
    }

    public bool IsMoving
    {
        get
        {
            if (!CanMove || unit.isStopped) return false;
            return unit.velocity.sqrMagnitude > 0.01f;
        }
    }

    
    
    private void Awake()
    {
        unit = GetComponent<NavMeshAgent>();

        unit.updateRotation = true;//이동 중에는 이동 방향을 바라보게
        unit.updateUpAxis = true;
        unit.autoRepath = true;
    }
    public void Initialize(float moveSpeed, float attackRange)
    {
        unit.speed = Mathf.Max(0.1f, moveSpeed);
        //사거리 밖에서 멈추고 사거리 끝에 적이 걸리고 그 적이 살짝 움직여서 덜덜거리는걸 방지
        //과하게 파고들지 않고 사거리 살짝 안쪽에서 멈추게 함(NavMesh 반지름, 거리오차때문에 생기는 현상)
        unit.stoppingDistance = Mathf.Max(unit.radius *2.0f, attackRange * 0.9f);
    }
    public void MoveTo(Vector3 position)
    {
        if (!CanMove) return;
        if (hasDestination)
        {
            Vector3 difference = position - lastDestination;
            difference.y = 0.0f;

            if (difference.sqrMagnitude <= 0.01f) return;
        }

        unit.isStopped = false;

        lastDestination = position;
        hasDestination = true;
        unit.SetDestination(position);
    }
    public void Stop()
    {
        if (!CanMove) return;
        unit.isStopped = true;
        if (unit.hasPath) unit.ResetPath();
        hasDestination = false;
    }
    public void FaceTarget(Transform target, float rotateSpeed)
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0.0f;
        if (dir.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
}
