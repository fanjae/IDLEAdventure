using System;
using UnityEngine;
using UnityEngine.UI;

// 자동 스테이지 패널의 열기 및 닫기 처리
public sealed class AutoStagePanelController : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private UIPanelTransition panelTransition;

    public event Action OnClosed;

    private void OnEnable()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(HandleBackButtonClicked);
        }
    }

    private void OnDisable()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(HandleBackButtonClicked);
        }
    }

    // 자동 스테이지 패널 표시
    public void Open()
    {
        gameObject.SetActive(true);
        panelTransition?.PlayOpen();
    }

    // 자동 스테이지 패널 닫기
    private void HandleBackButtonClicked()
    {
        if (panelTransition == null)
        {
            ClosePanel();
            return;
        }

        panelTransition.PlayClose(ClosePanel);
    }

    // 자동 스테이지 패널 종료 처리
    private void ClosePanel()
    {
        gameObject.SetActive(false);
        OnClosed?.Invoke();
    }
}