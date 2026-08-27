using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform mapImage;
    [SerializeField] private RectTransform playerMarker;

    [Header("World")]
    [SerializeField] private Vector2 worldMin = new Vector2(-150f, -150f);
    [SerializeField] private Vector2 worldMax = new Vector2(200f, 200f);

    [Header("Player Marker")]
    [SerializeField] private float markerRotationOffset = 0f;

    private void LateUpdate()
    {
        if (player == null || mapImage == null || playerMarker == null) return;

        UpdateMapPosition();
        UpdatePlayerDirection();
    }

    private void UpdateMapPosition()
    {
        float normalizedX = Mathf.InverseLerp(worldMin.x, worldMax.x, player.position.x);
        float normalizedZ = Mathf.InverseLerp(worldMin.y, worldMax.y, player.position.z);

        float mapX = (0.5f - normalizedX) * mapImage.rect.width;
        float mapY = (0.5f - normalizedZ) * mapImage.rect.height;

        mapImage.anchoredPosition = new Vector2(mapX, mapY);
    }

    private void UpdatePlayerDirection()
    {
        float angle = -player.eulerAngles.y + markerRotationOffset;
        playerMarker.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}