using Dukebox.Audio;
using Dukebox.Library.Model;
using Dukebox.Logging;
using Dukebox.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Un4seen.Bass.Misc;

namespace Dukebox.Library.CdRipping
{
    /// <summary>
    /// Contains CD ripping execution and montioring logic.
    /// </summary>
    public class CdRipHelper
    {
        /// <summary>
        /// Rips an audio CD to a folder as a collection of MP3 files
        /// with relevant metadata encoded in their ID3 tags.
        /// </summary>
        /// <param name="inPath">The CD drive root folder.</param>
        /// <param name="outPath">The folder to save MP3 files to.</param>
        public void RipCdToFolder(string inPath, string outPath)
        {
            try
            {
                string[] inFiles = Directory.GetFiles(inPath);
                string outFileFormat = outPath + "\\{0}.mp3";
                List<AudioFileMetaData> cdMetadata = CdMetadata.GetAudioFileMetaDataForCd(inPath[0]);
                int maxIdx = inFiles.Length;

                ProgressMonitorBox progressWindow = new ProgressMonitorBox();

                Thread viewerThread = new Thread(delegate()
                {
                    progressWindow = new ProgressMonitorBox();
                    progressWindow.Text = "Dukebox - MP3 Ripping Progress";
                    progressWindow.Show();
                    Dispatcher.Run();
                });

                viewerThread.SetApartmentState(ApartmentState.STA); // needs to be STA or throws exception
                viewerThread.Start();

                // Rip each track.
                for (int i = 0; i < maxIdx; i++)
                {
                    Track t = MusicLibrary.GetInstance().GetTrackFromFile(inFiles[i], cdMetadata[i]);
                    string outFile = string.Format(outFileFormat, t.ToString());

                    MediaPlayer.ConvertCdaFileToMp3(inFiles[i], outFile, new BaseEncoder.ENCODEFILEPROC((a, b) => CdRippingProgressMonitor(progressWindow, a, b, t, maxIdx, i)), true);

                    // Wait until track has been ripped.
                    while (progressWindow.ProgressBarValue != progressWindow.ProgressBarMaximum)
                    {
                        Thread.Sleep(10);
                    }
                       
                    // Save the track metadata details to the MP3 file.
                    /*
                    Track mp3Track = MusicLibrary.GetInstance().GetTrackFromFile(outFile);
                    mp3Track.Metadata.Album = t.Metadata.Album;
                    mp3Track.Metadata.Artist = t.Metadata.Artist;
                    mp3Track.Metadata.Title = t.Metadata.Title;

                    mp3Track.Metadata.CommitChangesToFile();*/

                    progressWindow.ResetProgressBar();
                }

                progressWindow.Invoke(new ValueUpdateDelegate(()=>progressWindow.Hide()));
                progressWindow.Invoke(new ValueUpdateDelegate(()=>progressWindow.Dispose()));
                progressWindow = null;
            }
            catch (Exception ex)
            {
                string msg = "Error ripping music from Audio CD: " + ex.Message;

                Logger.log(msg);
                MessageBox.Show(msg, "Dukebox - Error Ripping from CD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="totalBytesToEncode"></param>
        /// <param name="bytesEncodedSoFar"></param>
        /// <param name="track"></param>
        private void CdRippingProgressMonitor(ProgressMonitorBox progressWindow, long totalBytesToEncode, long bytesEncodedSoFar, Track track, int totalTracksToRip, int currentTrackIndex)
        {
            if (progressWindow.ProgressBarMaximum != 100)
            {
                progressWindow.ResetProgressBar();
                progressWindow.ProgressBarMaximum = 100;
            }

            int percentComplete = (int)(bytesEncodedSoFar / (totalBytesToEncode / 100));

            progressWindow.ProgressBarValue = percentComplete;
            progressWindow.NotifcationLabelUpdate("[" + currentTrackIndex + 1 + "/" + totalTracksToRip + "] Converting " + track + " to MP3 - " + percentComplete + "%");
        }

        /// <summary>
        /// Default delegate for value updates in the Dukebox UI.
        /// </summary>
        public delegate void ValueUpdateDelegate();
    }
}
