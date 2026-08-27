using System;
using UnityEngine;
using UnityEngine.UI;

// 전체 메뉴 패널의 열기 및 닫기 처리
public sealed class AllPanelController : MonoBehaviour
{
    [Header("패널 이동")]
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject allPanelRoot;

    [Header("패널 연출")]
    [SerializeField] private UIPanelTransition panelTransition;

    public event Action OnClosed;

    private void OnEnable()
    {
        if (backButton != null) backButton.onClick.AddListener(HandleBackButtonClicked);
    }

    private void OnDisable()
    {
        if (backButton != null) backButton.onClick.RemoveListener(HandleBackButtonClicked);
    }

    // 전체 메뉴 패널 오픈 연출
    public void PlayOpenAnimation()
    {
        panelTransition?.PlayOpen();
    }

    // 뒤로가기 버튼 클릭 처리
    private void HandleBackButtonClicked()
    {
        if (allPanelRoot == null)
        {
            return;
        }

        if (panelTransition == null)
        {
            ClosePanel();
            return;
        }

        panelTransition.PlayClose(ClosePanel);
    }

    // 전체 메뉴 패널 종료 처리
    private void ClosePanel()
    {
        allPanelRoot.SetActive(false);
        OnClosed?.Invoke();
    }
}