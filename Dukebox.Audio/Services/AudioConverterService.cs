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
        public async Task ConvertCdaFileToMp3(string cdaFileName, string mp3OutFile, BaseEncoder.ENCODEFILEPROC progressCallback, bool overwriteOuputFile)
        {
            EncoderLAME lameEncoder = new EncoderLAME(0);
            lameEncoder.EncoderDirectory = Dukebox.Audio.Properties.Settings.Default.lameEncoderPath;

            lameEncoder.InputFile = @cdaFileName;
            lameEncoder.OutputFile = @mp3OutFile;

            await Task.Run(() =>
            {
                // Extract cda file to wav file on disk.
                string wavFile = lameEncoder.OutputFile + ".wav";

                try
                {
                    WriteCdaFileToWavFile(lameEncoder.InputFile, wavFile, progressCallback);
                }
                catch (Exception ex)
                {
                    if (lameEncoder.EncoderExists)
                    {
                        logger.ErrorFormat("Encoder result: failed\nLast BASS error: ", Bass.BASS_ErrorGetCode().ToString());
                    }

                    logger.Error(ex);
                    return;
                }

                // Call lame encoder with arguments.
                lameEncoder.InputFile = wavFile;
                CallLameEncoder(lameEncoder);

                // Clean up.
                File.Delete(wavFile);
            });      
        }

        public void WriteCdaFileToWavFile(string inCdaFile, string outWavFile, BaseEncoder.ENCODEFILEPROC progressCallback)
        {
            var measuringStream = BassCd.BASS_CD_StreamCreateFile(inCdaFile, BASSFlag.BASS_DEFAULT);

            if (measuringStream == 0)
            {
                return;
            }

            var totalLength = Bass.BASS_ChannelGetLength(measuringStream, BASSMode.BASS_POS_BYTES);
            Bass.BASS_StreamFree(measuringStream);

            var stream = BassCd.BASS_CD_StreamCreateFile(inCdaFile, BASSFlag.BASS_STREAM_DECODE);

            if (stream == 0)
            {
                return;
            }

            try
            {
                var ww = new WaveWriter(outWavFile, stream, true);
                var data = new short[32768];

                var bytesSoFar = 0;

                while (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING)
                {
                    var length = Bass.BASS_ChannelGetData(stream, data, 32768);

                    if (length > 0)
                    {
                        ww.Write(data, length);

                        bytesSoFar += length;

                        progressCallback.Invoke(totalLength, bytesSoFar);
                    }
                }

                // finilize the wave file!
                ww.Close();
                Bass.BASS_StreamFree(stream);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error copying cda file '{0}' to wav file '{1}'", inCdaFile, outWavFile), ex);
            }
        }

        private void CallLameEncoder(EncoderLAME lameEncoder)
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
