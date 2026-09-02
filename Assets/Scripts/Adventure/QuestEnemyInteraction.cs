using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestEnemyInteraction : QuestInteractableObject
{
    [Header("Field Setting")]
    [SerializeField] private FieldType fieldType;
    [SerializeField] private string battleSceneName = "NewUIBattleScene";

    protected override InteractType GetInteractType() => InteractType.Enemy;

    protected override void OnInteract()
    {
        if (isInteracting) return;
        isInteracting = true;

        int setStageId = GetStageId(fieldType);

        StageRuntimeData.SelectStage(setStageId);
        StageRuntimeData.StartFieldEnemyBattle();

        QuestEnemyRuntimeData.SetQuestEnemyData(questId);

        if (FieldPlayerPositionController.Current != null && SaveManager.TryGetExistingInstance(out SaveManager saveManager) && saveManager.CurrentData != null)
        {
            FieldPlayerPositionController.Current.WriteSaveData(saveManager.CurrentData);
        }

        Debug.Log($"퀘스트 몬스터 전투 진입! 스테이지: {setStageId}, 퀘스트 ID: {questId}");
        SceneManager.LoadScene(battleSceneName);
    }

    private int GetStageId(FieldType fieldType)
    {
        if (fieldType == FieldType.Forest) return 1;
        if (fieldType == FieldType.Desert) return 6;
        if (fieldType == FieldType.Snow) return 11;
        return 0;
    }
}
