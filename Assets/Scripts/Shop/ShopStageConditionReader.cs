using System;

// 상점 패키지 조건에 필요한 스테이지 진행 정보를 전용으로 읽음
public sealed class ShopStageConditionReader
{
    // 저장된 최고 클리어 기록을 반환하고, 이전 순차 진행 세이브도 읽기 전용으로 보정함
    public int HighestClearedStageId
    {
        get
        {
            StageProgressSaveData progress = GetProgressData();
            return Math.Max(progress.HighestClearedStageId, Math.Max(0, progress.CurrentStageId - 1));
        }
    }

    // 지정 스테이지의 패배 이력이 있는지 반환함
    public bool HasDefeatedStage(int stageId)
    {
        if (stageId < 1)
            return false;

        StageProgressSaveData progress = GetProgressData();
        return progress.DefeatedStageIds != null && progress.DefeatedStageIds.Contains(stageId);
    }

    // 상점 조건 확인에 필요한 스테이지 진행 저장값만 조회함
    private static StageProgressSaveData GetProgressData()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null)
        {
            throw new InvalidOperationException("SaveManager가 초기화되지 않았습니다.");
        }

        return SaveManager.Instance.CurrentData.StageProgress ?? new StageProgressSaveData();
    }
}
