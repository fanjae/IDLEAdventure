using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class SlotDragController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private SlotBoard slotBoard;
    [SerializeField] private SlotInput slotInput;
    [SerializeField] private SlotHighlightController slotHighlightController;
    [SerializeField] private Camera dragCamera;

    [Header("드래그 설정")]
    [SerializeField] private float dragObjHeight = 0.3f;
    [SerializeField] private float rayDistance = 500f;

    private GameObject dragObj;
    private int dragSlotBase = -1;

    private Vector3 dragOriginalPos;
    private Vector3 dragFootOffset;

    private Plane dragPlane;

    private Collider[] dragColliders;
    private bool[] dragColliderChild;

    private readonly List<RaycastResult> uiRaycastResults = new();

    public bool IsDragging => dragObj != null;

    private readonly RaycastHit[] raycastHits = new RaycastHit[8];

    private void Awake()
    {
        if (slotBoard == null)
        {
            throw new Exception("SlotDragController의 Slot Board가 연결되어 있지 않습니다.");
        }

        if (slotInput == null)
        {
            throw new Exception("SlotDragController의 Slot Input이 연결되어 있지 않습니다.");
        }
        if (slotHighlightController == null)
        {
            throw new Exception("SlotDragController의 Slot Highlight Controller가 연결되어 있지 않습니다.");
        }

        if (dragCamera == null)
        {
            dragCamera = Camera.main;
        }
    }

    private void OnDisable()
    {
        CancelCurrentDrag();
    }

    private void Update()
    {
        if (slotInput.WasPressedThisFrame())
        {
            StartDrag();
        }

        if (dragObj != null && slotInput.IsPressed())
        {
            UpdateDrag();
        }

        if (dragObj != null && slotInput.WasReleasedThisFrame())
        {
            EndDrag();
        }
    }

    public void CancelCurrentDrag()
    {
        if (dragObj == null)
        {
            return;
        }

        dragObj.transform.position = dragOriginalPos;

        FinishDrag();
    }

    private void StartDrag()
    {
        if (dragCamera == null)
        {
            return;
        }

        Vector2 pointerPosition = slotInput.PointerPosition;

        if (IsPointerOverUI(pointerPosition))
        {
            return;
        }

        Ray ray = dragCamera.ScreenPointToRay(pointerPosition);

        int slotNumber = FindObj(ray);

        if (slotNumber == -1)
        {
            return;
        }

        dragObj = slotBoard.GetObj(slotNumber);

        if (dragObj == null)
        {
            return;
        }

        dragSlotBase = slotNumber;
        dragOriginalPos = dragObj.transform.position;

        Vector3 footPosition = GetObjFootPosition(dragObj);

        dragFootOffset = dragObj.transform.position - footPosition;

        Vector3 dragFootPosition = footPosition + Vector3.up * dragObjHeight;

        dragPlane = new Plane(Vector3.up, dragFootPosition);

        DisableDragObjColliders();

        slotHighlightController.StartDragHighlight();

        Debug.Log($"슬롯 드래그 시작: Object={dragObj.name}, Slot={dragSlotBase}");
    }

    private void UpdateDrag()
    {
        if (dragCamera == null || dragObj == null)
        {
            return;
        }

        Vector2 pointerPosition = slotInput.PointerPosition;
        Ray ray = dragCamera.ScreenPointToRay(pointerPosition);

        if (!dragPlane.Raycast(ray, out float distance))
        {
            return;
        }

        Vector3 point = ray.GetPoint(distance);

        dragObj.transform.position = point + dragFootOffset;

        int targetSlot = FindTargetSlot(ray);

        slotHighlightController.SetTargetSlot(targetSlot, dragSlotBase);
    }

    private void EndDrag()
    {
        if (dragCamera == null)
        {
            CancelCurrentDrag();
            return;
        }

        Vector2 pointerPosition = slotInput.PointerPosition;

        if (IsPointerOverUI(pointerPosition))
        {
            CancelCurrentDrag();
            return;
        }

        Ray ray = dragCamera.ScreenPointToRay(pointerPosition);

        int targetSlot = FindTargetSlot(ray);

        if (targetSlot == -1)
        {
            CancelCurrentDrag();
            return;
        }

        if (!slotBoard.MoveOrSwap(dragSlotBase, targetSlot))
        {
            CancelCurrentDrag();
            return;
        }

        Debug.Log($"슬롯 변경: {dragSlotBase} → {targetSlot}");

        FinishDrag();
    }

    private int FindObj(Ray ray)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance);

        float closestDistance = float.MaxValue;
        int closestSlot = -1;

        foreach (RaycastHit hit in hits)
        {
            int slotNumber = slotBoard.FindObj(hit.transform);

            if (slotNumber == -1)
            {
                continue;
            }

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestSlot = slotNumber;
            }
        }

        return closestSlot;
    }

    private int FindTargetSlot(Ray ray)
    {
        int hitCount = Physics.RaycastNonAlloc(ray, raycastHits, rayDistance);

        float closestDistance = float.MaxValue;
        int closestSlot = -1;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = raycastHits[i];

            int slotNumber = slotBoard.FindSlot(hit.transform);

            if (slotNumber == -1)
            {
                continue;
            }

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestSlot = slotNumber;
            }
        }

        return closestSlot;
    }

    private Vector3 GetObjFootPosition(GameObject target)
    {
        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);

        if (colliders.Length == 0)
        {
            return target.transform.position;
        }

        Bounds bounds = colliders[0].bounds;

        for (int i = 1; i < colliders.Length; i++)
        {
            bounds.Encapsulate(colliders[i].bounds);
        }

        return new Vector3(target.transform.position.x, bounds.min.y, target.transform.position.z);
    }

    private void DisableDragObjColliders()
    {
        dragColliders = dragObj.GetComponentsInChildren<Collider>(true);
        dragColliderChild = new bool[dragColliders.Length];

        for (int i = 0; i < dragColliders.Length; i++)
        {
            dragColliderChild[i] = dragColliders[i].enabled;
            dragColliders[i].enabled = false;
        }
    }

    private void EnableDragObjColliders()
    {
        if (dragColliders == null || dragColliderChild == null)
        {
            return;
        }

        for (int i = 0; i < dragColliders.Length; i++)
        {
            if (dragColliders[i] != null)
            {
                dragColliders[i].enabled = dragColliderChild[i];
            }
        }

        dragColliders = null;
        dragColliderChild = null;
    }

    private void FinishDrag()
    {
        EnableDragObjColliders();

        slotHighlightController.EndDragHighlight();

        dragObj = null;
        dragSlotBase = -1;
        dragOriginalPos = Vector3.zero;
        dragFootOffset = Vector3.zero;
    }

    private bool IsPointerOverUI(Vector2 pointerPosition)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = pointerPosition;

        uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(pointerEventData, uiRaycastResults);

        return uiRaycastResults.Count > 0;
    }
}