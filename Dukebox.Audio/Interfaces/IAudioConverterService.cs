using System;
using System.Threading.Tasks;
using Un4seen.Bass.Misc;

namespace Dukebox.Audio.Interfaces
{
    public interface IAudioConverterService
    {
        Task ConvertCdaFileToMp3(string cdaFileName, string mp3OutFile, BaseEncoder.ENCODEFILEPROC progressCallback, bool overwriteOuputFile);
        void WriteCdaFileToWavFile(string inCdaFile, string outWavFile, BaseEncoder.ENCODEFILEPROC progressCallback);
    }
}
