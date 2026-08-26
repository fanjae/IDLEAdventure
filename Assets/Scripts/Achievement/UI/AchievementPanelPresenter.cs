using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// 업적 목록 생성, 갱신, 열기/닫기를 담당하는 기본 패널임
public sealed class AchievementPanelPresenter : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private AchievementDatabaseSO database;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private AchievementRowView rowTemplate;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button claimAllButton;
    [SerializeField] private AchievementRewardToast rewardToast;
    [Header("하단 분류 탭")]
    [SerializeField] private Button partyGrowthButton;
    [SerializeField] private Button gachaButton;
    [SerializeField] private Button stageProgressButton;
    [SerializeField] private GameObject partyGrowthNotification;
    [SerializeField] private GameObject gachaNotification;
    [SerializeField] private GameObject stageProgressNotification;

    private readonly List<AchievementRowView> rows = new();
    private AchievementController subscribedController;
    private AchievementCategory selectedCategory = AchievementCategory.PartyGrowth;

    // 탭 버튼 이벤트를 한 번만 연결함
    private void Awake()
    {
        partyGrowthButton?.onClick.AddListener(OpenPartyGrowth);
        gachaButton?.onClick.AddListener(OpenGacha);
        stageProgressButton?.onClick.AddListener(OpenStageProgress);
    }

    // 패널 활성화 상태에서는 업적 진행도 변경을 화면에 반영함
    private void OnEnable()
    {
        SubscribeToAchievementEvents();
    }

    // 패널 비활성화 시 업적 이벤트 구독 해제함
    private void OnDisable()
    {
        UnsubscribeFromAchievementEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromAchievementEvents();
    }

    public void Open()
    {
        selectedCategory = AchievementCategory.PartyGrowth;
        panelRoot.SetActive(true);

        // 스크롤 뷰보다 닫기 버튼을 앞으로 올림
        if (closeButton != null)
        {
            closeButton.transform.SetAsLastSibling();
        }

        SubscribeToAchievementEvents();
        Rebuild();
    }

    // 모험단 성장 분류를 열고 목록을 다시 만듦
    public void OpenPartyGrowth()
    {
        SelectCategory(AchievementCategory.PartyGrowth);
    }

    // 가챠 분류를 열고 목록을 다시 만듦
    public void OpenGacha()
    {
        SelectCategory(AchievementCategory.Gacha);
    }

    // 스테이지 진행 분류를 열고 목록을 다시 만듦
    public void OpenStageProgress()
    {
        SelectCategory(AchievementCategory.StageProgress);
    }

    public void Close()
    {
        UnsubscribeFromAchievementEvents();
        panelRoot.SetActive(false);
    }

    public void Refresh()
    {
        foreach (AchievementRowView row in rows)
        {
            row.Refresh();
        }

        RefreshClaimAllButton();
        RefreshCategoryNotifications();
    }

    private void Rebuild()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i] == null)
            {
                continue;
            }

            // 이전 행은 즉시 레이아웃 계산에서 제외함
            rows[i].gameObject.SetActive(false);
            Destroy(rows[i].gameObject);
        }

        rows.Clear();
        RefreshCategoryButtonVisuals();
        RefreshCategoryNotifications();
        if (database == null || !AchievementManager.Instance.IsInitialized)
        {
            return;
        }

        AchievementController controller = AchievementManager.Instance.Controller;
        foreach (AchievementDefinitionSO definition in database.Definitions
            .Where(item => item != null && item.Category == selectedCategory)
            .OrderByDescending(item => item.HasReward && controller.CanClaim(item))
            .ThenBy(item => controller.GetProgress(item).IsClaimed)
            .ThenBy(item => item.DisplayOrder))
        {
            AchievementRowView row = Instantiate(rowTemplate, contentRoot);
            row.gameObject.SetActive(true);
            row.Bind(definition, this);
            rows.Add(row);
        }

        RefreshContentLayout();
        RefreshClaimAllButton();
    }

    // 새로 생성한 행의 위치와 스크롤 높이를 즉시 갱신함
    private void RefreshContentLayout()
    {
        RectTransform contentRect = contentRoot as RectTransform;
        if (contentRect == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    // 수령 가능한 보상을 모두 지급하고 목록 상태 갱신함
    // 버튼 Persistent OnClick에서 호출함
    public void ClaimAll()
    {
        if (AchievementManager.Instance.TryClaimAll(selectedCategory, out List<AchievementClaimReward> rewards) > 0)
        {
            rewardToast?.Show(rewards);
            Refresh();
        }
    }

    // 단일 수령 보상을 토스트로 표시함
    public void ShowRewardToast(AchievementClaimReward reward)
    {
        rewardToast?.Show(new[] { reward });
    }

    // 수령 가능한 업적이 있을 때만 일괄 수령 버튼을 활성화함
    private void RefreshClaimAllButton()
    {
        if (claimAllButton != null)
        {
            claimAllButton.interactable = AchievementManager.Instance != null &&
                                          AchievementManager.Instance.IsInitialized &&
                                          AchievementManager.Instance.HasClaimableRewardsInCategory(selectedCategory);
        }
    }

    // 선택 분류를 바꾸고 목록 및 탭 강조 상태를 갱신함
    private void SelectCategory(AchievementCategory category)
    {
        selectedCategory = category;
        Rebuild();
    }

    // 선택 탭은 선명하게, 나머지 탭은 반투명하게 표시함
    private void RefreshCategoryButtonVisuals()
    {
        RefreshCategoryButtonVisual(partyGrowthButton, AchievementCategory.PartyGrowth);
        RefreshCategoryButtonVisual(gachaButton, AchievementCategory.Gacha);
        RefreshCategoryButtonVisual(stageProgressButton, AchievementCategory.StageProgress);
    }

    private void RefreshCategoryButtonVisual(Button button, AchievementCategory category)
    {
        if (button == null || button.image == null)
        {
            return;
        }

        button.image.color = category == selectedCategory ? Color.white : new Color(1f, 1f, 1f, 0.55f);
    }

    // 각 분류에 수령 가능한 보상이 있을 때만 해당 탭 알림을 표시함
    private void RefreshCategoryNotifications()
    {
        SetCategoryNotification(partyGrowthNotification, AchievementCategory.PartyGrowth);
        SetCategoryNotification(gachaNotification, AchievementCategory.Gacha);
        SetCategoryNotification(stageProgressNotification, AchievementCategory.StageProgress);
    }

    private void SetCategoryNotification(GameObject notification, AchievementCategory category)
    {
        if (notification == null)
        {
            return;
        }

        notification.SetActive(AchievementManager.Instance != null &&
                               AchievementManager.Instance.IsInitialized &&
                               AchievementManager.Instance.HasClaimableRewardsInCategory(category));
    }

    // 현재 업적 컨트롤러의 진행도와 수령 알림을 구독함
    private void SubscribeToAchievementEvents()
    {
        if (subscribedController != null || AchievementManager.Instance == null || !AchievementManager.Instance.IsInitialized)
        {
            return;
        }

        subscribedController = AchievementManager.Instance.Controller;
        subscribedController.OnMetricChanged += HandleMetricChanged;
        subscribedController.OnAchievementClaimed += HandleAchievementClaimed;
    }

    // 업적 컨트롤러 알림 구독을 안전하게 해제함
    private void UnsubscribeFromAchievementEvents()
    {
        if (subscribedController == null)
        {
            return;
        }

        subscribedController.OnMetricChanged -= HandleMetricChanged;
        subscribedController.OnAchievementClaimed -= HandleAchievementClaimed;
        subscribedController = null;
    }

    // 진행도 변경 시 열린 목록의 표시 값을 갱신함
    private void HandleMetricChanged(AchievementMetric _, int __)
    {
        if (panelRoot != null && panelRoot.activeInHierarchy)
        {
            Refresh();
        }
    }

    // 보상 수령 시 열린 목록의 수령 상태를 갱신함
    private void HandleAchievementClaimed(string _)
    {
        if (panelRoot != null && panelRoot.activeInHierarchy)
        {
            Refresh();
        }
    }
}
