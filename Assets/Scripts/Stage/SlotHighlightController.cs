using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SlotHighlightController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private SlotBoard slotBoard;

    [Header("슬롯 Sprite")]
    [SerializeField] private Sprite availableSprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite enemySprite;

    private SpriteRenderer[] slotSpriteRenderers;

    private int currentTargetSlot = -1;

    private readonly Dictionary<SpriteRenderer, Sprite> enemyOriginalSprites = new();

    private void Awake()
    {
        if (slotBoard == null)
        {
            throw new Exception("SlotHighlightController의 Slot Board가 연결되어 있지 않음");
        }

        if (availableSprite == null)
        {
            throw new Exception("Available Sprite가 연결되어 있지 않음");
        }

        if (selectedSprite == null)
        {
            throw new Exception("Selected Sprite가 연결되어 있지 않음");
        }

        if (emptySprite == null)
        {
            throw new Exception("Empty Sprite가 연결되어 있지 않음");
        }

        if (enemySprite == null)
        {
            throw new Exception("Enemy Sprite가 연결되어 있지 않음");
        }

        CacheSlotSpriteRenderers();
    }

    private void Start()
    {
        Refresh();
    }

    // 모든 플레이어 슬롯을 기본 Available 상태로 되돌림
    public void Refresh()
    {
        currentTargetSlot = -1;

        for (int i = 1; i < slotBoard.SlotCount; i++)
        {
            if (!slotBoard.IsSlot(i))
            {
                continue;
            }

            SetSprite(i, availableSprite);
        }
    }

    // 드래그 시작
    public void StartDragHighlight()
    {
        currentTargetSlot = -1;
    }

    // 현재 마우스가 가리키는 슬롯 변경
    public void SetTargetSlot(int slotNumber, int draggedFromSlot)
    {
        if (currentTargetSlot == slotNumber)
        {
            return;
        }

        if (slotBoard.IsSlot(currentTargetSlot) && currentTargetSlot != draggedFromSlot)
        {
            SetSprite(currentTargetSlot, availableSprite);
        }

        currentTargetSlot = slotNumber;

        if (slotBoard.IsSlot(draggedFromSlot))
        {
            SetSprite(draggedFromSlot, selectedSprite);
        }

        if (!slotBoard.IsSlot(currentTargetSlot))
        {
            return;
        }

        if (currentTargetSlot == draggedFromSlot)
        {
            return;
        }

        SetSprite(currentTargetSlot, emptySprite);
    }
    // 드래그 종료
    public void EndDragHighlight()
    {
        Refresh();
    }

    // 적이 배치된 슬롯을 Enemy Sprite로 변경
    public void SetEnemy(Transform slot)
    {
        if (slot == null)
        {
            return;
        }

        SpriteRenderer spriteRenderer = GetSlotSpriteRenderer(slot);

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"{slot.name} 슬롯에서 SpriteRenderer를 찾을 수 없음");

            return;
        }

        if (!enemyOriginalSprites.ContainsKey(spriteRenderer))
        {
            enemyOriginalSprites.Add(spriteRenderer, spriteRenderer.sprite);
        }

        spriteRenderer.sprite = enemySprite;
    }

    // Enemy 표시를 원래 상태로 복구
    public void ClearEnemyHighlights()
    {
        foreach (KeyValuePair<SpriteRenderer, Sprite> pair in enemyOriginalSprites)
        {
            if (pair.Key == null)
            {
                continue;
            }

            pair.Key.sprite = pair.Value;
        }

        enemyOriginalSprites.Clear();
    }

    // 각 슬롯의 자식 SpriteRenderer를 미리 찾아 저장
    private void CacheSlotSpriteRenderers()
    {
        slotSpriteRenderers = new SpriteRenderer[slotBoard.SlotCount];

        for (int i = 1; i < slotBoard.SlotCount; i++)
        {
            Transform slot = slotBoard.GetSlotTransform(i);

            if (slot == null)
            {
                continue;
            }

            SpriteRenderer spriteRenderer = slot.GetComponentInChildren<SpriteRenderer>(true);

            if (spriteRenderer == null)
            {
                Debug.LogWarning($"{i}번 슬롯에서 SpriteRenderer를 찾을 수 없음");

                continue;
            }

            slotSpriteRenderers[i] = spriteRenderer;
        }
    }

    // 해당 슬롯의 Sprite 변경
    private void SetSprite(int slotNumber, Sprite sprite)
    {
        if (!slotBoard.IsSlot(slotNumber))
        {
            return;
        }

        SpriteRenderer spriteRenderer = slotSpriteRenderers[slotNumber];

        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sprite = sprite;
    }

    // Transform 아래에 있는 SpriteRenderer 검색
    private SpriteRenderer GetSlotSpriteRenderer(Transform slot)
    {
        return slot.GetComponentInChildren<SpriteRenderer>(true);
    }
}