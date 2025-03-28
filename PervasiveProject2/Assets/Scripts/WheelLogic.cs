using UnityEngine;

public class WheelLogic : MonoBehaviour
{
    public enum InputSource
    {
        LeftStick, RightStick
    }

    public InputSource source = InputSource.LeftStick;

    private Vector2 InputValue => source switch
    {
        InputSource.RightStick => InputManager.right,
        InputSource.LeftStick or _ => InputManager.left
    };

    [SerializeField] private RectTransform knob;

    void Update()
    {
        knob.anchoredPosition = InputValue * 128;
    }
}
