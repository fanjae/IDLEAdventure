using System.Collections;
using UnityEngine;

public class MapModeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject miniMap;
    [SerializeField] private GameObject worldMap;
    [SerializeField] private CanvasGroup mapViewportCanvasGroup;
    [SerializeField] private WorldMapController worldMapController;

    //
    [SerializeField] private FieldStreamingManager fieldStreamingManager;
    //

    [Header("Map Camera")]
    [SerializeField] private float mapCameraY = 50f;
    [SerializeField] private float transitionDuration = 0.7f;

    [Header("Map Fade")]
    [SerializeField, Range(0f, 1f)] private float mapFadeStart = 0.55f;

    [Header("3D Blend")]
    [SerializeField] private float worldBlendStartZoom = 1.5f;
    //
    [SerializeField] private float map3DStartZoom = 1.7f;
    //

    private Transform cameraParent;
    private Vector3 localPosition;
    private Quaternion localRotation;
    private Vector3 worldPosition;
    private Quaternion worldRotation;
    private Vector3 horizontalOffset;

    private float regionCameraWorldY;

    //
    private Vector3 mapIdleCameraPosition;
    //

    private bool isMapOpen;
    private bool isTransitioning;

    private void Awake()
    {
        miniMap.SetActive(true);
        worldMap.SetActive(false);

        isMapOpen = false;
        isTransitioning = false;
    }

    private void LateUpdate()
    {
        if (!isMapOpen || isTransitioning) return;

        UpdateMapCamera();

        //
        fieldStreamingManager.UpdateMapStreaming(mainCamera.transform.position);
        //
    }

    public void OpenMap()
    {
        if (isMapOpen || isTransitioning) return;

        StartCoroutine(EnterMapMode());
    }

    public void CloseMap()
    {
        if (!isMapOpen || isTransitioning) return;

        //
        bool closeFrom2D = worldMapController.CurrentZoom < map3DStartZoom;
        StartCoroutine(ExitMapMode(closeFrom2D));
        //

        //StartCoroutine(ExitMapMode());
    }

    private IEnumerator EnterMapMode()
    {
        isTransitioning = true;

        SaveCamera();

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        mainCamera.transform.SetParent(null, true);

        miniMap.SetActive(false);
        worldMap.SetActive(true);

        mapViewportCanvasGroup.alpha = 0f;
        mapViewportCanvasGroup.interactable = false;
        mapViewportCanvasGroup.blocksRaycasts = false;

        worldMapController.WorldMapOpen();

        Vector3 focusPos = worldMapController.CurrentFocusWorldPosition;

        Vector3 targetPos = new Vector3(focusPos.x, mapCameraY, focusPos.z);
        Quaternion targetRotation = Quaternion.Euler(90f, 0f, 0f);

        //
        mapIdleCameraPosition = targetPos;
        //

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);

            float fadeT = Mathf.InverseLerp(mapFadeStart, 1f, t);
            mapViewportCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, fadeT);

            yield return null;
        }

        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRotation;

        mapViewportCanvasGroup.alpha = 1f;
        mapViewportCanvasGroup.interactable = true;
        mapViewportCanvasGroup.blocksRaycasts = true;

        //
        fieldStreamingManager.StartMapStreaming(mainCamera.transform.position);
        //

        isMapOpen = true;
        isTransitioning = false;
    }

    private void SaveCamera()
    {
        cameraParent = mainCamera.transform.parent;

        worldPosition = mainCamera.transform.position;
        worldRotation = mainCamera.transform.rotation;

        regionCameraWorldY = worldPosition.y;

        if (cameraParent != null)
        {
            localPosition = mainCamera.transform.localPosition;
            localRotation = mainCamera.transform.localRotation;

            Vector3 localHorizontalOffset = new Vector3(localPosition.x, 0f, localPosition.z);
            horizontalOffset = cameraParent.TransformVector(localHorizontalOffset);
        }
        else
        {
            localPosition = Vector3.zero;
            localRotation = Quaternion.identity;
            horizontalOffset = Vector3.zero;
        }
    }

    private void UpdateMapCamera()
    {
        Vector3 focusPos = worldMapController.CurrentFocusWorldPosition;

        //float zoom = worldMapController.CurrentZoom;
        //float blend = Mathf.InverseLerp(worldBlendStartZoom, worldMapController.MaxZoom, zoom);
        //blend = Mathf.SmoothStep(0f, 1f, blend);

        //
        float blend = GetMapBlend();
        //

        Vector3 topPosition = new Vector3(focusPos.x, mapCameraY, focusPos.z);

        Vector3 regionPos = new Vector3(
            focusPos.x + horizontalOffset.x,
            regionCameraWorldY,
            focusPos.z + horizontalOffset.z);

        Quaternion topRotation = Quaternion.Euler(90f, 0f, 0f);

        mainCamera.transform.position = Vector3.Lerp(topPosition, regionPos, blend);
        mainCamera.transform.rotation = Quaternion.Slerp(topRotation, worldRotation, blend);

        mapViewportCanvasGroup.alpha = 1f - blend;
    }

    private IEnumerator ExitMapMode(bool closeFrom2D)
    {
        isTransitioning = true;

        mapViewportCanvasGroup.interactable = false;
        mapViewportCanvasGroup.blocksRaycasts = false;

        //
        if (closeFrom2D)
        {
            mainCamera.transform.position = mapIdleCameraPosition;
            mainCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        //

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        float startAlpha = mapViewportCanvasGroup.alpha;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 targetPos = GetCameraPosition();
            Quaternion targetRotation = GetCameraRotation();

            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);

            //
            fieldStreamingManager.UpdateMapStreaming(mainCamera.transform.position);
            //

            mapViewportCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, smoothT);

            yield return null;
        }

        RestoreCamera();

        //
        fieldStreamingManager.EndMapStreaming();
        //

        worldMap.SetActive(false);
        miniMap.SetActive(true);

        mapViewportCanvasGroup.alpha = 1f;

        isMapOpen = false;
        isTransitioning = false;
    }

    private Vector3 GetCameraPosition()
    {
        if (cameraParent != null)
        {
            return cameraParent.TransformPoint(localPosition);
        }

        return worldPosition;
    }

    private Quaternion GetCameraRotation()
    {
        if (cameraParent != null)
        {
            return cameraParent.rotation * localRotation;
        }

        return worldRotation;
    }

    private void RestoreCamera()
    {
        if (cameraParent != null)
        {
            mainCamera.transform.SetParent(cameraParent, false);
            mainCamera.transform.localPosition = localPosition;
            mainCamera.transform.localRotation = localRotation;
        }
        else
        {
            mainCamera.transform.SetParent(null);
            mainCamera.transform.position = worldPosition;
            mainCamera.transform.rotation = worldRotation;
        }
    }

    //
    private float GetMapBlend()
    {
        float blend = Mathf.InverseLerp(worldBlendStartZoom, worldMapController.MaxZoom, worldMapController.CurrentZoom);
        return Mathf.SmoothStep(0f, 1f, blend);
    }
    //

}