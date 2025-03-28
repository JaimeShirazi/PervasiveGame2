using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    private void Awake()
    {
        instance = this;
    }
    public static void StartChord(Pitch.Chord chord, double root)
    {
        instance.startNext = true;
        int[] semitoneOffsets = Pitch.GetChordIntervalOffsets(chord);
        instance.frequencies = Pitch.GetFrequencies(root, semitoneOffsets);
    }
    public static void StopChord()
    {
        instance.started = -1;
    }

    int sampleRate;

    bool startNext;
    double started = -1;
    double[] frequencies;

    private void Update()
    {
        sampleRate = AudioSettings.outputSampleRate;
    }
    public void OnAudioFilterRead(float[] input, int channels)
    {
        if (startNext)
        {
            started = AudioSettings.dspTime;
            startNext = false;
        }

        if (started < 0) return;

        for (int i = 0; i < input.Length / channels; i++)
        {
            float amplitude = (float)Pitch.GetAmplitude(frequencies, (AudioSettings.dspTime - started) + (i / (double)sampleRate));
            for (int channel = 0; channel < channels; channel++)
            {
                input[(i * channels) + channel] = amplitude;
            }
        }
    }
}
