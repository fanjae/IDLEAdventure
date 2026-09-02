using System;
using UnityEngine;

public sealed class StageManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private StageEnemySpawner stageEnemySpawner;

    [SerializeField] private StageFieldLoader stageFieldLoader;
    [SerializeField] private BattleManager battleManager;

    private StageProgressController stageProgressController;

    private int currentStageId;

    public int CurrentStageId => currentStageId;

    private void Awake()
    {
        if (stageEnemySpawner == null)
        {
            throw new Exception("StageManager의 StageEnemySpawner가 없습니다.");
        }

        if (stageFieldLoader == null)
        {
            throw new Exception("StageManager의 StageFieldLoader가 없습니다.");
        }

        if (battleManager == null)
        {
            throw new Exception("StageManager의 BattleManager가 없습니다.");
        }

        stageProgressController = new StageProgressController();
    }

    private void Start()
    {
        if (StageRuntimeData.SelectedStageId < 1)
        {
            Debug.LogError("선택된 스테이지가 없습니다.");

            return;
        }

        currentStageId = StageRuntimeData.SelectedStageId;

        Debug.Log($"전투 씬으로 전달된 StageId: {currentStageId}");

        PrepareCurrentStage();
    }

    private void PrepareCurrentStage()
    {
        if (StageDatabase.Instance == null)
        {
            Debug.LogError("StageDatabase가 없습니다.");

            return;
        }

        StageData stage = StageDatabase.Instance.GetStage(currentStageId);

        if (stage == null)
        {
            Debug.LogError($"{currentStageId}번 스테이지 데이터를 찾을 수 없습니다.");

            return;
        }

        stageFieldLoader.LoadField(stage.fieldName);

        stageEnemySpawner.LoadStage(stage);

        Debug.Log($"{currentStageId}번 스테이지 준비 완료");
    }

    private void OnEnable()
    {
        battleManager.OnBattleEnded += HandleBattleEnded;
    }

    private void OnDisable()
    {
        battleManager.OnBattleEnded -= HandleBattleEnded;
    }

    // 전투 결과에 따라 스테이지 클리어 또는 패배 이력을 저장함
    private void HandleBattleEnded(UnitTeam winner)
    {
        // 2026.08.31 필드 적은 StageData만 공유하며
        // 실제 스테이지 클리어 및 다음 스테이지 진행에는 포함하지 않는다.
        if (StageRuntimeData.IsFieldEnemyBattle)
        {
            // 2026.09.02 필드 적 전투 승리 시 처치 상태를 저장 데이터에 반영
            if (winner == UnitTeam.Hero && FieldEnemyRuntimeData.HasEnemyData)
            {
                SaveManager.Instance.CurrentData.FieldObjects ??= new FieldObjectSaveData();

                int defeatedEnemyId = FieldEnemyRuntimeData.InteractedFieldEnemyId;

                if (!SaveManager.Instance.CurrentData.FieldObjects.DefeatedEnemyIds.Contains(defeatedEnemyId))
                {
                    SaveManager.Instance.CurrentData.FieldObjects.DefeatedEnemyIds.Add(defeatedEnemyId);
                }
            }

            SaveManager.Instance.Save();
            return;
        }

        if (winner == UnitTeam.Hero)
        {
            stageProgressController.CompleteStage(currentStageId);
            AchievementManager.Instance.RecordStageCleared(currentStageId);
            SaveManager.Instance.Save();
            return;
        }

        stageProgressController.RecordStageDefeat(currentStageId);
        SaveManager.Instance.Save();
    }
}
