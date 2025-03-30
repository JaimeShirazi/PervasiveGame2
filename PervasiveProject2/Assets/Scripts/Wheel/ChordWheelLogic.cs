using UnityEngine;

public class ChordWheelLogic : BaseWheelLogic
{
    public ChordSetCollectionSO collection;
    protected override InputSource Source => InputSource.RightStick;
    protected override int Segments => collection.sets.Count;
    protected override GlobalWheelState.Values InputHeld(int segment)
    {
        GlobalWheelState.SetChordSet(collection.sets[segment]);
        return new GlobalWheelState.Values()
        {
            leftHeld = GlobalWheelState.Current.leftHeld,
            rightHeld = true,
            pitchIndex = GlobalWheelState.Current.pitchIndex,
            pitchSet = GlobalWheelState.Current.pitchSet,
            chordSet = GlobalWheelState.CurrentChordSet
        };
    }
    protected override GlobalWheelState.Values InputReleased()
    {
        return new GlobalWheelState.Values()
        {
            leftHeld = GlobalWheelState.Current.leftHeld,
            rightHeld = false,
            pitchIndex = GlobalWheelState.Current.pitchIndex,
            pitchSet = GlobalWheelState.Current.pitchSet,
            chordSet = GlobalWheelState.CurrentChordSet
        };
    }
}
