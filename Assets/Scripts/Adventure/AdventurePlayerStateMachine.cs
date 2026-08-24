using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 플레이어의 상태를 관리할 상태머신 클래스. <br/>
/// 공통으로 사용할 컴포넌트들과 상태들 가진다.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NavMeshAgent))]
public class AdventurePlayerStateMachine : MonoBehaviour
{
    // 상태들이 공통으로 사용할 컴포넌트들
    [Header("Component Binding")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Animator animator;
    // 수동 조작을 위한 가상 패드
    [Header("Virtual Pad")]
    [SerializeField] private VirtualPad virtualPad;
    // 상태들이 공통으로 사용할 속도
    [Header("Move Setting")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float rotationSpeed = 12.0f;
    // 상태 스크립트들
    [Header("States")]
    [SerializeField] private AdventurePlayerControlState playerControlState;
    [SerializeField] private AdventurePlayerAutoState playerAutoState;
    // 현재 상태 저장용
    private AdventurePlayerState currentState;
    // 애니메이터 파라미터 검사 시 문자열 검사를 피하기 위한 읽기 전용 변수
    private readonly int hashSpeedParam = Animator.StringToHash("Speed");

    // 프로퍼티
    public Camera MainCamera => mainCamera;
    public Animator Animator => animator;
    public VirtualPad VirtualPad => virtualPad;
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;

    public CharacterController CharacterController {  get; private set; }
    public NavMeshAgent Agent {  get; private set; }

    public AdventurePlayerControlState PlayerControlState => playerControlState;
    public AdventurePlayerAutoState PlayerAutoState => playerAutoState;


    private void Awake()
    {
        CharacterController = GetComponent<CharacterController>();
        Agent = GetComponent<NavMeshAgent>();

        playerControlState.Initialize(this);
        playerAutoState.Initialize(this);

        ChangeState(playerControlState);
    }

    private void Start()
    {
        if (PathManager.Instance != null)
        {
            PathManager.Instance.Initialize(this.transform);
        }
    }
    private void Update()
    {
        if (currentState != null)
        {
            currentState.OnUpdate();
        }
    }

    
    // 상태 전환 함수
    public void ChangeState(AdventurePlayerState newState)
    {
        if (currentState == newState) return;

        if (currentState != null)
        {
            currentState.OnExit();
        }

        currentState = newState;
        currentState.OnEnter();
    }
    // 애니메이터 파라미터 수정 함수
    public void SetAnimatorSpeed(float speed)
    {
        animator.SetFloat(hashSpeedParam, speed);
    }
}