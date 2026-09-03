using UnityEngine;

/// <summary>
/// 퀘스트 상호작용 객체가 상속받을 추상 클래스.
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class QuestInteractableObject : MonoBehaviour
{
    [Header("Quest Data")]
    [SerializeField] protected int questId;

    protected bool isInteracting = false;

    // 프로퍼티
    public int QuestId => questId;


    public virtual void Initialize(int id)
    {
        questId = id;
    }

    protected abstract InteractType GetInteractType();
    protected abstract void OnInteract();

    // Player 태그를 가진 객체와 충돌 시 상호작용 UI 출력.
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isInteracting) return;
        if (other.CompareTag("Player"))
        {
            InteractionUIManager.Instance.SetInteractable(true, GetInteractType(), OnInteract);
        }
    }
    // Player 태그를 가진 객체와 충돌이 끝날 시 상호작용 UI 제거.
    protected virtual void OnTriggerExit(Collider other)
    {
        if (isInteracting) return;
        if (other.CompareTag("Player"))
        {
            InteractionUIManager.Instance.SetInteractable(false, GetInteractType());
        }
    }
    // 객체 제거 함수.
    public virtual void SelfDestroy()
    {
        Destroy(gameObject);
    }
}
