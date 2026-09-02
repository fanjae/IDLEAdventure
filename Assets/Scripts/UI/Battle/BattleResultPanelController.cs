using UnityEngine;
using UnityEngine.SceneManagement;

// 전투 결과 UI 관리
public sealed class BattleResultPanelController : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject stagePanel;

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
        // 자동전투 승리 시에는 결과창을 표시하지 않음
        if (StageRuntimeData.IsAutoBattle && winner == UnitTeam.Hero)
        {
            return;
        }

        if (stagePanel != null)
        {
            stagePanel.SetActive(false);
        }

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
        if (StageRuntimeData.IsFieldEnemyBattle)
        {
            ReturnToField();
            return;
        }

        SceneManager.LoadScene(mainSceneName);
    }

    // 다음 스테이지 도전
    public void ChallengeNextStage()
    {
        // 2026.08.31 필드 적 전투는 StageData를 공유하지만
        // 다음 스테이지로 진행하지 않고 필드로 복귀한다.
        if (StageRuntimeData.IsFieldEnemyBattle)
        {
            ReturnToField();
            return;
        }

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

    private void ReturnToField()
    {
        // 2026.09.02 필드 복귀 시 사용이 끝난 필드 적 런타임 정보 초기화
        FieldEnemyRuntimeData.ClearEnemyData();

        StageRuntimeData.StopAutoBattle();
        StageRuntimeData.StopFieldEnemyBattle();

        SceneManager.LoadScene(mainSceneName);
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