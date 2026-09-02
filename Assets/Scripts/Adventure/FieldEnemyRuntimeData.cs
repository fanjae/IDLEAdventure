public static class FieldEnemyRuntimeData
{
    public static int InteractedFieldEnemyId { get; private set; } = -1;
    public static bool HasEnemyData => InteractedFieldEnemyId >= 0;

    public static void SetEnemyData(int enemyId)
    {
        InteractedFieldEnemyId = enemyId;
    }

    // 2026.09.02 필드 적 전투 종료 후 런타임 적 정보 초기화
    public static void ClearEnemyData()
    {
        InteractedFieldEnemyId = -1;
    }
}

