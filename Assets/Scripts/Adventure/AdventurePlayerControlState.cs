using UnityEngine;

/// <summary>
/// 캐릭터 수종 조작 상태 클래스.
/// </summary>
public class AdventurePlayerControlState : AdventurePlayerState
{
    public override void OnEnter()
    {
        base.OnEnter();
        
        if (stateMachine.Agent != null)
        {
            stateMachine.Agent.enabled = false;
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        Move();
    }

    private void Move()
    {
        Vector2 input = stateMachine.VirtualPad.InputDirection;

        stateMachine.SetAnimatorSpeed(input.magnitude);

        if (input.sqrMagnitude < 0.01f) return;

        // 쿼터뷰 등 상황에서 이동할 때 카메라 뷰에 맞게 이동하게끔 변환을 위한 작업
        // 카메라가 실제로 바라보는 방향 값 받아오기
        Vector3 cameraForward = stateMachine.MainCamera.transform.forward;
        Vector3 cameraRight = stateMachine.MainCamera.transform.right;

        // Y축 고정
        // 바닥에 붙어서 다녀야하기 때문
        cameraForward.y = 0.0f;
        cameraRight.y = 0.0f;

        // 방향 벡터를 정규화
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 카메라 방향을 기준으로 패드 입력값을 받아 최종 이동 방향 벡터 계산
        Vector3 moveDirection = (cameraForward * input.y) + (cameraRight * input.x);

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, stateMachine.RotationSpeed * Time.deltaTime);

        // 캐릭터 컨트롤러의 Move 함수를 통한 이동
        stateMachine.CharacterController.Move(moveDirection * stateMachine.MoveSpeed * Time.deltaTime);
    }
}