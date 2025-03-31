using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class ChordSymbolDisplayUI : MonoBehaviour
{
    private static ChordSymbolDisplayUI instance;
    
    private TMP_Text text;

    public static void Set(int offset, Pitch.Chord symbol)
    {
        instance.SetInternal(Pitch.GetDisplayChord(offset, symbol));
    }
    public static void Unset()
    {
        instance.UnsetInternal();
    }

    void Awake()
    {
        instance = this;
        text = GetComponent<TMP_Text>();
    }

    private void SetInternal(string target) => text.text = target;
    private void UnsetInternal() => text.text = "";
}
