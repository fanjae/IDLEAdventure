using UnityEngine;

/// <summary>
/// 캐릭터 자동 이동 상태 클래스.
/// </summary>
public class AdventurePlayerAutoState : AdventurePlayerState
{
    // 테스트 이동용 위치
    [Header("Test Target")]
    [SerializeField] private Transform testTarget;

    public override void OnEnter()
    {
        base.OnEnter();

        stateMachine.Agent.enabled = true;
        stateMachine.Agent.speed = stateMachine.MoveSpeed;
        stateMachine.Agent.angularSpeed = stateMachine.RotationSpeed * 10.0f;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        AutoMove();
    }

    public void SetTarget(Vector3 targetPos)
    {
        stateMachine.Agent.SetDestination(targetPos);
    }
    
    private void AutoMove()
    {
        // 버추얼 패드 조작을 확인
        Vector2 input = stateMachine.VirtualPad.InputDirection;
        // 조작이 있다면 자동이동 중지
        if (input.sqrMagnitude > 0.01f)
        {
            StopAutoMove();
            return;
        }

        // 속도 부드럽게 올라가게 보정
        float currentSpeed = stateMachine.Agent.velocity.magnitude / stateMachine.Agent.speed;
        stateMachine.SetAnimatorSpeed(currentSpeed);

        // 경로가 확보 되었는지 + 목표와의 거리가 충분히 가까운지 확인
        if (!stateMachine.Agent.pathPending && stateMachine.Agent.remainingDistance <= stateMachine.Agent.stoppingDistance)
        {
            // 경로가 끝났거나, 객체가 정지했는지 확인
            if (!stateMachine.Agent.hasPath || stateMachine.Agent.velocity.sqrMagnitude == 0.0f)
            {
                // 도착 함수 실행
                OnArrived();
            }
        }
    }
    // 이동 강제 종료 함수
    private void StopAutoMove()
    {
        stateMachine.ChangeState(stateMachine.PlayerControlState);
    }
    // 목표에 제대로 도착 시 실행할 함수
    private void OnArrived()
    {
        stateMachine.ChangeState(stateMachine.PlayerControlState);
    }

    // 테스트용 퀘스트 버튼 클릭 시 정해진 위치로 이동하게끔 한 함수
    public void OnClickQuestButton()
    {
        if (testTarget != null)
        {
            stateMachine.ChangeState(this);
            SetTarget(testTarget.position);
        }
    }
}