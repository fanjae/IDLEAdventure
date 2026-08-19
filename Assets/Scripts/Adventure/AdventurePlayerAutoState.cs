using System;
using UnityEngine;

/// <summary>
/// 캐릭터 자동 이동 상태 클래스.
/// </summary>
public class AdventurePlayerAutoState : AdventurePlayerState
{
    // 퀘스트 파트에서 받아올 다이얼로그 출력 함수를 저장하기 위한 변수.
    private Action onArrived;

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

    public void SetTarget(Vector3 targetPos, Action onArrived = null)
    {
        this.onArrived = onArrived;

        if (stateMachine.Agent != null)
        {
            stateMachine.Agent.SetDestination(targetPos);
        }
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
    // 도착 시 실행할 함수 제거 후 수동 조작 상태로 전환.
    private void StopAutoMove()
    {
        onArrived = null;
        stateMachine.ChangeState(stateMachine.PlayerControlState);
    }
    // 목표에 제대로 도착 시 실행할 함수
    // 수동 조작 상태로 전환 후 저장된 Action 함수 실행 후 제거.
    private void OnArrived()
    {
        stateMachine.ChangeState(stateMachine.PlayerControlState);

        onArrived?.Invoke();
        onArrived = null;
    }
}