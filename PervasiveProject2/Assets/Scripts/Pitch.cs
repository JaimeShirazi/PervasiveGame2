using System;
using Unity.Collections;
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
        > 11 => GetJustInterval(root * 2.0, semitones - 12)
    };
    public enum Chord
    {
        Maj, Min, Min7, Dom7, Dom9, Sus4, Maj7, Dim, HalfDim
    }
    private const char FLAT = '\uE260';
    private const char SHARP = '\uE262';

    private const char SYMBOL_DIM = '\uE870';
    private const char SYMBOL_HALF_DIM = '\uE871';
    private const char SYMBOL_MAJ7 = '\uE873';
    private static string GetDisplayChordCenter(int offset)
    {
        switch (GlobalWheelState.CurrentTonality)
        {
            case GlobalWheelState.Tonality.Major: default:
                return offset switch
                {
                    < 0  => GetDisplayChordCenter(offset + 12),
                      0  => "I",
                      1  => FLAT + "II",
                      2  => "II",
                      3  => FLAT + "III",
                      4  => "III",
                      5  => "IV",
                      6  => FLAT + "V",
                      7  => "V",
                      8  => FLAT + "VI",
                      9  => "VI",
                      10 => FLAT + "VII",
                      11 => "VII",
                    > 11 => GetDisplayChordCenter(offset - 12)
                };
            case GlobalWheelState.Tonality.Minor:
                return offset switch
                {
                    < 0  => GetDisplayChordCenter(offset + 12),
                      0  => "I",
                      1  => FLAT + "II",
                      2  => "II",
                      3  => "III",
                      4  => SHARP + "III",
                      5  => "IV",
                      6  => FLAT + "V",
                      7  => "V",
                      8  => "VI",
                      9  => SHARP + "VI",
                      10 => "VII",
                      11 => "VII",
                    > 11 => GetDisplayChordCenter(offset - 12)
                };
        }
    }
    public static string GetDisplayChord(int offset, Chord chord)
    {
        return chord switch
        {
            Chord.Sus4 => GetDisplayChordCenter(offset) + "sus4",
            Chord.Min => GetDisplayChordCenter(offset).ToLower(),
            Chord.Dim => GetDisplayChordCenter(offset) + SYMBOL_DIM + "7",
            Chord.HalfDim => GetDisplayChordCenter(offset) + SYMBOL_HALF_DIM + "7",
            Chord.Min7 => GetDisplayChordCenter(offset).ToLower() + "7",
            Chord.Dom7 => GetDisplayChordCenter(offset) + "7",
            Chord.Maj7 => GetDisplayChordCenter(offset) + SYMBOL_MAJ7 + "7",
            Chord.Dom9 => GetDisplayChordCenter(offset) + "9",
            Chord.Maj or _ => GetDisplayChordCenter(offset)
        };
    }
    
    public static int[] GetChordIntervalOffsets(Chord chord) => chord switch
    {
        Chord.Sus4     => new int[] { 0, 5, 7 },
        Chord.Min      => new int[] { 0, 3, 7 },
        Chord.Dim      => new int[] { 0, 3, 6, 9 },
        Chord.HalfDim  => new int[] { 0, 3, 6, 10 },
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
    public static double GetAmplitude(NativeArray<double> frequencies, double time)
    {
        double amplitude = 0;
        for (int i = 0; i < frequencies.Length; i++)
        {
            amplitude += math.sin(2.0 * Math.PI * frequencies[i] * time);

            //taper the high frequencies off by a little
            amplitude *= (math.pow(0.7, i) * 0.5) + 0.5;
        }
        return amplitude / (double)(frequencies.Length + 0.01);
    }
}
