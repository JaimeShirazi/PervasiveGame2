using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private LineRenderer line;

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
    float[] recentIndexes;

    bool startNext;
    double started = -1;
    double[] frequencies;

    private void Update()
    {
        sampleRate = AudioSettings.outputSampleRate;

        if (recentIndexes == null) return;
        line.positionCount = recentIndexes.Length;
        for (int i = 0; i < recentIndexes.Length; i++)
        {
            float horizontal = i / (float)recentIndexes.Length;
            horizontal *= 2f;
            horizontal -= 1f;
            horizontal *= 4f;
            line.SetPosition(i, new Vector3(horizontal, recentIndexes[i], 0));
        }
    }
    public void OnAudioFilterRead(float[] input, int channels)
    {
        if (startNext)
        {
            started = AudioSettings.dspTime;
            startNext = false;
        }

        if (started < 0) return;

        recentIndexes = new float[input.Length];

        float minRange = float.MaxValue;
        float maxRange = float.MinValue;
        for (int i = 0; i < input.Length / channels; i++)
        {
            double time = (AudioSettings.dspTime - started) + (i / (double)sampleRate);
            float amplitude = (float)Pitch.GetAmplitude(frequencies, time);

            if ((float)time < minRange)
                minRange = (float)time;
            if ((float)time > maxRange)
                maxRange = (float)time;

            for (int channel = 0; channel < channels; channel++)
            {
                input[(i * channels) + channel] = amplitude;
                recentIndexes[(i * channels) + channel] = amplitude;
            }
        }

        Debug.Log(string.Format("({0}, {1})", minRange, maxRange));
    }
}
