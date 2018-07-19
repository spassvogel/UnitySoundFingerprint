namespace SoundFingerprinting.Audio
{
    using System;
    using System.IO;
    using UnityEngine;

    internal class WaveFormat
    {
        public int SampleRate { get; private set; }

        public short Channels { get; private set; }

        public short BitsPerSample { get; private set; }

        public long Length { get; private set; }

        public float LengthInSeconds
        {
            get
            {
                return (float)Length / (SampleRate * (BitsPerSample / 8) * Channels);
            }
        }

        
        public static WaveFormat FromFile(string pathToFileName)
        {
            byte[] header = new byte[44];
            short channels;
            int sampleRate;
            short bitsPerSample;
            long bytes;

            // TODO: CHECK https://github.com/yasirkula/UnityAndroidStreamingAssets on how to extract StreamingAssets

            // #if UNITY_ANDROID
         
            //     WWW www = new WWW(pathToFileName);
            //     while(!www.isDone) {}
        
            //     channels = (short)(header[22] | header[23] << 8);
            //     sampleRate = header[24] | header[25] << 8 | header[26] << 16 | header[27] << 24;
            //     bitsPerSample = (short)(header[34] | header[35] << 8);
            //     bytes = www.bytes.Length - 44;
                      
            //   #else //PC Platform
            using (var fileStream = new FileStream(pathToFileName, FileMode.Open))
            {
                if (fileStream.Read(header, 0, 44) != 44)
                {
                    throw new ArgumentException(String.Format(
                        "{pathToFileName} is not a valid wav file since it does not contain a header",
                        pathToFileName));
                }

                channels = (short)(header[22] | header[23] << 8);
                sampleRate = header[24] | header[25] << 8 | header[26] << 16 | header[27] << 24;
                bitsPerSample = (short)(header[34] | header[35] << 8);
                bytes = fileStream.Length - 44;
            }
            //#endif
            
            return new WaveFormat
                    {
                        Channels = channels,
                        SampleRate = sampleRate,
                        BitsPerSample = bitsPerSample,
                        Length = bytes
                    };
            
        }

        // public static WaveFormat FromFile2(string pathToFileName)
        // {
        //     byte[] header = new byte[44];
        //     short channels;
        //     int sampleRate;
        //     short bitsPerSample;
        //     long bytes;

        //     #if UNITY_ANDROID
         
        //         WWW www = new WWW (pathToFileName);
        
        //         WaitForSeconds w;
        //         while (!www.isDone)
        //             w = new WaitForSeconds(0.1f);
        
        //         channels = (short)(header[22] | header[23] << 8);
        //         sampleRate = header[24] | header[25] << 8 | header[26] << 16 | header[27] << 24;
        //         bitsPerSample = (short)(header[34] | header[35] << 8);
        //         bytes = www.bytes.Length - 44;

        //         return new WaveFormat
        //                {
        //                    Channels = channels,
        //                    SampleRate = sampleRate,
        //                    BitsPerSample = bitsPerSample,
        //                    Length = bytes
        //                };
                      
        // #else //PC Platform
        //     using (var fileStream = new FileStream(pathToFileName, FileMode.Open))
        //     {
        //         if (fileStream.Read(header, 0, 44) != 44)
        //         {
        //             throw new ArgumentException(String.Format(
        //                 "{pathToFileName} is not a valid wav file since it does not contain a header",
        //                 pathToFileName));
        //         }

        //         channels = (short)(header[22] | header[23] << 8);
        //         sampleRate = header[24] | header[25] << 8 | header[26] << 16 | header[27] << 24;
        //         bitsPerSample = (short)(header[34] | header[35] << 8);
        //         bytes = fileStream.Length - 44;
        //     }
        // #endif
            
        //     return new WaveFormat
        //             {
        //                 Channels = channels,
        //                 SampleRate = sampleRate,
        //                 BitsPerSample = bitsPerSample,
        //                 Length = bytes
        //             };
            
        // }

        public override string ToString()
        {
            return String.Format("Format: SampleRate ${SampleRate}, Channels ${Channels}, BitsPerSample ${BitsPerSample}, Length ${Length}",
                SampleRate,
                Channels,
                BitsPerSample,
                Length);
        }
    }
}
