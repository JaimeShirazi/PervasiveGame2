using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pitch Set Collection", menuName = "ScriptableObjects/Pitch Set Collection")]
public class PitchSetCollectionSO : ScriptableObject
{
    public PitchSetSO major, minor, chromatic;
}
