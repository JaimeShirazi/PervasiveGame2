using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pitch Set", menuName = "ScriptableObjects/Pitch Set")]
public class PitchSetSO : ScriptableObject
{
    public List<int> pitches;
}
