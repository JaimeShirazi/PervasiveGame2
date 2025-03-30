using System;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public struct WaveData : IEquatable<WaveData>
{
    public double RootFrequency => rootFrequency;
    public NativeArray<double> Frequencies => frequencies;
    public double TimeStart => timeStart;
    public double TimeEnd => timeEnd;

    [ReadOnly] private double rootFrequency;

    private readonly Pitch.Chord chord;

    [ReadOnly] private NativeArray<double> frequencies;
    [ReadOnly] private double timeStart;
    [ReadOnly] private double timeEnd;
    public WaveData(double rootFrequency, Pitch.Chord chord)
    {
        this.rootFrequency = rootFrequency;
        this.chord = chord;

        int[] intervals = Pitch.GetChordIntervalOffsets(chord);
        frequencies = new NativeArray<double>(intervals.Length, Allocator.Persistent);

        for (int i = 0; i < intervals.Length; i++)
        {
            frequencies[i] = Pitch.GetJustInterval(rootFrequency, intervals[i]);
        }

        timeStart = -1; timeEnd = -1;
    }
    public void Start(double timeStart) => this.timeStart = timeStart;
    public void Finish(double timeEnd)
    {
        if (this.timeEnd < 0) this.timeEnd = timeEnd;
    }
    public bool IsCreated => frequencies.IsCreated;
    public void Dispose() => frequencies.Dispose();

    #region IEquatable
    public override bool Equals(object obj)
    {
        return obj is WaveData data && Equals(data);
    }
    public bool Equals(WaveData other)
    {
        return rootFrequency == other.rootFrequency &&
               chord == other.chord &&
               timeStart == other.timeStart &&
               timeEnd == other.timeEnd &&
               IsCreated == other.IsCreated;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(rootFrequency, chord, timeStart, timeEnd, IsCreated);
    }
    public static bool operator ==(WaveData left, WaveData right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(WaveData left, WaveData right)
    {
        return !(left == right);
    }
    #endregion
}
public class WaveCalculator
{
    public const double ATTACK = 0.01;
    public const double RELEASE = 0.1;

    private WaveData data;
    private PredictiveBuffer buffer;
    public struct PredictiveBuffer
    {
        private bool created;
        private int channels, sampleRate;
        private double time;
        private NativeArray<float> queue;
        private JobHandle job;
        public PredictiveBuffer(WaveData data, int length, int channels, int sampleRate, double time)
        {
            created = true;
            this.channels = channels;
            this.sampleRate = sampleRate;
            this.time = time;
            queue = new NativeArray<float>(length, Allocator.TempJob);
            job = Calculate(data, queue, channels, sampleRate, time);
        }
        public bool AppendDataIfCompatible(float[] target, int channels, int sampleRate, double time)
        {
            if (!IsCreated) return false;

            job.Complete();
            if (queue.Length == target.Length &&
                this.channels == channels &&
                this.sampleRate == sampleRate &&
                this.time == time)
            {
                for (int i = 0; i < target.Length; i++)
                {
                    target[i] += queue[i];
                }
                queue.Dispose();
                return true;
            }

            return false;
        }
        public bool IsCreated => created;
        public void Dispose()
        {
            job.Complete();
            if (IsCreated)
                queue.Dispose();
        }
    }

    public void Set(WaveData newData, double time)
    {
        if (data == newData) return;

        if (data.IsCreated)
            data.Dispose();

        data = newData;
        data.Start(time);
    }
    public void Unset(double time) => data.Finish(time);

    public void Calculate(float[] queue, int channels, int sampleRate, double time)
    {
        bool usedBuffer = false;
        if (buffer.IsCreated)
        {
            usedBuffer = buffer.AppendDataIfCompatible(queue, channels, sampleRate, time);
        }

        if (!usedBuffer) //If the buffered data wasn't right or failed, calculate the data now and block the main thread.
        {
            NativeArray<float> nativeQueue = new NativeArray<float>(queue.Length, Allocator.TempJob);
            Calculate(data, nativeQueue, channels, sampleRate, time).Complete();
            for (int i = 0; i < queue.Length; i++)
            {
                queue[i] += nativeQueue[i];
            }
            nativeQueue.Dispose();
        }

        buffer = new(data, queue.Length, channels, sampleRate, time + (queue.Length / (double)sampleRate));
    }
    internal static JobHandle Calculate(WaveData data, NativeArray<float> queue, int channels, int sampleRate, double time)
    {
        CalculateAmplitudes job = new CalculateAmplitudes()
        {
            data = data,
            channels = channels,
            time = time,
            sampleRate = sampleRate,
            output = queue
        };

        JobHandle handle = job.Schedule(queue.Length, 256);

        return handle;
    }
    public bool IsActiveAtTime(double time)
    {
        return (time >= data.TimeStart)
            && (data.TimeStart >= 0)
            && ((time < data.TimeEnd + RELEASE) || data.TimeEnd < 0);
    }
    public void Dispose()
    {
        if (buffer.IsCreated)
            buffer.Dispose();
        data.Dispose();
    }
}
[BurstCompile]
struct CalculateAmplitudes : IJobParallelFor
{
    [ReadOnly] public WaveData data;

    [ReadOnly] public int channels;
    [ReadOnly] public double time;
    [ReadOnly] public int sampleRate;

    public NativeArray<float> output;
    public void Execute(int index)
    {
        int channel = index % channels;
        int sampleIndex = (index - channel) / channels;

        double sampleTime = (time - data.TimeStart) + (sampleIndex / (double)sampleRate);

        double fade = 1;
        if (data.TimeEnd > 0) //note has been released
        {
            double sampleHeldTime = (data.TimeEnd - data.TimeStart);

            fade = math.clamp(1.0 - ((sampleTime - sampleHeldTime) / WaveCalculator.RELEASE), 0.0, 1.0);
        }
        else if (sampleTime < WaveCalculator.ATTACK)
        {
            fade = sampleTime / WaveCalculator.ATTACK;
        }

        double amplitude = Pitch.GetAmplitude(data.Frequencies, sampleTime);

        output[index] = (float)(amplitude * fade);
    }
}