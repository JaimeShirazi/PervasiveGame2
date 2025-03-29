using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private struct WanderingPitchCenter
    {
        private int rootIntervalOffset;
        private double pitch;
        public double SetNewRoot(int interval)
        {
            double newPitch = Pitch.GetJustInterval(pitch, interval - rootIntervalOffset);
            pitch = newPitch;
            rootIntervalOffset = interval;
            return newPitch;
        }
        public static WanderingPitchCenter Default => new WanderingPitchCenter()
        {
            rootIntervalOffset = 0,
            pitch = 440.0
        };
    }

    private WanderingPitchCenter pitch = WanderingPitchCenter.Default;

    private List<WaveData> pendingWaveData = new();
    private List<WaveCalculator> activeWaves = new();

    private List<WaveCalculator> pendingWaveEnds = new();

    private static SoundManager instance;
    private void Awake()
    {
        instance = this;
    }
    public static void ReplacePitch(Pitch.Chord chord, int rootIntervalOffset)
    {
        Stop();
        double targetPitch = instance.pitch.SetNewRoot(rootIntervalOffset);
        WaveData newData = new WaveData(targetPitch, chord);
        instance.pendingWaveData.Add(newData);
    }
    public static void Stop()
    {
        instance.pendingWaveData.Clear();
        instance.pendingWaveEnds.AddRange(instance.activeWaves);
    }

    int sampleRate;

    private void Update()
    {
        sampleRate = AudioSettings.outputSampleRate;
    }
    public void OnAudioFilterRead(float[] input, int channels)
    {
        //Begin waves that are pending
        for (int i = 0; i < pendingWaveData.Count; i++)
        {
            //TODO: Replace all of this when WaveCalculator is pooled instead of instantiated/destroyed
            activeWaves.Insert(0, new WaveCalculator());
            activeWaves[0].Set(pendingWaveData[i], AudioSettings.dspTime);
        }
        pendingWaveData.Clear();

        //End waves that are pending to end
        for (int i = 0; i < pendingWaveEnds.Count; i++)
        {
            pendingWaveEnds[i].Unset(AudioSettings.dspTime);
        }
        pendingWaveEnds.Clear();

        //Remove waves that have completely released
        //TODO: Replace all of this when WaveCalculator is pooled instead of instantiated/destroyed
        foreach (WaveCalculator waveCalc in activeWaves.Where(calculator => !calculator.IsActiveAtTime(AudioSettings.dspTime)))
        {
            waveCalc.Dispose();
        }
        activeWaves.RemoveAll(calculator => !calculator.IsActiveAtTime(AudioSettings.dspTime));

        //Calculate the amplitudes
        for (int i = 0; i < activeWaves.Count; i++)
        {
            activeWaves[i].Calculate(input, channels, sampleRate, AudioSettings.dspTime);
        }
    }
}
