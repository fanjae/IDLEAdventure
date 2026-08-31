using UnityEngine;

/// <summary>
/// 캐릭터 수종 조작 상태 클래스.
/// </summary>
public class AdventurePlayerControlState : AdventurePlayerState
{
    private float gravity = -9.81f;

    public override void OnEnter()
    {
        base.OnEnter();

        if (PathManager.Instance != null)
        {
            PathManager.Instance.HideLine();
        }

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
        // 중력 계산
        // 바닥에 붙어있더라도 살짝 눌러주기
        if (stateMachine.CharacterController.isGrounded)
        {
            gravity = -1.0f;
        }
        // 바닥에서 떨어져 있다면 중력값 적용
        else
        {
            gravity += Physics.gravity.y * Time.deltaTime;
        }

        Vector2 input = stateMachine.VirtualPad.InputDirection;

        stateMachine.SetAnimatorSpeed(input.magnitude);

        Vector3 finalMoveDirection = Vector3.zero;

        if (input.sqrMagnitude >= 0.01f)
        {
            // 쿼터뷰 등 상황에서 이동할 때 카메라 뷰에 맞게 이동하게끔 변환을 위한 작업
            // 카메라가 실제로 바라보는 방향 값 받아오기
            Vector3 cameraForward = stateMachine.MainCamera.transform.forward;
            Vector3 cameraRight = stateMachine.MainCamera.transform.right;

            // 카메라 방향 벡터 Y축 고정
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

            finalMoveDirection = moveDirection * stateMachine.MoveSpeed;
        }
        // 중력 적용
        finalMoveDirection.y = gravity;

        // 캐릭터 컨트롤러의 Move 함수를 통한 이동
        stateMachine.CharacterController.Move(finalMoveDirection * Time.deltaTime);
    }
}