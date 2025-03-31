using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChordEditorLogic : MonoBehaviour
{
    public struct ChordBundle : IEquatable<ChordBundle>
    {
        public bool exists;

        public int Offset;
        public Pitch.Chord Chord;
        public string DisplayName;

        public ChordBundle(int offset, Pitch.Chord chord, string displayName)
        {
            exists = true;
            Offset = offset;
            Chord = chord;
            DisplayName = displayName;
        }

        #region IEquatable
        public override bool Equals(object obj)
        {
            return obj is ChordBundle bundle && Equals(bundle);
        }

        public bool Equals(ChordBundle other)
        {
            return Offset == other.Offset &&
                   Chord == other.Chord;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Offset, Chord);
        }

        public static bool operator ==(ChordBundle left, ChordBundle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ChordBundle left, ChordBundle right)
        {
            return !(left == right);
        }
        #endregion

        public override string ToString()
        {
            if (exists) return DisplayName;
            else return "...";
        }
    }
    [SerializeField] private TMP_Text text;
    private ChordBundle[] current = new ChordBundle[0];
    private int selected = 0;
    private InputAction chordLeft, chordRight, confirm;
    private void Awake()
    {
        chordLeft = InputSystem.actions.FindActionMap("Controls").FindAction("ChordLeft");
        chordRight = InputSystem.actions.FindActionMap("Controls").FindAction("ChordRight");
        confirm = InputSystem.actions.FindActionMap("Controls").FindAction("Confirm");
    }
    private void OnEnable()
    {
        GlobalPromptState.OnNewPrompt += BeginNew;
        chordLeft.Enable();
        chordRight.Enable();
        confirm.Enable();
        chordLeft.started += OnLeftStart;
        chordLeft.canceled += OnLeftStop;
        chordRight.started += OnRightStart;
        chordRight.canceled += OnRightStop;
        confirm.performed += OnConfirm;
    }
    private void OnDisable()
    {
        GlobalPromptState.OnNewPrompt -= BeginNew;
        chordLeft.started -= OnLeftStart;
        chordLeft.canceled -= OnLeftStop;
        chordRight.started -= OnRightStart;
        chordRight.canceled -= OnRightStop;
        confirm.performed -= OnConfirm;
        chordLeft.Disable();
        chordRight.Disable();
        confirm.Disable();
    }
    private enum FirstHold
    {
        Left, Right, None
    }
    private FirstHold heldSource;
    private bool previewingChord;
    void OnLeftStart(InputAction.CallbackContext _)
    {
        if (current.Length <= 0) return;
        if (heldSource != FirstHold.None) return;

        heldSource = FirstHold.Left;

        selected--;
        if (selected < 0) selected += current.Length;
        UpdateDisplay();
        if (current[selected].exists)
        {
            previewingChord = true;
            GlobalChordState.StartPlaying(current[selected].Offset, current[selected].Chord, GlobalChordState.Source.ChordEditorPreview);
        }
    }
    void OnRightStart(InputAction.CallbackContext _)
    {
        if (current.Length <= 0) return;
        if (heldSource != FirstHold.None) return;

        heldSource = FirstHold.Right;

        selected = (selected + 1) % current.Length;
        UpdateDisplay();
        if (current[selected].exists)
        {
            previewingChord = true;
            GlobalChordState.StartPlaying(current[selected].Offset, current[selected].Chord, GlobalChordState.Source.ChordEditorPreview);
        }
    }
    void OnLeftStop(InputAction.CallbackContext _)
    {
        if (heldSource != FirstHold.Left) return;
        
        heldSource = FirstHold.None;
        if (previewingChord)
        {
            previewingChord = false;
            GlobalChordState.StopPlaying(GlobalChordState.Source.ChordEditorPreview);
        }
    }
    void OnRightStop(InputAction.CallbackContext _)
    {
        if (heldSource != FirstHold.Right) return;
        
        heldSource = FirstHold.None;
        if (previewingChord)
        {
            previewingChord = false;
            GlobalChordState.StopPlaying(GlobalChordState.Source.ChordEditorPreview);
        }
    }
    void OnConfirm(InputAction.CallbackContext _)
    {
        (int, Pitch.Chord) values = GlobalWheelState.Current.GetValues();
        current[selected] = new(values.Item1, values.Item2, Pitch.GetDisplayChord(values.Item1, values.Item2));
        UpdateDisplay();
    }
    public void BeginNew(int slots)
    {
        selected = 0;
        current = new ChordBundle[slots];
        UpdateDisplay();
    }
    private void UpdateDisplay()
    {
        text.text = "";
        for (int i = 0; i < current.Length; i++)
        {
            if (i == selected) text.text += "<u>";

            text.text += current[i];

            if (i == selected) text.text += "</u>";

            if (i < current.Length - 1) text.text += ", ";
        }
    }
}
