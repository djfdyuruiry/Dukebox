using Dukebox.Audio.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Cd;
using Un4seen.Bass.Misc;

namespace Dukebox.Audio.Services
{
    public class AudioConverterService : IAudioConverterService
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_fileName"></param>
        /// <returns></returns>
        public void ConvertCdaFileToMp3(string cdaFileName, string mp3OutFile, BaseEncoder.ENCODEFILEPROC progressCallback, bool overwriteOuputFile)
        {
            EncoderLAME lameEncoder = new EncoderLAME(0);
            lameEncoder.EncoderDirectory = Dukebox.Audio.Properties.Settings.Default.lameEncoderPath;

            lameEncoder.InputFile = @cdaFileName;
            lameEncoder.OutputFile = @mp3OutFile;

            (new Thread(() =>
            {
                // Extract cda file to wav file on disk.
                string wavFile = lameEncoder.OutputFile + ".wav";
                bool conversionResult = WriteCdaToWavFile(lameEncoder.InputFile, wavFile, progressCallback);

                if (!conversionResult && lameEncoder.EncoderExists)
                {
                    string msg = "Encoder result: " + conversionResult + "\nLast BASS error: " + Bass.BASS_ErrorGetCode().ToString();

                    logger.Info(msg);
                    return;
                }

                // Call lame encoder with arguments.
                lameEncoder.InputFile = wavFile;
                CallLameEncoder(lameEncoder);

                // Clean up.
                File.Delete(wavFile);
            })).Start();
        }

        public bool WriteCdaToWavFile(string inCdaFile, string outWavFile, BaseEncoder.ENCODEFILEPROC progressCallback)
        {
            int measuringStream = BassCd.BASS_CD_StreamCreateFile(inCdaFile, BASSFlag.BASS_DEFAULT);
            long totalLength = 0;

            if (measuringStream != 0)
            {
                totalLength = Bass.BASS_ChannelGetLength(measuringStream, BASSMode.BASS_POS_BYTES);
                Bass.BASS_StreamFree(measuringStream);

                int stream = BassCd.BASS_CD_StreamCreateFile(inCdaFile, BASSFlag.BASS_STREAM_DECODE);

                if (stream != 0)
                {
                    try
                    {
                        WaveWriter ww = new WaveWriter(outWavFile, stream, true);
                        short[] data = new short[32768];

                        long bytesSoFar = 0;

                        while (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING)
                        {
                            int length = Bass.BASS_ChannelGetData(stream, data, 32768);

                            if (length > 0)
                            {
                                ww.Write(data, length);

                                bytesSoFar += length;

                                progressCallback.Invoke(totalLength, bytesSoFar);
                            }
                        }

                        // finilize the wave file!
                        ww.Close();
                        return Bass.BASS_StreamFree(stream);
                    }
                    catch (Exception ex)
                    {
                        logger.Info("Error copying cda file '" + inCdaFile + "' to wav file '" + outWavFile + "': " + ex.Message);
                    }
                }
            }

            return false;
        }

        public void CallLameEncoder(EncoderLAME lameEncoder)
        {
            Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.CreateNoWindow = true;

            string lamePath = lameEncoder.EncoderCommandLine.Split(' ')[0];
            pProcess.StartInfo.FileName = lamePath;

            string commandArguments = lameEncoder.EncoderCommandLine.Replace(lamePath, string.Empty);
            pProcess.StartInfo.Arguments = commandArguments;

            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;

            pProcess.StartInfo.WorkingDirectory = Dukebox.Audio.Properties.Settings.Default.lameEncoderPath;
            pProcess.Start();

            //Get program output
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            logger.Info("Output from lame encoding: " + strOutput);

            pProcess.WaitForExit();
        }
    }
}
