using UnityEngine;

public class QuestNPCInteraction : QuestInteractableObject
{
    protected override InteractType GetInteractType() => InteractType.NPC;

    protected override void OnInteract()
    {
        if (QuestManager.Instance == null || DialogueManager.Instance == null) return;

        QuestData data = QuestManager.Instance.GetQuestData(questId);
        if (data == null) return;

        if (data.DialogueData != null)
        {
            DialogueManager.Instance.StartDialogue(data.DialogueData, () =>
            {
                QuestManager.Instance.ClearQuest(questId);
                SelfDestroy();
            });
        }
        else
        {
            QuestManager.Instance.ClearQuest(questId);
            SelfDestroy();
        }
    }
}
