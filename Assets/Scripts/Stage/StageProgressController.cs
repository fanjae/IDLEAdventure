using System;
using UnityEngine;

// 스테이지 진행도 관리
public sealed class StageProgressController
{
    public int CurrentStageId
    {
        get
        {
            StageProgressSaveData progress = GetProgressData();
            return progress.CurrentStageId;
        }
    }

    public int HighestClearedStageId
    {
        get
        {
            StageProgressSaveData progress = GetProgressData();
            return progress.HighestClearedStageId;
        }
    }

    // 현재 진행 중인 스테이지 클리어 처리
    public void CompleteStage(int stageId)
    {
        StageProgressSaveData progress = GetProgressData();

        if (stageId != progress.CurrentStageId)
        {
            return;
        }

        progress.HighestClearedStageId = Mathf.Max(progress.HighestClearedStageId, stageId);

        int nextStageId = Mathf.Min(stageId + 1, StageDatabase.Instance.StageCount);
        progress.CurrentStageId = nextStageId;
    }

    // 패배한 스테이지를 중복 없이 기록해 조건부 패키지 노출에 사용함
    public void RecordStageDefeat(int stageId)
    {
        if (stageId < 1)
            return;

        StageProgressSaveData progress = GetProgressData();
        progress.DefeatedStageIds ??= new System.Collections.Generic.List<int>();
        if (!progress.DefeatedStageIds.Contains(stageId))
            progress.DefeatedStageIds.Add(stageId);
    }

    // 지정 스테이지에 한 번이라도 패배했는지 반환함
    public bool HasDefeatedStage(int stageId)
    {
        if (stageId < 1)
            return false;

        StageProgressSaveData progress = GetProgressData();
        return progress.DefeatedStageIds != null && progress.DefeatedStageIds.Contains(stageId);
    }

    // 현재 저장된 스테이지 진행도 반환
    private StageProgressSaveData GetProgressData()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null)
        {
            throw new InvalidOperationException("SaveManager가 초기화되지 않았습니다.");
        }

        SaveManager.Instance.CurrentData.StageProgress ??= new StageProgressSaveData();

        return SaveManager.Instance.CurrentData.StageProgress;
    }
}
