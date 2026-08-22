using UnityEngine;
using UnityEngine.UI;

// 스테이지 진행 패널 UI 관리
public sealed class StageProgressPanelController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform nodeContainer;
    [SerializeField] private RectTransform progressLine;
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
        UpdateProgressLayout();
        FocusCurrentStage(currentStage);
    }

    // 스테이지 개수와 현재 진행 상태에 맞춰 노드 생성
    private void CreateStageNodes(int stageCount, int currentStage)
    {
        for (int index = 0; index < stageCount; index++)
        {
            int stageNumber = index + 1;
            bool isCurrent = stageNumber == currentStage;
            bool isCleared = stageNumber < currentStage;

            StageNodeView nodeView = Instantiate(stageNodePrefab, nodeContainer);
            nodeView.Bind(stageNumber, isCurrent, isCleared);
        }
    }

    // 현재 스테이지가 화면 중앙에 오도록 스크롤 위치 갱신
    private void FocusCurrentStage(int currentStage)
    {
        Canvas.ForceUpdateCanvases();

        if (nodeContainer.childCount == 0)
        {
            return;
        }

        int currentIndex = Mathf.Clamp(currentStage - 1, 0, nodeContainer.childCount - 1);
        RectTransform currentNode = nodeContainer.GetChild(currentIndex) as RectTransform;

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

        float nodeCenterX = currentNode.anchoredPosition.x + (0.5f - currentNode.pivot.x) * currentNode.rect.width;
        float targetOffset = nodeCenterX - viewportWidth * 0.5f;
        float scrollableWidth = contentWidth - viewportWidth;
        float normalizedPosition = targetOffset / scrollableWidth;

        scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(normalizedPosition);
    }

    // 생성된 스테이지 노드를 기준으로 진행 UI 크기 갱신
    private void UpdateProgressLayout()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(nodeContainer);

        if (nodeContainer.childCount == 0)
        {
            return;
        }

        float nodeContainerWidth = nodeContainer.rect.width;

        // 스크롤 영역을 생성된 노드 전체 너비에 맞춤
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, nodeContainerWidth);

        if (nodeContainer.childCount == 1)
        {
            progressLine.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
            return;
        }

        RectTransform firstNode = nodeContainer.GetChild(0) as RectTransform;
        RectTransform lastNode = nodeContainer.GetChild(nodeContainer.childCount - 1) as RectTransform;

        if (firstNode == null || lastNode == null)
        {
            return;
        }

        // 첫 번째 노드의 왼쪽 끝부터 마지막 노드의 오른쪽 끝까지 진행선 표시
        float firstNodeLeftX = firstNode.anchoredPosition.x - firstNode.rect.width * firstNode.pivot.x;
        float lastNodeRightX = lastNode.anchoredPosition.x + lastNode.rect.width * (1f - lastNode.pivot.x);

        progressLine.anchoredPosition = new Vector2(firstNodeLeftX, progressLine.anchoredPosition.y);
        progressLine.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, lastNodeRightX - firstNodeLeftX);

        // Content 크기 변경 후 ScrollRect 내부 Bounds 갱신
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();
    }
}