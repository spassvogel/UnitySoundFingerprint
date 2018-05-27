using SoundFingerprinting.Audio;
using UnityEngine;

public static class AudioClipBridge {

    private static ILowPassFilter lowPassFilter = new LowPassFilter();
    private static IAudioSamplesNormalizer audioSamplesNormalizer = new AudioSamplesNormalizer();

    public static AudioSamples AudioClipToAudioSamples(AudioClip clip, int startAt = 0)
    {
        double seconds = clip.length;
        int sampleRate = 5512;
        var samples = new float[clip.samples];
        clip.GetData(samples, startAt);

        float[] monoSamples = ToMonoSamples(samples, clip);
        float[] downsampled = ToTargetSampleRate(samples, clip.frequency, sampleRate);
        audioSamplesNormalizer.NormalizeInPlace(downsampled);
        
        return new AudioSamples(downsampled, "mic", sampleRate);
    }

    private static float[] ToTargetSampleRate(float[] monoSamples, int sourceSampleRate, int sampleRate)
    {
        return lowPassFilter.FilterAndDownsample(monoSamples, sourceSampleRate, sampleRate);
    }

    private static float[] ToMonoSamples(float[] samples, AudioClip clip)
    {
        if (clip.channels == 1) return samples;

        float[] mono = new float[samples.Length / 2];
        for (int i = 0, k = 0; i < samples.Length - 1; i += 2, k++)
        {
            int left = i;
            int right = i + 1;
            mono[k] = (samples[left] + samples[right]) / 2;
        }

        return mono;
    }
}