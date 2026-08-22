using UnityEngine;
using UnityEngine.UI;

// 메인 화면의 업적 진입 버튼을 패널에 연결함
[RequireComponent(typeof(Button))]
public sealed class AchievementPanelButton : MonoBehaviour
{
    [SerializeField] private AchievementPanelPresenter panelPresenter;
    [SerializeField] private GameObject notificationRoot;
    private AchievementController subscribedController;

    // 업적 진행도 변경을 알림 표시와 연결함
    private void OnEnable()
    {
        SubscribeToAchievementEvents();
        RefreshNotification();
    }

    // 비활성화 시 업적 이벤트 구독 해제함
    private void OnDisable()
    {
        UnsubscribeFromAchievementEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromAchievementEvents();
    }

    // 버튼 Persistent OnClick에서 호출함
    public void OpenPanel()
    {
        if (panelPresenter == null)
        {
            Debug.LogError("AchievementPanelPresenter 참조 없음", this);
            return;
        }

        panelPresenter.Open();
        RefreshNotification();
    }

    // 현재 업적 컨트롤러의 변경 알림을 구독함
    private void SubscribeToAchievementEvents()
    {
        if (subscribedController != null || AchievementManager.Instance == null || !AchievementManager.Instance.IsInitialized)
        {
            return;
        }

        subscribedController = AchievementManager.Instance.Controller;
        subscribedController.OnMetricChanged += HandleAchievementChanged;
        subscribedController.OnAchievementClaimed += HandleAchievementClaimed;
    }

    // 업적 컨트롤러 변경 알림 구독 해제함
    private void UnsubscribeFromAchievementEvents()
    {
        if (subscribedController == null)
        {
            return;
        }

        subscribedController.OnMetricChanged -= HandleAchievementChanged;
        subscribedController.OnAchievementClaimed -= HandleAchievementClaimed;
        subscribedController = null;
    }

    // 진행도 변경 시 수령 가능 알림 갱신함
    private void HandleAchievementChanged(AchievementMetric _, int __)
    {
        RefreshNotification();
    }

    // 보상 수령 시 수령 가능 알림 갱신함
    private void HandleAchievementClaimed(string _)
    {
        RefreshNotification();
    }

    // 수령 가능한 보상이 있으면 진입 버튼 알림을 표시함
    private void RefreshNotification()
    {
        if (notificationRoot != null)
        {
            notificationRoot.SetActive(AchievementManager.Instance != null &&
                                       AchievementManager.Instance.IsInitialized &&
                                       AchievementManager.Instance.HasClaimableRewards);
        }
    }
}
