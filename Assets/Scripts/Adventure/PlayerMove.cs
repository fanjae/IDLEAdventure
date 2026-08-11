using UnityEngine;

/// <summary>
/// 가상 패드 입력을 받아 플레이어 객체를 이동시키는 클래스. <br/>
/// 쿼터뷰에 가까운 카메라 시점을 가지고 있기에 움직일 때 바닥을 뚫거나, 하늘로 솟아오르는 등의 문제점을 해결하기 위해 카메라의 벡터값을 받아와 변환 후 이동. <br/>
/// X축만 회전한 뷰라면, 추가 계산 없이 늘 하던대로 new Vector3(input.x, 0.0f, input.y)로 구현해도 결과는 동일할 것. <br/>
/// 애니메이션은 다양할 필요가 없기에 별도 조건, 상태 등 없이 속도만을 받아 속도에 맞는 애니메이션을 재생하도록 구현. <br/>
/// 애니메이터에 노드와 트렌지션으로 잇는 방법이 아닌 Blend Tree라는 것을 사용해봄. 같은 파라미터 값으로 자연스럽게 애니메이션이 이어져야 하는 상황에서 괜찮은듯?
/// </summary>
public class PlayerMove : MonoBehaviour
{
    [Header("Virtual Pad")]
    [SerializeField] private VirtualPad virtualPad;

    [Header("Move Setting")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float rotationSpeed = 12.0f;

    private CharacterController characterController;
    private Animator animator;
    private Camera mainCamera;

    private readonly string paramSpeed = "Speed";

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Move();
    }

    // 실제 이동 함수
    private void Move()
    {
        Vector2 input = virtualPad.InputDirection;

        AnimatorParamSet(input);

        if (input.sqrMagnitude < 0.01f) return;

        // 쿼터뷰 등 상황에서 이동할 때 카메라 뷰에 맞게 이동하게끔 변환을 위한 작업
        // 카메라가 실제로 바라보는 방향 값 받아오기
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
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
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);

        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
    // 애니메이터 파라미터 정보 전달 함수
    private void AnimatorParamSet(Vector2 input)
    {
        float currentSpeed = input.magnitude;

        animator.SetFloat(paramSpeed, currentSpeed);
    }
}