using System;

// 스테이지 진행도 저장 데이터
[Serializable]
public sealed class StageProgressSaveData
{
    public int CurrentStageId { get; set; } = 1;
}