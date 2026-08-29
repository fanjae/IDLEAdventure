using UnityEngine;

// 메인 화면 모험 패널 UI 관리
public sealed class MainAdventurePanelController : MonoBehaviour
{
    [SerializeField] private GameObject mainBottomPanel;
    [SerializeField] private GameObject adventurePanel;

    [Header("패널 연출")]
    [SerializeField] private UIPanelTransition panelTransition;

    // 모험 패널 표시
    public void OpenAdventurePanel()
    {
        if (adventurePanel == null)
        {
            return;
        }

        adventurePanel.SetActive(true);
        panelTransition?.PlayOpen();
    }

    // 모험 패널 숨김
    public void CloseAdventurePanel()
    {
        if (adventurePanel == null)
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

    // 모험 패널 종료 처리
    private void ClosePanel()
    {
        adventurePanel.SetActive(false);

        if (mainBottomPanel != null)
        {
            mainBottomPanel.SetActive(true);
        }
    }
}