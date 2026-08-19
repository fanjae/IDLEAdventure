using UnityEngine;
using UnityEngine.UI;

// 스테이지 진행 패널 UI 관리
public sealed class StageProgressPanelController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private StageNodeView stageNodePrefab;

    private readonly StageProgressController stageProgressController = new();

    private void Start()
    {
        InitializeStageProgress();
    }

    // 현재 스테이지 데이터를 기준으로 진행 UI 초기화
    // 현재 스테이지 데이터를 기준으로 진행 UI 초기화
    private void InitializeStageProgress()
    {
        if (StageDatabase.Instance == null || !StageDatabase.Instance.IsInitialized)
        {
            Debug.LogError("StageDatabase가 초기화되지 않았습니다.");
            return;
        }

        if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null)
        {
            Debug.LogError("SaveManager가 초기화되지 않았습니다.");
            return;
        }

        int stageCount = StageDatabase.Instance.StageCount;
        int currentStage = Mathf.Clamp(stageProgressController.CurrentStageId, 1, stageCount);

        CreateStageNodes(stageCount, currentStage);
        FocusCurrentStage(currentStage);
    }

    // 스테이지 개수와 현재 진행 상태에 맞춰 노드 생성
    private void CreateStageNodes(int stageCount, int currentStage)
    {
        for (int index = 0; index < stageCount; index++)
        {
            int stageNumber = index + 1;
            bool showRightLine = index < stageCount - 1;
            bool isCurrent = stageNumber == currentStage;
            bool isCleared = stageNumber < currentStage;

            StageNodeView nodeView = Instantiate(stageNodePrefab, content);
            nodeView.Bind(stageNumber, showRightLine, isCurrent, isCleared);
        }
    }

    // 현재 스테이지가 화면 중앙에 오도록 스크롤 위치 갱신
    private void FocusCurrentStage(int currentStage)
    {
        Canvas.ForceUpdateCanvases();

        if (content.childCount == 0)
        {
            return;
        }

        int currentIndex = Mathf.Clamp(currentStage - 1, 0, content.childCount - 1);
        RectTransform currentNode = content.GetChild(currentIndex) as RectTransform;

        if (currentNode == null)
        {
            return;
        }

        float contentWidth = content.rect.width;
        float viewportWidth = viewport.rect.width;

        if (contentWidth <= viewportWidth)
        {
            scrollRect.horizontalNormalizedPosition = 0f;
            return;
        }

        float nodeCenterX = currentNode.anchoredPosition.x + currentNode.rect.width * 0.5f;
        float targetOffset = nodeCenterX - viewportWidth * 0.5f;
        float scrollableWidth = contentWidth - viewportWidth;
        float normalizedPosition = targetOffset / scrollableWidth;

        scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(normalizedPosition);
    }
}