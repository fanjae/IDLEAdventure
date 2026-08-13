using UnityEngine;
using UnityEngine.InputSystem;

public sealed class SlotInput : MonoBehaviour
{
    private InputAction pointerPositionAction;
    private InputAction pointerPressAction;

    public Vector2 PointerPosition
    {
        get
        {
            return pointerPositionAction != null ? pointerPositionAction.ReadValue<Vector2>() : Vector2.zero;
        }
    }

    private void Awake()
    {
        pointerPositionAction = new InputAction(
            "SlotPointerPosition",
            InputActionType.PassThrough,
            "<Pointer>/position"
        );

        pointerPressAction = new InputAction(
            "SlotPointerPress",
            InputActionType.Button,
            "<Pointer>/press"
        );
    }

    private void OnEnable()
    {
        pointerPositionAction?.Enable();
        pointerPressAction?.Enable();
    }

    private void OnDisable()
    {
        pointerPositionAction?.Disable();
        pointerPressAction?.Disable();
    }

    private void OnDestroy()
    {
        pointerPositionAction?.Dispose();
        pointerPressAction?.Dispose();
    }

    public bool WasPressedThisFrame()
    {
        return pointerPressAction != null && pointerPressAction.WasPressedThisFrame();
    }

    public bool IsPressed()
    {
        return pointerPressAction != null && pointerPressAction.IsPressed();
    }

    public bool WasReleasedThisFrame()
    {
        return pointerPressAction != null && pointerPressAction.WasReleasedThisFrame();
    }
}