using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SlotHighlightController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private SlotBoard slotBoard;

    [Header("Highlight 색상")]
    [SerializeField] private Color originalPosColor = new Color(0.25f, 0.45f, 1f, 1f);
    [SerializeField] private Color targetColor = new Color(0.2f, 1f, 0.35f, 1f);
    [SerializeField] private Color swapColor = new Color(1f, 0.75f, 0.15f, 1f);

    [Header("Enemy Highlight")]
    [SerializeField] private Color enemyColor = new Color(1f, 0.2f, 0.2f, 1f);

    private Renderer[] slotRenderers;
    private Color[] normalColors;
    private int[] colorPropertyIds;

    private MaterialPropertyBlock propertyBlock;

    private int currentTargetSlot = -1;

    private readonly HashSet<Renderer> enemyHighlightRenderers = new();

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        if (slotBoard == null)
        {
            throw new Exception("SlotHighlightController의 Slot Board가 연결되어 있지 않습니다.");
        }

        propertyBlock = new MaterialPropertyBlock();

        CacheSlotRenderers();
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        currentTargetSlot = -1;

        for (int i = 1; i < slotBoard.SlotCount; i++)
        {
            if (!slotBoard.IsSlot(i))
            {
                continue;
            }

            if (slotBoard.IsEmpty(i))
            {
                SetNormal(i);
            }
            else
            {
                SetColor(i, originalPosColor);
            }
        }
    }

    public void StartDragHighlight()
    {
        currentTargetSlot = -1;
    }

    public void SetTargetSlot(int slotNumber, int draggedFromSlot)
    {
        if (currentTargetSlot == slotNumber)
        {
            return;
        }

        DragState(currentTargetSlot);

        currentTargetSlot = slotNumber;

        if (!slotBoard.IsSlot(currentTargetSlot))
        {
            return;
        }

        if (currentTargetSlot == draggedFromSlot)
        {
            SetColor(currentTargetSlot, targetColor);
            return;
        }

        if (slotBoard.IsEmpty(currentTargetSlot))
        {
            SetColor(currentTargetSlot, targetColor);
        }
        else
        {
            SetColor(currentTargetSlot, swapColor);
        }
    }

    public void EndDragHighlight()
    {
        Refresh();
    }

    public void SetEnemy(Transform slot)
    {
        if (slot == null)
        {
            return;
        }

        Renderer slotRenderer = GetSlotRenderer(slot);

        if (slotRenderer == null)
        {
            return;
        }

        SetColor(slotRenderer, enemyColor);

        enemyHighlightRenderers.Add(slotRenderer);
    }

    public void ClearEnemyHighlights()
    {
        foreach (Renderer slotRenderer in enemyHighlightRenderers)
        {
            if (slotRenderer == null)
            {
                continue;
            }

            slotRenderer.SetPropertyBlock(null);
        }

        enemyHighlightRenderers.Clear();
    }

    private void DragState(int slotNumber)
    {
        if (!slotBoard.IsSlot(slotNumber))
        {
            return;
        }

        if (slotBoard.IsEmpty(slotNumber))
        {
            SetNormal(slotNumber);
        }
        else
        {
            SetColor(slotNumber, originalPosColor);
        }
    }

    private void CacheSlotRenderers()
    {
        slotRenderers = new Renderer[slotBoard.SlotCount];
        normalColors = new Color[slotBoard.SlotCount];
        colorPropertyIds = new int[slotBoard.SlotCount];

        for (int i = 1; i < slotBoard.SlotCount; i++)
        {
            Transform slot = slotBoard.GetSlotTransform(i);

            if (slot == null)
            {
                continue;
            }

            Renderer slotRenderer = slot.GetComponent<Renderer>();

            if (slotRenderer == null)
            {
                slotRenderer = slot.GetComponentInChildren<Renderer>();
            }

            if (slotRenderer == null)
            {
                Debug.LogWarning($"{i}번 슬롯에서 Renderer를 찾을 수 없습니다.");
                continue;
            }

            slotRenderers[i] = slotRenderer;

            Material material = slotRenderer.sharedMaterial;

            if (material == null)
            {
                continue;
            }

            if (material.HasProperty(BaseColorId))
            {
                colorPropertyIds[i] = BaseColorId;
                normalColors[i] = material.GetColor(BaseColorId);
            }
            else if (material.HasProperty(ColorId))
            {
                colorPropertyIds[i] = ColorId;
                normalColors[i] = material.GetColor(ColorId);
            }
            else
            {
                Debug.LogWarning($"{i}번 슬롯 Material에서 색상 Property를 찾을 수 없습니다.");
            }
        }
    }

    private void SetNormal(int slotNumber)
    {
        SetColor(slotNumber, normalColors[slotNumber]);
    }

    private void SetColor(int slotNumber, Color color)
    {
        if (!slotBoard.IsSlot(slotNumber))
        {
            return;
        }

        Renderer slotRenderer = slotRenderers[slotNumber];

        if (slotRenderer == null)
        {
            return;
        }

        int colorPropertyId = colorPropertyIds[slotNumber];

        if (colorPropertyId == 0)
        {
            return;
        }

        slotRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(colorPropertyId, color);
        slotRenderer.SetPropertyBlock(propertyBlock);
        propertyBlock.Clear();
    }

    private Renderer GetSlotRenderer(Transform slot)
    {
        Renderer slotRenderer = slot.GetComponent<Renderer>();

        if (slotRenderer == null)
        {
            slotRenderer = slot.GetComponentInChildren<Renderer>();
        }

        return slotRenderer;
    }

    private void SetColor(Renderer slotRenderer, Color color)
    {
        Material material = slotRenderer.sharedMaterial;

        if (material == null)
        {
            return;
        }

        int colorPropertyId = 0;

        if (material.HasProperty(BaseColorId))
        {
            colorPropertyId = BaseColorId;
        }
        else if (material.HasProperty(ColorId))
        {
            colorPropertyId = ColorId;
        }

        if (colorPropertyId == 0)
        {
            return;
        }

        slotRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(colorPropertyId, color);
        slotRenderer.SetPropertyBlock(propertyBlock);
        propertyBlock.Clear();
    }
}