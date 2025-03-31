using System.Collections.Generic;
using UnityEngine;

public static class GlobalChordState
{
    public enum Source //Assumed to have incremental numbers that descend down to 0
    {
        ChordEditorPreview = 0,
        WheelPlayback = 1
    }
    private static Dictionary<Source, (int, Pitch.Chord)?> sources = new();
    private static int current = -1;
    public static void StartPlaying(int offset, Pitch.Chord chord, Source source)
    {
        if (!sources.ContainsKey(source))
        {
            sources.Add(source, null);
        }

        sources[source] = (offset, chord);

        if ((int)source >= current)
        {
            SoundManager.ReplacePitch(sources[source].Value.Item2, sources[source].Value.Item1);
            //ChordSymbolDisplayUI.Set(sources[source].Value.Item1, sources[source].Value.Item2);
            current = (int)source;
        }
    }
    public static void StopPlaying(Source source)
    {
        if (!sources.ContainsKey(source))
        {
            sources.Add(source, null);
        }

        sources[source] = null;

        if (current == (int)source)
        {
            for (int i = current - 1; i >= 0; i--)
            {
                if (sources.ContainsKey((Source)i))
                {
                    if (sources[(Source)i].HasValue)
                    {
                        SoundManager.ReplacePitch(sources[(Source)i].Value.Item2, sources[(Source)i].Value.Item1);
                        //ChordSymbolDisplayUI.Set(sources[(Source)i].Value.Item1, sources[(Source)i].Value.Item2);
                        current = (int)source;
                        return;
                    }
                }
            }
        }

        current = -1;
        SoundManager.Stop();
        //ChordSymbolDisplayUI.Unset();
    }
}
