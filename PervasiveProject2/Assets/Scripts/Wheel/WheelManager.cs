using System;
using UnityEngine;

public static class WheelManager
{
    public static Values Current
    {
        get => current;
        set
        {
            if (current != value)
            {
                SetValues(value);
            }
        }
    }
    private static Values current;
    
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
            return leftHeld == other.leftHeld &&
                   rightHeld == other.rightHeld &&
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

        public bool IsStateEqual(Values other)
        {
            return Playing == other.Playing &&
                   rootIntervalOffset == other.rootIntervalOffset &&
                   chord == other.chord;
        }

        public override string ToString() => string.Format("Left(held: {0}, note: {1}), Right(held: {2}, chord: {3})", leftHeld, rootIntervalOffset, rightHeld, (int)chord);
    }

    private static void SetValues(Values target)
    {
        Values prev = current;
        current = target;

        if (prev.IsStateEqual(target)) return;

        if (target.Playing)
        {
            SoundManager.ReplacePitch(target.chord, target.rootIntervalOffset);
        }
        else
        {
            SoundManager.Stop();
        }
    }
}
