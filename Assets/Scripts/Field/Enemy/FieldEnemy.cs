using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFieldAI : MonoBehaviour
{
    private enum EnemyState
    {
        Idle,
        Wander,
        Chase,
        Return
    }
    //
    private static readonly int MoveHash = Animator.StringToHash("Move");
    //

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 6f;
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 3f;

    [Header("Chase")]
    [SerializeField] private float detectDistance = 8f;
    [SerializeField] private float loseDistance = 12f;
    [SerializeField] private float maxChaseDistance = 15f;
    [SerializeField] private float encounterDistance = 1.2f;

    [Header("Enemy Type")]
    [SerializeField] private bool isWorm;

    //
    private Animator animator;
    //



    private NavMeshAgent agent;
    private Transform player;
    private Vector3 homePosition;
    private EnemyState state;
    private float idleTimer;

    private void Awake()
    {
        //
        agent = GetComponent<NavMeshAgent>();

        animator = GetComponent<Animator>();
        //
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("Player");
            return;
        }

        player = playerObject.transform;
        homePosition = transform.position;

        StartIdle();
    }

    private void Update()
    {
        if (isWorm)
        {
            if (IsPlayerInRange(encounterDistance))
            {
                StartBattle();
                return;
            }
        }
        else
        {
            if (state != EnemyState.Chase && state != EnemyState.Return && IsPlayerInRange(detectDistance)) StartChase();
        }
        switch (state)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;

            case EnemyState.Wander:
                UpdateWander();
                break;

            case EnemyState.Chase:
                UpdateChase();
                break;

            case EnemyState.Return:
                UpdateReturn();
                break;
        }
    }

    private void StartIdle()
    {
        state = EnemyState.Idle;
        agent.ResetPath();
        idleTimer = Random.Range(minIdleTime, maxIdleTime);

        //
        SetMoving(false);
        //
    }

    private void UpdateIdle()
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f) StartWander();
    }

    private void StartWander()
    {
        Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
        Vector3 randomPosition = homePosition + new Vector3(randomPoint.x, 0f, randomPoint.y);

        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 2f, agent.areaMask))
        {
            state = EnemyState.Wander;
            agent.stoppingDistance = 0.1f;
            agent.SetDestination(hit.position);

            //
            SetMoving(true);
            //
        }
        else
        {
            StartIdle();
        }
    }

    private void UpdateWander()
    {
        if (agent.pathPending) return;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance) StartIdle();
    }

    private void StartChase()
    {
        state = EnemyState.Chase;
        agent.stoppingDistance = encounterDistance;
        agent.SetDestination(player.position);

        //
        SetMoving(true);
        //
    }

    private void UpdateChase()
    {
        float playerDistance = Vector3.Distance(transform.position, player.position);
        float homeDistance = Vector3.Distance(transform.position, homePosition);

        if (playerDistance <= encounterDistance)
        {
            StartBattle();
            return;
        }

        if (playerDistance > loseDistance || homeDistance > maxChaseDistance)
        {
            StartReturn();
            return;
        }

        agent.SetDestination(player.position);
    }

    private void StartReturn()
    {
        state = EnemyState.Return;
        agent.stoppingDistance = 0.1f;
        agent.SetDestination(homePosition);

        //
        SetMoving(true);
        //
    }

    private void UpdateReturn()
    {
        if (agent.pathPending) return;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance) StartIdle();
    }

    private bool IsPlayerInRange(float distance)
    {
        return Vector3.Distance(transform.position, player.position) <= distance;
    }

    //
    private void SetMoving(bool isMoving)
    {
        if (animator == null) return;

        animator.SetBool(MoveHash, isMoving);
    }
    //

    private void StartBattle()
    {
        agent.isStopped = true;
        enabled = false;
        //
        SetMoving(false);
        //

        //씬 이동 추가
    }
    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? homePosition : transform.position;

        Gizmos.DrawWireSphere(center, wanderRadius);
        Gizmos.DrawWireSphere(transform.position, detectDistance);
        Gizmos.DrawWireSphere(center, maxChaseDistance);
    }
}