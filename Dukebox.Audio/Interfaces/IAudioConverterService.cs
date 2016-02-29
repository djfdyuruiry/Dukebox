using System;
using Un4seen.Bass.Misc;

namespace Dukebox.Audio.Interfaces
{
    public interface IAudioConverterService
    {
        void CallLameEncoder(EncoderLAME lameEncoder);
        void ConvertCdaFileToMp3(string cdaFileName, string mp3OutFile, BaseEncoder.ENCODEFILEPROC progressCallback, bool overwriteOuputFile);
        bool WriteCdaToWavFile(string inCdaFile, string outWavFile, BaseEncoder.ENCODEFILEPROC progressCallback);
    }
}
