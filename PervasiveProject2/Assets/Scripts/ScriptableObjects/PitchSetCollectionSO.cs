using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pitch Set Collection", menuName = "ScriptableObjects/Pitch Set Collection")]
public class PitchSetCollectionSO : ScriptableObject
{
    [SerializeReference]
    public List<PitchSetSO> sets;

    [NonSerialized]
    private int current = 0;

    public PitchSetSO Current => sets[current];
    public void PreviousSet()
    {
        current--;
        if (current < 0) current += sets.Count;
    }
    public void NextSet()
    {
        current++;
        current %= sets.Count;
    }
}
