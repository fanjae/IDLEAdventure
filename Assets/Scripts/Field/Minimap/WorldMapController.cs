using UnityEngine;

public class WorldMapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform mapViewport;
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private RectTransform playerMarker;

    [Header("World")]
    [SerializeField] private Vector2 worldMin = new Vector2(-150f, -150f);
    [SerializeField] private Vector2 worldMax = new Vector2(200f, 200f);

    [Header("Focus")]
    [SerializeField] private Vector2 focusPadding = new Vector2(50f, 50f);

    [Header("Zoom")]
    [SerializeField] private float defaultZoom = 0.8f;
    [SerializeField] private float minZoom = 0.8f;
    [SerializeField] private float maxZoom = 2f;
    [SerializeField] private float zoomSmoothTime = 0.15f;

    private Canvas rootCanvas;

    private float currentZoom;
    private float targetZoom;
    private float zoomVelocity;

    private Vector2 zoomAnchorViewportPoint;
    private Vector2 zoomAnchorMapPoint;
    private bool hasZoomAnchor;

    private Vector3 currentFocusWorldPosition;

    public float CurrentZoom => currentZoom;
    public float MaxZoom => maxZoom;
    public Vector3 CurrentFocusWorldPosition => currentFocusWorldPosition;

    private void Awake()
    {
        rootCanvas = mapViewport.GetComponentInParent<Canvas>();

        currentZoom = defaultZoom;
        targetZoom = defaultZoom;
    }

    private void Update()
    {
        UpdateSmoothZoom();
    }

    private void LateUpdate()
    {
        UpdatePlayerMarker();
    }

    public void WorldMapOpen()
    {
        currentZoom = defaultZoom;
        targetZoom = defaultZoom;
        zoomVelocity = 0f;
        hasZoomAnchor = false;

        Zoom();
        UpdatePlayerMarker();
        CenterPlayer();

        currentFocusWorldPosition = ClampFocusWorldPosition(new Vector3(player.position.x, 0f, player.position.z));
    }

    private void UpdatePlayerMarker()
    {
        if (player == null || mapContent == null || playerMarker == null) return;

        playerMarker.anchoredPosition = WorldToMapPosition(player.position);
    }

    private Vector2 WorldToMapPosition(Vector3 worldPos)
    {
        float worldX = Mathf.InverseLerp(worldMin.x, worldMax.x, worldPos.x);
        float worldZ = Mathf.InverseLerp(worldMin.y, worldMax.y, worldPos.z);

        float mapX = Mathf.Lerp(mapContent.rect.xMin, mapContent.rect.xMax, worldX);
        float mapY = Mathf.Lerp(mapContent.rect.yMin, mapContent.rect.yMax, worldZ);

        return new Vector2(mapX, mapY);
    }

    private Vector3 MapToWorldPosition(Vector2 mapPos)
    {
        float mapX = Mathf.InverseLerp(mapContent.rect.xMin, mapContent.rect.xMax, mapPos.x);
        float mapZ = Mathf.InverseLerp(mapContent.rect.yMin, mapContent.rect.yMax, mapPos.y);

        float worldX = Mathf.Lerp(worldMin.x, worldMax.x, mapX);
        float worldZ = Mathf.Lerp(worldMin.y, worldMax.y, mapZ);

        return new Vector3(worldX, 0f, worldZ);
    }

    private void CenterPlayer()
    {
        if (player == null || mapContent == null) return;

        Vector2 playerMapPos = WorldToMapPosition(player.position);

        mapContent.anchoredPosition = Vector2.zero;

        Vector3 playerMapWorldPos = mapContent.TransformPoint(playerMapPos);
        Vector3 viewportCenterWorldPos = mapViewport.TransformPoint(mapViewport.rect.center);

        Vector3 worldPos = viewportCenterWorldPos - playerMapWorldPos;
        Vector3 localPos = mapContent.parent.InverseTransformVector(worldPos);

        mapContent.anchoredPosition += new Vector2(localPos.x, localPos.y);

        ClampMapPosition();
    }


    //드래그 로직 관련 WorldMapInputHandler에서 사용
    public void MoveMap(Vector2 delta)
    {
        hasZoomAnchor = false;
        targetZoom = currentZoom;
        zoomVelocity = 0f;

        Vector2 moveLimit = GetMapMoveLimit();

        mapContent.anchoredPosition += delta;
        ClampMapPosition();

        float mapWidth = mapContent.rect.width * currentZoom;
        float mapHeight = mapContent.rect.height * currentZoom;

        if (moveLimit.x > 0.001f)
        {
            float worldDeltaX = -(delta.x / mapWidth) * (worldMax.x - worldMin.x);
            currentFocusWorldPosition.x += worldDeltaX;
        }

        if (moveLimit.y > 0.001f)
        {
            float worldDeltaZ = -(delta.y / mapHeight) * (worldMax.y - worldMin.y);
            currentFocusWorldPosition.z += worldDeltaZ;
        }

        currentFocusWorldPosition = ClampFocusWorldPosition(currentFocusWorldPosition);
    }

    public void ZoomAtScreenPoint(float amount, Vector2 screenPoint)
    {
        if (Mathf.Approximately(amount, 0f)) return;

        float newTargetZoom = Mathf.Clamp(targetZoom + amount, minZoom, maxZoom);
        if (Mathf.Approximately(newTargetZoom, targetZoom)) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(mapViewport, screenPoint, null, out Vector2 viewportPoint)) return;

        Vector3 viewportWorldPos = mapViewport.TransformPoint(viewportPoint);
        Vector3 mapLocalPos = mapContent.InverseTransformPoint(viewportWorldPos);

        zoomAnchorViewportPoint = viewportPoint;
        zoomAnchorMapPoint = new Vector2(mapLocalPos.x, mapLocalPos.y);
        hasZoomAnchor = true;

        currentFocusWorldPosition = ClampFocusWorldPosition(MapToWorldPosition(zoomAnchorMapPoint));

        targetZoom = newTargetZoom;
    }

    private void UpdateSmoothZoom()
    {
        if (Mathf.Approximately(currentZoom, targetZoom)) return;

        if (Mathf.Abs(currentZoom - targetZoom) < 0.001f)
        {
            currentZoom = targetZoom;
            zoomVelocity = 0f;

            Zoom();

            if (hasZoomAnchor) KeepZoomAnchorPosition();

            ClampMapPosition();

            hasZoomAnchor = false;
            return;
        }

        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomSmoothTime);

        Zoom();

        if (hasZoomAnchor) KeepZoomAnchorPosition();

        ClampMapPosition();
    }

    private void KeepZoomAnchorPosition()
    {
        Vector3 currentAnchorWorldPos = mapContent.TransformPoint(zoomAnchorMapPoint);
        Vector3 targetAnchorWorldPos = mapViewport.TransformPoint(zoomAnchorViewportPoint);

        Vector3 worldPos = targetAnchorWorldPos - currentAnchorWorldPos;
        Vector3 localPos = mapContent.parent.InverseTransformVector(worldPos);

        mapContent.anchoredPosition += new Vector2(localPos.x, localPos.y);
    }

    private void Zoom()
    {
        mapContent.localScale = Vector3.one * currentZoom;
        UpdateMarkerScale();
    }

    private void UpdateMarkerScale()
    {
        if (playerMarker == null || currentZoom <= 0f) return;

        playerMarker.localScale = Vector3.one / currentZoom;
    }

    private Vector2 GetMapMoveLimit()
    {
        float scaledWidth = mapContent.rect.width * currentZoom;
        float scaledHeight = mapContent.rect.height * currentZoom;

        float maxX = Mathf.Max(0f, (scaledWidth - mapViewport.rect.width) * 0.5f);
        float maxY = Mathf.Max(0f, (scaledHeight - mapViewport.rect.height) * 0.5f);

        return new Vector2(maxX, maxY);
    }

    private void ClampMapPosition()
    {
        Vector2 moveLimit = GetMapMoveLimit();
        Vector2 position = mapContent.anchoredPosition;

        position.x = Mathf.Clamp(position.x, -moveLimit.x, moveLimit.x);
        position.y = Mathf.Clamp(position.y, -moveLimit.y, moveLimit.y);

        mapContent.anchoredPosition = position;
    }

    private Vector3 ClampFocusWorldPosition(Vector3 worldPosition)
    {
        worldPosition.x = Mathf.Clamp(worldPosition.x, worldMin.x + focusPadding.x, worldMax.x - focusPadding.x);
        worldPosition.y = 0f;
        worldPosition.z = Mathf.Clamp(worldPosition.z, worldMin.y + focusPadding.y, worldMax.y - focusPadding.y);

        return worldPosition;
    }
}