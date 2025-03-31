using System;
using UnityEngine;

public static class GlobalPromptState
{
    public static event Action<int> OnNewPrompt = (_) => { };
    public static void NewPrompt(int slots) => OnNewPrompt.Invoke(slots);
}
