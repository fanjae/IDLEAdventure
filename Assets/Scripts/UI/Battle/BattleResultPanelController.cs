using UnityEngine;
using UnityEngine.SceneManagement;

// 전투 결과 UI 관리
public sealed class BattleResultPanelController : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("씬 이동")]
    [SerializeField] private string mainSceneName = "ItemandSaveTestMainScene";
    [SerializeField] private string battleSceneName = "NewUIBattleScene";

    private void Start()
    {
        HideResult();

        if (BattleManager.Instance == null)
        {
            Debug.LogError("BattleManager가 없습니다.");
            return;
        }

        BattleManager.Instance.OnBattleEnded += HandleBattleEnded;
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleEnded -= HandleBattleEnded;
        }
    }

    // 전투 결과에 따라 결과 패널 표시
    private void HandleBattleEnded(UnitTeam winner)
    {
        if (winPanel != null)
        {
            winPanel.SetActive(winner == UnitTeam.Hero);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(winner == UnitTeam.Enemy);
        }
    }

    // 메인 화면으로 이동
    public void ReturnToMain()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    // 다음 스테이지 도전
    public void ChallengeNextStage()
    {
        if (StageDatabase.Instance == null)
        {
            Debug.LogError("StageDatabase가 없습니다.");
            return;
        }

        int currentStageId = StageRuntimeData.SelectedStageId;
        int nextStageId = currentStageId + 1;

        if (!StageDatabase.Instance.TryGetStage(nextStageId, out _))
        {
            ReturnToMain();
            return;
        }

        StageRuntimeData.SelectStage(nextStageId);
        SceneManager.LoadScene(battleSceneName);
    }

    // 전투 결과 패널 숨김
    private void HideResult()
    {
        winPanel?.SetActive(false);
        losePanel?.SetActive(false);
    }

    // 현재 스테이지 재도전
    public void RetryCurrentStage()
    {
        int currentStageId = StageRuntimeData.SelectedStageId;

        if (currentStageId < 0)
        {
            Debug.LogError("선택된 스테이지가 없습니다.");
            ReturnToMain();
            return;
        }

        if (StageDatabase.Instance == null || !StageDatabase.Instance.TryGetStage(currentStageId, out _))
        {
            Debug.LogError($"{currentStageId}번 스테이지 데이터가 없습니다.");
            ReturnToMain();
            return;
        }

        SceneManager.LoadScene(battleSceneName);
    }
}