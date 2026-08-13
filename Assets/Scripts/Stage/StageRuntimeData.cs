public static class StageRuntimeData
{
    public static int SelectedStageId { get; private set; } = -1;

    public static void SelectStage(int stageId)
    {
        SelectedStageId = stageId;
    }
}