using UnityEngine;

/// <summary>
/// 퀘스트 채집 객체 상호작용 클래스.
/// </summary>
public class QuestGatherInteraction : QuestInteractableObject
{
    protected override InteractType GetInteractType() => InteractType.Gather;

    protected override void OnInteract()
    {
        if (isInteracting) return;
        isInteracting = true;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.RemoveGatherTarget(this);

            InteractionUIManager.Instance.SetInteractable(false, GetInteractType());
        }
    }
}
