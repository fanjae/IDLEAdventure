using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 퀘스트 목적지에 가는 길 표시를 담당해줄 매니저 클래스.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class PathManager : LocalSingleton<PathManager>
{
    [Header("Setting")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float offset = 0.2f;
    [SerializeField] private float flowSpeed = 2.0f;
    [SerializeField] private float pathUpdateDelay = 0.1f;

    private Transform player;

    private Vector3 targetPosition;
    private bool isShowning = false;
    private NavMeshPath navMeshPath;
    private Material line;

    private readonly string path = "GameData/Quests/Materials/QuestPathLine";

    private Coroutine pathUpdateCoroutine;

    protected override void Awake()
    {
        base.Awake();

        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        navMeshPath = new NavMeshPath();

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.textureMode = LineTextureMode.Tile;

        Material lineMaterial = Resources.Load<Material>(path);

        if (lineMaterial != null)
        {
            line = new Material(lineMaterial);
            lineRenderer.material = line;
        }
        else
        {
            line = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.material = line;
        }

        HideLine();
    }

    private void Update()
    {
        if (!isShowning || line == null) return;

        float lineOffset = Time.time * flowSpeed;
        line.mainTextureOffset = new Vector2(-lineOffset, 0.0f);
    }

    public void Initialize(Transform player)
    {
        this.player = player;
    }

    // 패스 갱신 코루틴.
    // 패스 경로 체크는 매 프레임 할 필요는 없을 것 같기에 코루틴으로 구현.
    private IEnumerator UpdatePathCo()
    {
        WaitForSeconds delay = new WaitForSeconds(pathUpdateDelay);

        while (isShowning && player != null)
        {
            // 경로 계산
            if (NavMesh.CalculatePath(player.position, targetPosition, NavMesh.AllAreas, navMeshPath))
            {
                // 경로의 코너 점 확인
                // 코너가 존재한다면, 그에 맞게 선 그리기
                if (navMeshPath.corners.Length > 1)
                {
                    lineRenderer.positionCount = navMeshPath.corners.Length;

                    for (int i = 0; i < navMeshPath.corners.Length; i++)
                    {
                        Vector3 cornerPosition = navMeshPath.corners[i];
                        cornerPosition.y += offset;
                        lineRenderer.SetPosition(i, cornerPosition);
                    }
                }
                else
                {
                    lineRenderer.positionCount = 0;
                }
            }
            yield return delay;
        }
    }
    // 경로 선 활성화 함수.
    public void ShowLine(Vector3 dest)
    {
        targetPosition = dest;
        isShowning = true;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }

        if (pathUpdateCoroutine != null)
        {
            StopCoroutine(pathUpdateCoroutine);
        }
        pathUpdateCoroutine = StartCoroutine(UpdatePathCo());
    }
    // 경로 선 비활성화 함수.
    public void HideLine()
    {
        isShowning = false;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 0;
        }

        if (pathUpdateCoroutine!= null)
        {
            StopCoroutine(pathUpdateCoroutine);
            pathUpdateCoroutine = null;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (line != null)
        {
            Destroy(line);
        }
    }
}