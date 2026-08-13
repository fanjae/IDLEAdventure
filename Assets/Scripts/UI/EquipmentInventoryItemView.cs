using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class EquipmentInventoryItemView : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    private static readonly Color SelectedColor = new(0.72f, 1f, 0.72f, 1f);

    private Image backgroundImage;
    private Button selectButton;
    private string instanceId;
    private Action<string> onSelected;
    private bool isListenerRegistered;

    public void Bind(
        EquipmentSO equipment,
        string equipmentInstanceId,
        Sprite fallbackIcon,
        bool isSelected,
        Action<string> selectedCallback)
    {
        EnsureComponents();

        instanceId = equipmentInstanceId;
        onSelected = selectedCallback;

        if (iconImage == null)
        {
            return;
        }

        iconImage.sprite = equipment != null && equipment.Icon != null
            ? equipment.Icon
            : fallbackIcon;
        iconImage.preserveAspect = true;

        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? SelectedColor : Color.white;
        }

        if (selectButton != null)
        {
            selectButton.interactable = equipment != null;
        }
    }

    private void EnsureComponents()
    {
        backgroundImage ??= GetComponent<Image>();
        selectButton ??= GetComponent<Button>();

        if (backgroundImage != null)
        {
            backgroundImage.raycastTarget = true;
        }

        if (selectButton == null)
        {
            return;
        }

        selectButton.targetGraphic = backgroundImage;

        if (!isListenerRegistered)
        {
            selectButton.onClick.AddListener(HandleSelected);
            isListenerRegistered = true;
        }
    }

    private void HandleSelected()
    {
        if (!string.IsNullOrEmpty(instanceId))
        {
            onSelected?.Invoke(instanceId);
        }
    }
}
