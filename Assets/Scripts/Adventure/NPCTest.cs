using UnityEngine;

/// <summary>
/// NPC와 상호작용이 가능한지 여부를 판단하기 위한 테스트 클래스.
/// </summary>
public class NPCTest : MonoBehaviour
{
    [Header("Quest Data")]
    [SerializeField] private int questId;

    private void OnTriggerEnter(Collider other)
    {
        if (QuestManager.Instance == null) return;

        if (other.CompareTag("Player"))
        {
            QuestManager.Instance.NPCInteractable(questId, true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (QuestManager.Instance == null) return;

        if (other.CompareTag("Player"))
        {
            QuestManager.Instance.NPCInteractable(questId, false);
        }
    }

    public void Initialize(int id)
    {
        questId = id;
    }
    public void selfDestroy()
    {
        Destroy(gameObject);
    }
}