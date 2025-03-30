using System;
using UnityEngine;

public abstract class BaseWheelLogic : MonoBehaviour
{
    public enum InputSource
    {
        LeftStick, RightStick
    }
    protected abstract InputSource Source { get; }
    protected Vector2 InputValue => Source switch
    {
        InputSource.RightStick => InputManager.right,
        InputSource.LeftStick or _ => InputManager.left
    };
    protected float Angle
    {
        get
        {
            float clockwiseAngle = -Vector2.SignedAngle(Vector2.up, InputValue);
            if (clockwiseAngle < 0) clockwiseAngle += 360;
            return clockwiseAngle;
        }
    }
    protected int GetSegment(int segments)
    {
        //Determine how many degrees 1 segment is
        float segmentAngle = 360f / segments;

        //Offset the start a little so that segment 1 is smack dab in the middle of (0, 1)
        float offsetAngle = Angle + (segmentAngle * 0.5f);
        if (offsetAngle > 360) offsetAngle -= 360f;

        //Determine the current segment
        return Mathf.FloorToInt(offsetAngle / segmentAngle);
    }

    [SerializeField] private RectTransform knob;

    void Update()
    {
        knob.anchoredPosition = InputValue * 128;

        if (InputValue.sqrMagnitude > 0)
        {
            WheelManager.Current = InputHeld();
        }
        else
        {
            WheelManager.Current = InputReleased();
        }
    }
    protected abstract WheelManager.Values InputHeld();
    protected abstract WheelManager.Values InputReleased();
}
