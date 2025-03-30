using UnityEngine;

public class PitchWheelLogic : BaseWheelLogic
{
    public PitchSetCollectionSO collection;
    protected override InputSource Source => InputSource.LeftStick;
    private PitchSetSO Current
    {
        get
        {
            if (GlobalWheelState.IsChromatic) return collection.chromatic;
            else
            {
                switch (GlobalWheelState.CurrentTonality)
                {
                    case GlobalWheelState.Tonality.Major: default:
                        return collection.major;
                    case GlobalWheelState.Tonality.Minor:
                        return collection.minor;
                }
            }
        }
    }
    protected override int Segments => Current.pitches.Count;
    protected override GlobalWheelState.Values InputHeld(int segment)
    {
        return new GlobalWheelState.Values()
        {
            leftHeld = true,
            rightHeld = GlobalWheelState.Current.rightHeld,
            pitchIndex = segment,
            pitchSet = Current,
            chordSet = GlobalWheelState.Current.chordSet
        };
    }
    protected override GlobalWheelState.Values InputReleased()
    {
        return new GlobalWheelState.Values()
        {
            leftHeld = false,
            rightHeld = GlobalWheelState.Current.rightHeld,
            pitchIndex = GlobalWheelState.Current.pitchIndex,
            pitchSet = Current,
            chordSet = GlobalWheelState.Current.chordSet
        };
    }
}
