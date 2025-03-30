using System.Collections.Generic;
using UnityEngine;

public class WheelManager : MonoBehaviour
{
    public List<BaseWheelLogic> wheelLogics = new();
    void Update()
    {
        bool unappliedChanges = false;
        for (int i = 0; i < 999; i++) //avoid infinite loops
        {
            GlobalWheelState.changed = false;
            foreach (BaseWheelLogic wheel in wheelLogics)
            {
                wheel.UpdateGlobalWheelState(); //Since they depend on each other, we're just gonna keep updating the states until there's no changes
            }
            if (!GlobalWheelState.changed) break;
            else unappliedChanges = true;
        }
        if (unappliedChanges)
            GlobalWheelState.Apply();
    }
}
