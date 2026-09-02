using UnityEngine;
using UnityEngine.SceneManagement;

public enum FieldType
{
    None,
    Forest, Desert, Snow,
    Length
}

[RequireComponent(typeof(Collider))]
public class FieldEnemyInteraction : MonoBehaviour
{
    [Header("Enemy Info")]
    [SerializeField] private int enemyId;

    [Header("Field Setting")]
    [SerializeField] private FieldType fieldType;
    [SerializeField] private string battleSceneName = "NewUIBattleScene";

    private bool isBattle = false;

    // 2026.09.02 저장된 필드 적 처치 이력이 있으면 필드에서 제거
    private void Start()
    {
        if (!SaveManager.TryGetExistingInstance(out SaveManager saveManager) || saveManager.CurrentData == null)
        {
            return;
        }

        saveManager.CurrentData.FieldObjects ??= new FieldObjectSaveData();

        if (saveManager.CurrentData.FieldObjects.DefeatedEnemyIds.Contains(enemyId))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isBattle) return;

        if (other.CompareTag("Player"))
        {
            InteractionUIManager.Instance.SetInteractable(true, InteractType.Enemy, OnInteracEnemy);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (isBattle) return;

        if (other.CompareTag("Player"))
        {
            InteractionUIManager.Instance.SetInteractable(false, InteractType.Enemy);
        }
    }

    private void OnInteracEnemy()
    {
        if (isBattle) return;
        isBattle = true;

        int setStageId = GetStageId(fieldType);
        
        // 스테이지 진입 방식 확인 필요.
        StageRuntimeData.SelectStage(setStageId);

        // 2026.08.31 필드 적 상호작용으로 진입한 전투임을 기록
        StageRuntimeData.StartFieldEnemyBattle();

        FieldEnemyRuntimeData.SetEnemyData(enemyId);

        // 2026.09.01 전투 씬 이동 전에 현재 필드 플레이어 위치 저장
        if (FieldPlayerPositionController.Current != null && SaveManager.TryGetExistingInstance(out SaveManager saveManager) && saveManager.CurrentData != null)
        {
            FieldPlayerPositionController.Current.WriteSaveData(saveManager.CurrentData);
        }

        Debug.Log($"스테이지 전투 진입: {setStageId}");

        SceneManager.LoadScene(battleSceneName);
    }

    private int GetStageId(FieldType fieldType)
    {
        int fieldId = 0;
        if (fieldType == FieldType.Forest) fieldId = 1;
        else if (fieldType == FieldType.Desert) fieldId = 6;
        else if (fieldType == FieldType.Snow) fieldId = 11;

        return fieldId;
    }
}
