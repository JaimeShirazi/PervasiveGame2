using System;
using UnityEngine;

public static class ScoreCounter
{
    private static int pinkScore = 0;
    private static int purpleScore = 0;
    public static void IncrementPink()
    {
        if (!pinkActive) return;
        pinkScore++;
        OnPinkScore.Invoke(pinkScore);
    }
    public static void IncrementPurple()
    {
        if (!purpleActive) return;
        purpleScore++;
        OnPurpleScore.Invoke(purpleScore);
    }
    public static bool pinkActive;
    public static bool purpleActive;
    public static void Reset()
    {
        pinkScore = 0;
        purpleScore = 0;
    }
    public static event Action<int> OnPinkScore = (_) => { };
    public static event Action<int> OnPurpleScore = (_) => { };
}
