using UnityEngine;

public class ChordWheelLogic : BaseWheelLogic
{
    protected override InputSource Source => InputSource.RightStick;
    protected override WheelManager.Values InputHeld()
    {
        int segment = GetSegment(7);

        return new WheelManager.Values()
        {
            leftHeld = WheelManager.Current.leftHeld,
            rightHeld = true,
            rootIntervalOffset = WheelManager.Current.rootIntervalOffset,
            chord = (Pitch.Chord)segment
        };
    }
    protected override WheelManager.Values InputReleased()
    {
        return new WheelManager.Values()
        {
            leftHeld = WheelManager.Current.leftHeld,
            rightHeld = false,
            rootIntervalOffset = WheelManager.Current.rootIntervalOffset,
            chord = WheelManager.Current.chord
        };
    }
}
