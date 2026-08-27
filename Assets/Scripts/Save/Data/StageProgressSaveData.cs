using System;
using System.Collections.Generic;

// 스테이지 진행도 저장 데이터
[Serializable]
public sealed class StageProgressSaveData
{
    public int CurrentStageId { get; set; } = 1;
    public List<int> DefeatedStageIds { get; set; } = new();
}
