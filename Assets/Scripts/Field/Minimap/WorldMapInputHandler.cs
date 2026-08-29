using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class WorldMapInputHandler : MonoBehaviour, IDragHandler, IScrollHandler
{
    [SerializeField] private WorldMapController worldMapController;

    [Header("Zoom")]
    [SerializeField] private float mouseZoomSpeed = 0.1f;
    [SerializeField] private float pinchZoomSpeed = 2f;

    private RectTransform touchArea;
    private Canvas rootCanvas;

    private void Awake()
    {
        touchArea = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        HandlePinchZoom();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (worldMapController == null) return;
        if (Touch.activeTouches.Count >= 2) return;

        worldMapController.MoveMap(eventData.delta);
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (worldMapController == null) return;

        float zoomAmount = eventData.scrollDelta.y * mouseZoomSpeed;

        worldMapController.ZoomAtScreenPoint(zoomAmount, eventData.position);
    }

    private void HandlePinchZoom()
    {
        if (worldMapController == null || touchArea == null) return;
        if (Touch.activeTouches.Count < 2) return;

        Touch touch0 = Touch.activeTouches[0];
        Touch touch1 = Touch.activeTouches[1];

        Vector2 pinchCenter = (touch0.screenPosition + touch1.screenPosition) * 0.5f;

        if (!RectTransformUtility.RectangleContainsScreenPoint(touchArea, pinchCenter, null)) return;

        float currentDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);

        Vector2 previousPosition0 = touch0.screenPosition - touch0.delta;
        Vector2 previousPosition1 = touch1.screenPosition - touch1.delta;

        float previousDistance = Vector2.Distance(previousPosition0, previousPosition1);
        float pinchDelta = currentDistance - previousDistance;

        if (Mathf.Abs(pinchDelta) < 0.01f) return;

        float screenReference = Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height));
        float zoomAmount = (pinchDelta / screenReference) * pinchZoomSpeed;

        worldMapController.ZoomAtScreenPoint(zoomAmount, pinchCenter);
    }
}