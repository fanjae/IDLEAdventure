using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class QuestInteractableObject : MonoBehaviour
{
    [Header("Quest Data")]
    [SerializeField] protected int questId;

    protected bool isInteracting = false;


    public virtual void Initialize(int id)
    {
        questId = id;
    }

    protected abstract InteractType GetInteractType();
    protected abstract void OnInteract();

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isInteracting) return;
        if (other.CompareTag("Player"))
        {
            InteractionUIManager.Instance.SetInteractable(true, GetInteractType(), OnInteract);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (isInteracting) return;
        if (other.CompareTag("Player"))
        {
            InteractionUIManager.Instance.SetInteractable(false, GetInteractType());
        }
    }

    public virtual void SelfDestroy()
    {
        Destroy(gameObject);
    }
}
