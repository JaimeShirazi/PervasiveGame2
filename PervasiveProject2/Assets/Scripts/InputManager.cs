using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static Vector2 left, right;

    private InputAction actionLeft, actionRight, actionChromatic;

    private void Awake()
    {
        actionLeft = InputSystem.actions.FindActionMap("Controls").FindAction("Left");
        actionRight = InputSystem.actions.FindActionMap("Controls").FindAction("Right");
        actionChromatic = InputSystem.actions.FindActionMap("Controls").FindAction("Chromatic");
    }

    void OnEnable()
    {
        actionLeft.performed += OnLeftPerformed;
        actionLeft.canceled += OnLeftCanceled;
        actionRight.performed += OnRightPerformed;
        actionRight.canceled += OnRightCanceled;
        actionChromatic.performed += OnChromaticPerformed;
    }
    void OnDisable()
    {
        actionLeft.performed -= OnLeftPerformed;
        actionLeft.canceled -= OnLeftCanceled;
        actionRight.performed -= OnRightPerformed;
        actionRight.canceled -= OnRightCanceled;
        actionChromatic.performed -= OnChromaticPerformed;
    }
    private void OnLeftPerformed(InputAction.CallbackContext context)
    {
        left = context.ReadValue<Vector2>();
    }
    private void OnLeftCanceled(InputAction.CallbackContext context)
    {
        left = Vector2.zero;
    }
    private void OnRightPerformed(InputAction.CallbackContext context)
    {
        right = context.ReadValue<Vector2>();
    }
    private void OnRightCanceled(InputAction.CallbackContext context)
    {
        right = Vector2.zero;
    }
    private void OnChromaticPerformed(InputAction.CallbackContext context)
    {
        GlobalWheelState.SetChromatic(!GlobalWheelState.IsChromatic);
    }
}
