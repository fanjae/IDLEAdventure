using UnityEngine;

public static class FieldEnemyRuntimeData
{
    public static int InteractedFieldEnemyId { get; private set; }

    public static void SetEnemyData(int enemyId)
    {
        InteractedFieldEnemyId = enemyId;
    }
}

