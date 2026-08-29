using UnityEngine;

public class QuestNPCInteraction : MonoBehaviour
{
    [Header("Quest Data")]
    [SerializeField] private int questId;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InteractionUIManager.Instance.SetInteractable(true, InteractType.NPC, OnInteractNPC);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InteractionUIManager.Instance.SetInteractable(false, InteractType.NPC);
        }
    }

    public void Initialize(int id)
    {
        questId = id;
    }

    private void OnInteractNPC()
    {
        if (QuestManager.Instance == null || DialogueManager.Instance == null) return;

        QuestData data = QuestManager.Instance.GetQuestData(questId);
        if (data == null) return;

        if (data.DialogueData != null)
        {
            // 대화를 띄우고 끝나면 클리어 및 삭제되도록 세팅
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

    public void SelfDestroy()
    {
        Destroy(gameObject);
    }
}
