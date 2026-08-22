public static class StageRuntimeData
{
    public static int SelectedStageId { get; private set; } = -1;
    public static bool IsAutoBattle { get; private set; } // 0821 추가(자동 전투설정)

    public static void SelectStage(int stageId)
    {
        SelectedStageId = stageId;
    }

    // 자동전투 시작 0821 추가
    public static void StartAutoBattle()
    {
        IsAutoBattle = true;
    }

    // 자동전투 종료 0821 추가
    public static void StopAutoBattle()
    {
        IsAutoBattle = false;
    }
}