using System;
using Unity.Mathematics;
using UnityEngine;

public static class Pitch
{
    public static double GetJustInterval(double root, int semitones) => semitones switch
    {
        < 0  => GetJustInterval(root * 0.5, semitones + 12),
          0  =>                 root, //Perfect unison
          1  => (16.0 / 15.0) * root, //Minor second
          2  => (9.0  / 8.0)  * root, //Major second
          3  => (6.0  / 5.0)  * root, //Minor third
          4  => (5.0  / 4.0)  * root, //Major third
          5  => (4.0  / 3.0)  * root, //Perfect fourth
          6  => (64.0 / 45.0) * root, //Diminished fifth
          7  => (3.0  / 2.0)  * root, //Perfect fifth
          8  => (8.0  / 5.0)  * root, //Minor sixth
          9  => (5.0  / 3.0)  * root, //Major sixth
          10 => (9.0  / 5.0)  * root, //Minor seventh
          11 => (15.0 / 8.0)  * root, //Major seventh
          12 => 2.0           * root, //Perfect octave
        > 12 => GetJustInterval(root * 2.0, semitones - 12)
    };
    public enum Chord
    {
        Sus4, Maj, Min, Min7, Dom7, Maj7, Dom9
    }
    public static int[] GetChordIntervalOffsets(Chord chord) => chord switch
    {
        Chord.Sus4     => new int[] { 0, 5, 7 },
        Chord.Min      => new int[] { 0, 3, 7 },
        Chord.Min7     => new int[] { 0, 3, 7, 10 },
        Chord.Dom7     => new int[] { 0, 4, 7, 10 },
        Chord.Maj7     => new int[] { 0, 4, 7, 11 },
        Chord.Dom9     => new int[] { 0, 4, 7, 10, 14 },
        Chord.Maj or _ => new int[] { 0, 4, 7 }
    };
    public static double[] GetFrequencies(double root, int[] semitones)
    {
        double[] frequencies = new double[semitones.Length];
        for (int i = 0; i < frequencies.Length; i++)
        {
            frequencies[i] = GetJustInterval(root, semitones[i]);
        }
        return frequencies;
    }
    public static double GetAmplitude(double[] frequencies, double time)
    {
        double amplitude = 0;
        for (int i = 0; i < frequencies.Length; i++)
        {
            amplitude += Math.Sin(2.0 * Math.PI * frequencies[i] * time);
        }
        return amplitude * 0.2;
    }
}
