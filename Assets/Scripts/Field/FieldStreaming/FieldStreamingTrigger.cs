using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class FieldStreamingTrigger : MonoBehaviour
{
    private FieldStreamingManager streamingManager;
    private Transform player;
    private float chunkSize;
    private bool initialized;

    public void Initialize(FieldStreamingManager manager, Transform targetPlayer, float size, Vector2Int startChunk)
    {
        streamingManager = manager;
        player = targetPlayer;
        chunkSize = size;

        MoveToChunk(startChunk);
        initialized = true;
    }

    public void MoveToChunk(Vector2Int chunk)
    {
        float centerX = (chunk.x + 0.5f) * chunkSize;
        float centerZ = (chunk.y + 0.5f) * chunkSize;

        transform.position = new Vector3(centerX, 0f, centerZ);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!initialized) return;
        if (!IsPlayerCollider(other)) return;

        streamingManager.HandleTriggerExit();
    }

    private bool IsPlayerCollider(Collider other)
    {
        if (other.transform == player) return true;
        if (other.transform.IsChildOf(player)) return true;

        return false;
    }
}