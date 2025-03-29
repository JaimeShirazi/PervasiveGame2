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
                    WheelManager.SetRoot(segment);
                }
                else
                {
                    WheelManager.SetLeftReleased();
                }
                break;
            case InputSource.RightStick:
                if (InputValue.sqrMagnitude > 0)
                {
                    int segment = GetSegment(7);
                    WheelManager.SetChord((Pitch.Chord)segment);
                }
                else
                {
                    WheelManager.SetRightReleased();
                }
                break;
        }
    }

    private static class WheelManager
    {
        private static Values current;
        public static Values Current
        {
            set
            {
                if (current != value)
                {
                    SetValues(current);
                }
            }
        }
        private static void SetValues(Values target)
        {
            if (target.Playing)
            {
                SoundManager.ReplacePitch(target.chord, target.rootIntervalOffset);
            }
            else
            {
                SoundManager.Stop();
            }
            current = target;
        }
        public struct Values : IEquatable<Values>
        {
            public bool leftHeld;
            public bool rightHeld;
            public bool Playing => leftHeld && rightHeld;
            public int rootIntervalOffset;
            public Pitch.Chord chord;

            #region IEquatable
            public override bool Equals(object obj)
            {
                return obj is Values values && Equals(values);
            }

            public bool Equals(Values other)
            {
                return Playing == other.Playing &&
                       rootIntervalOffset == other.rootIntervalOffset &&
                       chord == other.chord;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Playing, rootIntervalOffset, chord);
            }

            public static bool operator ==(Values left, Values right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Values left, Values right)
            {
                return !(left == right);
            }
            #endregion

            public override string ToString() => string.Format("Left(held: {0}, note: {1}), Right(held: {2}, chord: {3})", leftHeld, rootIntervalOffset, rightHeld, (int)chord);
        }
        public static void SetRoot(int rootIntervalOffset)
        {
            Current = new Values()
            {
                leftHeld = true,
                rightHeld = current.rightHeld,
                rootIntervalOffset = rootIntervalOffset,
                chord = current.chord
            };
        }
        public static void SetChord(Pitch.Chord chord)
        {
            Current = new Values()
            {
                leftHeld = current.leftHeld,
                rightHeld = true,
                rootIntervalOffset = current.rootIntervalOffset,
                chord = chord
            };
        }
        public static void SetLeftReleased()
        {
            Current = new Values()
            {
                leftHeld = false,
                rightHeld = current.rightHeld,
                rootIntervalOffset = current.rootIntervalOffset,
                chord = current.chord
            };
        }
        public static void SetRightReleased()
        {
            Current = new Values()
            {
                leftHeld = current.leftHeld,
                rightHeld = false,
                rootIntervalOffset = current.rootIntervalOffset,
                chord = current.chord
            };
        }
    }
}
