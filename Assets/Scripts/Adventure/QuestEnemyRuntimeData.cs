using UnityEngine;

public static class QuestEnemyRuntimeData
{
    public static int InteractedQuestId { get; private set; }

    public static void SetQuestEnemyData(int questId)
    {
        InteractedQuestId = questId;
    }
}
