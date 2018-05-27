using SoundFingerprinting.Audio;
using UnityEngine;

public static class AudioClipBridge {

    private static ILowPassFilter lowPassFilter = new LowPassFilter();
    private static IAudioSamplesNormalizer audioSamplesNormalizer = new AudioSamplesNormalizer();


    public static AudioSamples AudioClipToAudioSamples(AudioClip clip, int startAt = 0)
    {
        double seconds = clip.length;
        int sampleRate = clip.frequency;
        var samples = new float[clip.samples];
        clip.GetData(samples, startAt);

        //float[] samples = ToSamples(clip, format, seconds, startAt);
        float[] monoSamples = ToMonoSamples(samples, clip);
        // float[] downsampled = ToTargetSampleRate(samples, sampleRate, sampleRate);
        float[] downsampled = samples;
        audioSamplesNormalizer.NormalizeInPlace(downsampled);
        
        return new AudioSamples(downsampled, "mic", sampleRate);
    }

    // private float[] ToSamples(AudioClip clip, double seconds, double startsAt)
    // {
    //     // using (var stream = new FileStream(pathToFile, FileMode.Open))
    //     // {
    //     //     stream.Seek(waveHeaderLength, SeekOrigin.Begin);
    //     //     int samplesToSeek = (int)(startsAt * format.SampleRate * format.Channels);
    //     //     int bytesPerSample = format.BitsPerSample / 8;
    //     //     stream.Seek(bytesPerSample * samplesToSeek, SeekOrigin.Current);
    //     //     return GetInts(stream, format, seconds, startsAt);
    //     // }
    // }
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
        // public override float GetLengthInSeconds(string pathToSourceFile)
        // {
        //     var format = WaveFormat.FromFile(pathToSourceFile);
        //     CheckInputFileFormat(format, 0);
        //     return format.LengthInSeconds;
        // }

        // public override IReadOnlyCollection<string> SupportedFormats
        // {
        //     get
        //     {
        //         return new[] { ".wav" };
        //     }
        // }

        // private static void CheckInputFileFormat(WaveFormat format, double startsAt)
        // {
        //     if (!AcceptedSampleRates.Contains(format.SampleRate))
        //         throw new ArgumentException(String.Format(
        //             "Sample rate of the given file is not supported {0}. Supported sample rates {1}. "
        //             + "Submit a github request if you need a different sample rate to be supported.",
        //                 format,
        //                 AcceptedSampleRates.ToString())
        //             );
        //     if (!AcceptedBitsPerSample.Contains(format.BitsPerSample))
        //         throw new ArgumentException(String.Format(
        //             "Bad file format {format}. Bits per sample ({format.BitsPerSample}) is less than accepted range.",
        //             format,
        //             format.BitsPerSample
        //         ));
        //     if (!AcceptedChannels.Contains(format.Channels))
        //         throw new ArgumentException(String.Format(
        //             "Bad file format {format}. Number of channels is not in the accepted range.",
        //             format));
        //     if (startsAt > format.LengthInSeconds)
        //         throw new ArgumentException(String.Format(
        //             "Could not start reading from {startsAt} second, since input file is shorter {format.LengthInSeconds}",
        //             startsAt,
        //             format.LengthInSeconds));
        // }
        
        // private float[] ToSamples(string pathToFile, WaveFormat format, double seconds, double startsAt)
        // {
        //     using (var stream = new FileStream(pathToFile, FileMode.Open))
        //     {
        //         stream.Seek(waveHeaderLength, SeekOrigin.Begin);
        //         int samplesToSeek = (int)(startsAt * format.SampleRate * format.Channels);
        //         int bytesPerSample = format.BitsPerSample / 8;
        //         stream.Seek(bytesPerSample * samplesToSeek, SeekOrigin.Current);
        //         return GetInts(stream, format, seconds, startsAt);
        //     }
        // }

        // private float[] ToMonoSamples(float[] samples, WaveFormat format)
        // {
        //     if (format.Channels == 1) return samples;

        //     float[] mono = new float[samples.Length / 2];
        //     for (int i = 0, k = 0; i < samples.Length - 1; i += 2, k++)
        //     {
        //         int left = i;
        //         int right = i + 1;
        //         mono[k] = (samples[left] + samples[right]) / 2;
        //     }

        //     return mono;
        // }


        // private float[] GetInts(Stream reader, WaveFormat format, double seconds, double startsAt)
        // {
        //     int bytesPerSample = format.BitsPerSample / 8;
        //     long samplesCount = GetSamplesToRead(format, seconds, startsAt);

        //     byte[] buffer = new byte[bytesPerSample];

        //     int normalizer = bytesPerSample == 1 ? 127 : bytesPerSample == 2 ? Int16.MaxValue : bytesPerSample == 3 ? (int)Math.Pow(2, 24) / 2 - 1 : Int32.MaxValue;

        //     int samplesOffset = 0;
        //     float[] samples = new float[samplesCount];
        //     while (reader.CanRead && samplesOffset < samplesCount)
        //     {
        //         int read = reader.Read(buffer, 0, bytesPerSample);
        //         if (read != bytesPerSample) return samples;

        //         if (bytesPerSample == 1)
        //         {
        //             samples[samplesOffset] = (float) buffer[0] / normalizer;
        //         }
        //         else if (bytesPerSample == 2)
        //         {
        //             short sample = (short)(buffer[0] | buffer[1] << 8);
        //             samples[samplesOffset] = (float)sample / normalizer;
        //         }
        //         else if (bytesPerSample == 3)
        //         {
        //             int sample = buffer[0] | buffer[1] << 8 | buffer[2] << 16;
        //             samples[samplesOffset] = (float)sample / normalizer;
        //         }
        //         else
        //         {
        //             int sample = buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24;
        //             samples[samplesOffset] = (float)sample / normalizer;
        //         }

        //         samples[samplesOffset] = Math.Min(1, samples[samplesOffset]);
        //         samplesOffset++;
        //     }

        //     return samples;
        // }

        // private long GetSamplesToRead(WaveFormat format, double seconds, double startsAt)
        // {
        //     int samplesPerSecond = format.SampleRate * format.Channels;
        //     int requestedSamples = Math.Abs(seconds) < 0.001 ? int.MaxValue : (int)seconds * samplesPerSecond;
        //     int bytesPerSample = format.BitsPerSample / 8;
        //     int samplesInInput = (int)format.Length / bytesPerSample - (int)startsAt * samplesPerSecond;

        //     if (samplesInInput > requestedSamples)
        //     {
        //         return requestedSamples;
        //     }

        //     return samplesInInput;
        // }