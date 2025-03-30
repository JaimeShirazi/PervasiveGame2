using UnityEngine;

public class PitchWheelLogic : BaseWheelLogic
{
    public PitchSetCollectionSO collection;
    protected override InputSource Source => InputSource.LeftStick;
    protected override WheelManager.Values InputHeld()
    {
        int segment = GetSegment(collection.Current.pitches.Count);
        int pitchOffset = collection.Current.pitches[segment];

        return new WheelManager.Values()
        {
            leftHeld = true,
            rightHeld = WheelManager.Current.rightHeld,
            rootIntervalOffset = pitchOffset,
            chord = WheelManager.Current.chord
        };
    }
    protected override WheelManager.Values InputReleased()
    {
        return new WheelManager.Values()
        {
            leftHeld = false,
            rightHeld = WheelManager.Current.rightHeld,
            rootIntervalOffset = WheelManager.Current.rootIntervalOffset,
            chord = WheelManager.Current.chord
        };
    }
}
