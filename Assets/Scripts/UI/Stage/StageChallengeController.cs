using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// 현재 진행 스테이지 전투 진입 UI 관리
public sealed class StageChallengeController : MonoBehaviour
{
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private string battleSceneName = "NewUIBattleScene";

    private readonly StageProgressController stageProgressController = new();

    private void Start()
    {
        Refresh();
    }

    // 현재 진행 스테이지 정보 갱신
    private void Refresh()
    {
        int currentStageId = stageProgressController.CurrentStageId;
        stageText.text = $"스테이지 {currentStageId}";
    }

    // 현재 진행 중인 스테이지 전투 진입
    public void ChallengeCurrentStage()
    {
        int currentStageId = stageProgressController.CurrentStageId;

        StageRuntimeData.SelectStage(currentStageId);

        Debug.Log($"스테이지 전투 진입: {currentStageId}");

        SceneManager.LoadScene(battleSceneName);
    }
}