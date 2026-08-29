using System;
using System.Collections.Generic;

// 스테이지 진행도 저장 데이터
[Serializable]
public sealed class StageProgressSaveData
{
    // 현재 진행 중인 스테이지
    public int CurrentStageId { get; set; } = 1;
    // 패배한 스테이지 ID
    public List<int> DefeatedStageIds { get; set; } = new();

    // 지금까지 클리어한 최고 스테이지
    public int HighestClearedStageId { get; set; } = 0;
}
