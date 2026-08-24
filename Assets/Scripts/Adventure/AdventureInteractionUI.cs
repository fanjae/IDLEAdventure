using UnityEngine;

/// <summary>
/// 상호작용 UI를 담당할 클래스.
/// </summary>
public class AdventureInteractionUI : MonoBehaviour
{
    [Header("UI Component")]
    [SerializeField] private GameObject interactionButton;

    [SerializeField] private int questId;

    private void Awake()
    {
        if (interactionButton != null)
        {
            interactionButton.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (QuestManager.Instance == null) return;

        QuestManager.Instance.OnNPCInteracted += IsInteractable;
    }
    private void OnDisable()
    {
        if (QuestManager.Instance == null) return;

        QuestManager.Instance.OnNPCInteracted -= IsInteractable;
    }

    // 퀘스트 상호작용 가능 여부에 따라 작동할 함수.
    // 상호작용 가능 여부에 따라 진행할 퀘스트 번호 저장 및 UI 활성화.
    private void IsInteractable(int id, bool isInteractable)
    {
        if (interactionButton == null) return;

        if (isInteractable)
        {
            questId = id;
            interactionButton.SetActive(true);
        }
        else
        {
            interactionButton.SetActive(false);
            questId = 0;
        }
    }

    // 상호작용 버튼을 눌렀을 때 호출될 함수.
    // 저장해둔 ID를 가진 퀘스트 진행 (대화 > 클리어)
    public void OnClickInteractionButton()
    {
        if (questId == 0) return;
        if (interactionButton == null) return;
        if (QuestManager.Instance == null || DialogueManager.Instance == null) return;

        QuestData data = QuestManager.Instance.GetQuestData(questId);
        if (data == null) return;

        if (data.DialogueData != null)
        {
            DialogueManager.Instance.StartDialogue(data.DialogueData, () =>
            {
                QuestManager.Instance.ClearQuest(questId);
                QuestManager.Instance.DestroyNPC();
            });
        }
        else
        {
            QuestManager.Instance.ClearQuest(questId);
            QuestManager.Instance.DestroyNPC();
        }

        interactionButton.SetActive(false);
    }
}