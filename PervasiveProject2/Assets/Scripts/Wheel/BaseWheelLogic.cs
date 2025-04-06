using System;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public abstract class BaseWheelLogic : MonoBehaviour
{
    [SerializeField] private LilypadVisual visual;
    public enum InputSource
    {
        LeftStick, RightStick
    }
    protected abstract InputSource Source { get; }
    protected abstract int Segments { get; }
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
    public void UpdateGlobalWheelState()
    {
        switch (visual.Colour)
        {
            case LilypadVisual.FrogColour.Pink:
                ScoreCounter.pinkActive = InputValue.sqrMagnitude > 0;
                break;
            case LilypadVisual.FrogColour.Purple:
                ScoreCounter.purpleActive = InputValue.sqrMagnitude > 0;
                break;
        }
        if (InputValue.sqrMagnitude > 0)
        {
            int segment = GetSegment(Segments);
            visual.SetSegment(segment);
            GlobalWheelState.Current = InputHeld(segment);
        }
        else
        {
            GlobalWheelState.Current = InputReleased();
            visual.SetSegment(-1);
        }
    }
    protected abstract GlobalWheelState.Values InputHeld(int segment);
    protected abstract GlobalWheelState.Values InputReleased();
}
