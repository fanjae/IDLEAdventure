using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 전투 배치 패널 UI 관리
public sealed class FormationPanelController : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "Filed_Persistent";

    [Header("전투 배치")]
    [SerializeField] private FormationManager formationManager;
    [SerializeField] private GameObject warningPanel;

    private Coroutine warningPanelCoroutine;

    // 전투 시작 가능 여부 확인
    public bool CanStartBattle()
    {
        if (formationManager == null)
        {
            Debug.LogError("FormationManager가 연결되지 않음");
            return false;
        }

        if (formationManager.HasPlacedHero)
        {
            return true;
        }

        if (warningPanel != null)
        {
            ShowWarningPanel();
        }

        Debug.LogWarning("배치된 영웅이 없음");
        return false;
    }

    // 일반 전투 시작
    public void StartBattle()
    {
        if (!CanStartBattle())
        {
            return;
        }

        formationManager.StartBattle();
        CloseFormationPanel();
    }

    // 전투 시작 시 배치 패널 닫음
    public void CloseFormationPanel()
    {
        gameObject.SetActive(false);
    }

    // 경고창 닫음
    public void CloseWarningPanel()
    {
        if (warningPanelCoroutine != null)
        {
            StopCoroutine(warningPanelCoroutine);
            warningPanelCoroutine = null;
        }

        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }

    // 메인 화면으로 이동
    public void ReturnToMain()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    // 경고창 표시
    private void ShowWarningPanel()
    {
        if (warningPanel == null)
        {
            return;
        }

        warningPanel.SetActive(true);

        if (warningPanelCoroutine != null)
        {
            StopCoroutine(warningPanelCoroutine);
        }

        warningPanelCoroutine = StartCoroutine(HideWarningPanelAfterDelay());
    }

    // 일정 시간 후 경고창 닫음
    private IEnumerator HideWarningPanelAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        CloseWarningPanel();
        warningPanelCoroutine = null;
    }
}