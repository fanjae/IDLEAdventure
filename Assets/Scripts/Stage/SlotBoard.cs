using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SlotBoard : MonoBehaviour
{
    [Header("슬롯")]
    [SerializeField] private Transform[] slots;

    private GameObject[] obj;

    private readonly Dictionary<GameObject, int> slotObj = new();
    private readonly Dictionary<GameObject, Vector3> offsetObj = new();

    public int SlotCount => slots != null ? slots.Length : 0;

    private void Awake()
    {
        int slotCount = slots != null ? slots.Length : 0;

        obj = new GameObject[slotCount];
    }

    public Transform GetSlotTransform(int slotNumber)
    {
        if (!IsSlot(slotNumber))
        {
            return null;
        }

        return slots[slotNumber];
    }

    public bool IsSlot(int slotNumber)
    {
        return slots != null && slotNumber > 0 && slotNumber < slots.Length && slots[slotNumber] != null;
    }

    public bool IsEmpty(int slotNumber)
    {
        return IsSlot(slotNumber) && obj[slotNumber] == null;
    }

    public int FindEmptySlot()
    {
        if (slots == null || obj == null)
        {
            return -1;
        }

        for (int i = 1; i < slots.Length; i++)
        {
            if (slots[i] != null && obj[i] == null)
            {
                return i;
            }
        }

        return -1;
    }

    public GameObject GetObj(int slotNumber)
    {
        if (!IsSlot(slotNumber))
        {
            return null;
        }

        return obj[slotNumber];
    }

    public int FindObj(GameObject target)
    {
        if (target == null)
        {
            return -1;
        }

        return slotObj.TryGetValue(target, out int slotNumber) ? slotNumber : -1;
    }

    public int FindObj(Transform hitTransform)
    {
        if (hitTransform == null || obj == null)
        {
            return -1;
        }

        for (int i = 1; i < obj.Length; i++)
        {
            GameObject occupant = obj[i];

            if (occupant == null)
            {
                continue;
            }

            if (hitTransform == occupant.transform || hitTransform.IsChildOf(occupant.transform))
            {
                return i;
            }
        }

        return -1;
    }

    public int FindSlot(Transform hitTransform)
    {
        if (hitTransform == null || slots == null)
        {
            return -1;
        }

        for (int i = 1; i < slots.Length; i++)
        {
            Transform slot = slots[i];

            if (slot == null)
            {
                continue;
            }

            if (hitTransform == slot || hitTransform.IsChildOf(slot))
            {
                return i;
            }
        }

        return -1;
    }

    public Vector3 GetSlotPosition(int slotNumber)
    {
        return GetSlotPosition(slotNumber, Vector3.zero);
    }

    public Vector3 GetSlotPosition(int slotNumber, Vector3 localOffset)
    {
        if (!IsSlot(slotNumber))
        {
            throw new Exception($"{slotNumber}번 슬롯 문제");
        }

        Transform slot = slots[slotNumber];

        return slot.position + slot.rotation * localOffset;
    }

    public bool Place(GameObject target, int slotNumber, Vector3 localOffset)
    {
        if (target == null)
        {
            return false;
        }

        if (!IsSlot(slotNumber))
        {
            Debug.LogWarning($"{slotNumber}번 슬롯 문제");
            return false;
        }

        if (obj[slotNumber] != null)
        {
            Debug.LogWarning($"{slotNumber}번 슬롯 사용 중");
            return false;
        }

        if (slotObj.ContainsKey(target))
        {
            Debug.LogWarning($"{target.name}는 이미 다른 슬롯에 등록되어 있늠");
            return false;
        }

        obj[slotNumber] = target;

        slotObj.Add(target, slotNumber);
        offsetObj.Add(target, localOffset);

        SnapToSlot(target);

        return true;
    }

    public bool Remove(GameObject target)
    {
        if (target == null)
        {
            return false;
        }

        if (!slotObj.TryGetValue(target, out int slotNumber))
        {
            return false;
        }

        obj[slotNumber] = null;

        slotObj.Remove(target);
        offsetObj.Remove(target);

        return true;
    }

    public bool MoveOrSwap(int fromSlot, int toSlot)
    {
        if (!IsSlot(fromSlot) || !IsSlot(toSlot))
        {
            return false;
        }

        GameObject movingObject = obj[fromSlot];

        if (movingObject == null)
        {
            return false;
        }

        if (fromSlot == toSlot)
        {
            SnapToSlot(movingObject);
            return true;
        }

        GameObject targetObject = obj[toSlot];

        obj[toSlot] = movingObject;
        slotObj[movingObject] = toSlot;

        if (targetObject == null)
        {
            obj[fromSlot] = null;
        }
        else
        {
            obj[fromSlot] = targetObject;
            slotObj[targetObject] = fromSlot;

            SnapToSlot(targetObject);
        }

        SnapToSlot(movingObject);

        return true;
    }

    public void SnapToSlot(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (!slotObj.TryGetValue(target, out int slotNumber))
        {
            return;
        }

        Vector3 localOffset = Vector3.zero;

        if (offsetObj.TryGetValue(target, out Vector3 savedOffset))
        {
            localOffset = savedOffset;
        }

        target.transform.position = GetSlotPosition(slotNumber, localOffset);
    }


}