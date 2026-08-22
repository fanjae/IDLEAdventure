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

    // 현재 진행 중인 스테이지 클리어 처리
    public void CompleteStage(int stageId)
    {
        StageProgressSaveData progress = GetProgressData();

        if (stageId != progress.CurrentStageId)
        {
            return;
        }

        int nextStageId = Mathf.Min(stageId + 1, StageDatabase.Instance.StageCount);
        progress.CurrentStageId = nextStageId;
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