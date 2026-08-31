public static class StageRuntimeData
{
    public static int SelectedStageId { get; private set; } = -1;
    public static bool IsAutoBattle { get; private set; } // 0821 추가(자동 전투설정)

    // 2026.08.31 필드 적 상호작용 전투의 연속 스테이지 진행 방지를 위한 상태값
    public static bool IsFieldEnemyBattle { get; private set; }

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

    // 2026.08.31 필드 적 상호작용 전투 진입 상태 설정
    public static void StartFieldEnemyBattle()
    {
        IsFieldEnemyBattle = true;
    }

    // 2026.08.31 필드 적 상호작용 전투 종료 상태 설정
    public static void StopFieldEnemyBattle()
    {
        IsFieldEnemyBattle = false;
    }
}