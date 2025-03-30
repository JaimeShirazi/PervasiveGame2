using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Chord Set Collection", menuName = "ScriptableObjects/Chord Set Collection")]
public class ChordSetCollectionSO : ScriptableObject
{
    [SerializeReference]
    public List<ChordSetSO> sets;
}
