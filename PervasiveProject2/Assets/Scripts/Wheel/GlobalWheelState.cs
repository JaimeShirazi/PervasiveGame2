using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public static class GlobalWheelState
{
    public enum Tonality
    {
        Major, Minor
    }
    public static Tonality CurrentTonality => latest;
    public static bool IsChromatic => chromatic;
    public static ChordSetSO CurrentChordSet => chordSet;

    private static Tonality latest = Tonality.Major;
    private static bool chromatic;
    private static ChordSetSO chordSet;
    public static void SetChromatic(bool state)
    {
        chromatic = state;
    }
    public static void SetChordSet(ChordSetSO target)
    {
        switch (target.associatedTonality)
        {
            case ChordSetSO.AssociatedTonality.Major:
                latest = Tonality.Major;
                break;
            case ChordSetSO.AssociatedTonality.Minor:
                latest = Tonality.Minor;
                break;
        }
        chordSet = target;
    }

    #region Stick values

    public static bool changed;
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
        public int pitchIndex;
        public PitchSetSO pitchSet;
        public ChordSetSO chordSet;

        #region IEquatable
        public override bool Equals(object obj)
        {
            return obj is Values values && Equals(values);
        }

        public bool Equals(Values other)
        {
            return leftHeld == other.leftHeld &&
                   rightHeld == other.rightHeld &&
                   pitchIndex == other.pitchIndex &&
                   pitchSet == other.pitchSet &&
                   chordSet == other.chordSet;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(leftHeld, rightHeld, pitchIndex, pitchSet, chordSet);
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

        private int[] GetNotesAsOffsets()
        {
            int pitch = pitchSet.pitches[pitchIndex];
            int[] degrees = Pitch.GetChordIntervalOffsets(chordSet.GetChord(pitch));
            for (int i = 0; i < degrees.Length; i++)
            {
                degrees[i] += pitch;
            }
            return degrees;
        }
        public bool IsStateEqual(Values other)
        {
            if (Playing != other.Playing) return false;

            int[] thisNotes = GetNotesAsOffsets();
            int[] otherNotes = other.GetNotesAsOffsets();
            if (thisNotes.Length != otherNotes.Length) return false;

            for (int i = 0; i < thisNotes.Length; i++)
            {
                if (thisNotes[i] != otherNotes[i]) return false;
            }

            return true;
        }

        public override string ToString() => string.Format("Playing {0}, Notes {1}", Playing, GetNotesAsOffsets());
    }
    private static void SetValues(Values target)
    {
        Values prev = current;
        current = target;

        if (prev.IsStateEqual(target)) return;

        changed = true;
    }
    public static void Apply()
    {
        if (current.Playing)
        {
            int pitch = current.pitchSet.pitches[current.pitchIndex];
            SoundManager.ReplacePitch(current.chordSet.GetChord(pitch), pitch);
        }
        else
        {
            SoundManager.Stop();
        }
    }
    #endregion
}
