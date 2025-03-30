using UnityEngine;

public class PitchWheelLogic : BaseWheelLogic
{
    protected override InputSource Source => InputSource.LeftStick;
    protected override WheelManager.Values InputHeld()
    {
        int segment = GetSegment(7);

        return new WheelManager.Values()
        {
            leftHeld = true,
            rightHeld = WheelManager.Current.rightHeld,
            rootIntervalOffset = segment,
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
