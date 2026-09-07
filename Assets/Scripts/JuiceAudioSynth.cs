using UnityEngine;

namespace ChickenRush
{
    /// <summary>Small deterministic placeholder sounds; no downloads or global Random state.</summary>
    public static class JuiceAudioSynth
    {
        public static AudioClip Create(string label, int kind)
        {
            const int rate = 22050;
            float length = kind == 3 ? 8f : kind == 2 ? .55f : .22f;
            var samples = new float[Mathf.RoundToInt(rate * length)];
            var random = new System.Random(61 + kind);
            float phase = 0;
            float[] melody = { 523.25f, 659.25f, 783.99f, 659.25f, 587.33f, 659.25f, 523.25f, 392f,
                523.25f, 659.25f, 783.99f, 1046.5f, 783.99f, 659.25f, 587.33f, 523.25f };
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate;
                float local = kind == 3 ? t % .5f : t;
                float frequency = kind == 0 ? 880 + 880 * t : kind == 1 ? 180 + 1800 * t :
                    kind == 2 ? 180 * Mathf.Exp(-7 * t) : melody[Mathf.Min(15, (int)(t / .5f))];
                phase += 2 * Mathf.PI * frequency / rate;
                float envelope = Mathf.Min(1, local / .006f) * Mathf.Exp(-local * (kind == 3 ? 10 : 12));
                float tone = Mathf.Sin(phase) * .5f + Mathf.Sin(phase * 2) * .12f;
                if (kind == 1 || kind == 2) tone += ((float)random.NextDouble() * 2 - 1) * (kind == 2 ? .5f : .15f);
                samples[i] = tone * envelope * Mathf.Clamp01((length - t) / .02f);
            }
            var clip = AudioClip.Create(label, samples.Length, 1, rate, false); clip.SetData(samples, 0); return clip;
        }
    }
}
