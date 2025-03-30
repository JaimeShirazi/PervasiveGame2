using System;
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
    private float Angle
    {
        get
        {
            float clockwiseAngle = -Vector2.SignedAngle(Vector2.up, InputValue);
            if (clockwiseAngle < 0) clockwiseAngle += 360;
            return clockwiseAngle;
        }
    }

    private int GetSegment(int segments)
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

        switch (source)
        {
            case InputSource.LeftStick:
                if (InputValue.sqrMagnitude > 0)
                {
                    int segment = GetSegment(7);
                    SetRoot(segment);
                }
                else
                {
                    SetLeftReleased();
                }
                break;
            case InputSource.RightStick:
                if (InputValue.sqrMagnitude > 0)
                {
                    int segment = GetSegment(7);
                    SetChord((Pitch.Chord)segment);
                }
                else
                {
                    SetRightReleased();
                }
                break;
        }
    }

    private static void SetRoot(int rootIntervalOffset)
    {
        WheelManager.Current = new WheelManager.Values()
        {
            leftHeld = true,
            rightHeld = WheelManager.Current.rightHeld,
            rootIntervalOffset = rootIntervalOffset,
            chord = WheelManager.Current.chord
        };
    }
    private static void SetChord(Pitch.Chord chord)
    {
        WheelManager.Current = new WheelManager.Values()
        {
            leftHeld = WheelManager.Current.leftHeld,
            rightHeld = true,
            rootIntervalOffset = WheelManager.Current.rootIntervalOffset,
            chord = chord
        };
    }
    private static void SetLeftReleased()
    {
        WheelManager.Current = new WheelManager.Values()
        {
            leftHeld = false,
            rightHeld = WheelManager.Current.rightHeld,
            rootIntervalOffset = WheelManager.Current.rootIntervalOffset,
            chord = WheelManager.Current.chord
        };
    }
    private static void SetRightReleased()
    {
        WheelManager.Current = new WheelManager.Values()
        {
            leftHeld = WheelManager.Current.leftHeld,
            rightHeld = false,
            rootIntervalOffset = WheelManager.Current.rootIntervalOffset,
            chord = WheelManager.Current.chord
        };
    }
}
