using UnityEngine;

[CreateAssetMenu(fileName = "Chord Set", menuName = "ScriptableObjects/Chord Set")]
public class ChordSetSO : ScriptableObject
{
    public string label;
    public enum AssociatedTonality
    {
        Major, Minor, Latest
    }
    public AssociatedTonality associatedTonality = AssociatedTonality.Latest;
    public Pitch.Chord chord1, chord2, chord3, chord4, chord5, chord6, chord7, chord8, chord9, chord10, chord11, chord12;
    public Pitch.Chord GetChord(int index) => index switch
    {
        < 0  => GetChord(index + 12),
          0  => chord1,
          1  => chord2,
          2  => chord3,
          3  => chord4,
          4  => chord5,
          5  => chord6,
          6  => chord7,
          7  => chord8,
          8  => chord9,
          9  => chord10,
          10 => chord11,
          11 => chord12,
        > 11 => GetChord(index - 12)
    };
}
