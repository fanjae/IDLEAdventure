using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 현재 진행 스테이지 전투 진입 UI 관리
public sealed class StageChallengeController : MonoBehaviour
{
    [SerializeField] private string battleSceneName = "NewUIBattleScene";
    [SerializeField] private Button challengeButton;

    private readonly StageProgressController stageProgressController = new();

    private void OnEnable()
    {
        RefreshChallengeButton();
    }

    // 현재 진행 상태에 따라 도전 버튼 활성화 여부 갱신
    private void RefreshChallengeButton()
    {
        if (challengeButton == null)
        {
            Debug.LogWarning("StageChallengeController의 Challenge Button이 연결되지 않았습니다.", this);
            return;
        }

        int currentStageId = stageProgressController.CurrentStageId;
        int highestClearedStageId = stageProgressController.HighestClearedStageId;

        bool isFinalStageCleared = highestClearedStageId >= StageProgressController.MaxPlayableStageId;
        bool isPlayableStage = currentStageId >= 1 && currentStageId <= StageProgressController.MaxPlayableStageId;
        bool hasStageData = StageDatabase.Instance != null && StageDatabase.Instance.TryGetStage(currentStageId, out _);

        challengeButton.interactable = !isFinalStageCleared && isPlayableStage && hasStageData;
    }

    // 현재 진행 중인 스테이지 전투 진입
    public void ChallengeCurrentStage()
    {
        int currentStageId = stageProgressController.CurrentStageId;
        int highestClearedStageId = stageProgressController.HighestClearedStageId;

        if (highestClearedStageId >= StageProgressController.MaxPlayableStageId)
        {
            Debug.Log("최종 스테이지를 이미 클리어하여 더 이상 도전할 수 없습니다.");
            return;
        }

        if (currentStageId < 1 || currentStageId > StageProgressController.MaxPlayableStageId)
        {
            Debug.LogWarning($"도전할 수 없는 스테이지입니다. StageId: {currentStageId}");
            return;
        }

        if (StageDatabase.Instance == null || !StageDatabase.Instance.TryGetStage(currentStageId, out _))
        {
            Debug.LogError($"{currentStageId}번 스테이지 데이터가 없습니다.");
            return;
        }

        StageRuntimeData.SelectStage(currentStageId);

        // 전투 씬 이동 전에 현재 필드 플레이어 위치 저장
        if (FieldPlayerPositionController.Current != null && SaveManager.TryGetExistingInstance(out SaveManager saveManager) && saveManager.CurrentData != null)
        {
            FieldPlayerPositionController.Current.WriteSaveData(saveManager.CurrentData);
        }

        Debug.Log($"스테이지 전투 진입: {currentStageId}");

        SceneManager.LoadScene(battleSceneName);
    }
}