using UnityEngine;
using UnityEngine.SceneManagement;

// 현재 진행 스테이지 전투 진입 UI 관리
public sealed class StageChallengeController : MonoBehaviour
{
    [SerializeField] private string battleSceneName = "NewUIBattleScene";

    private readonly StageProgressController stageProgressController = new();

    // 현재 진행 중인 스테이지 전투 진입
    public void ChallengeCurrentStage()
    {
        int currentStageId = stageProgressController.CurrentStageId;

        StageRuntimeData.SelectStage(currentStageId);

        Debug.Log($"스테이지 전투 진입: {currentStageId}");

        SceneManager.LoadScene(battleSceneName);
    }
}
