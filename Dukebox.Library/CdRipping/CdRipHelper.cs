using Dukebox.Audio;
using Dukebox.Library.Model;
using Dukebox.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
        private static readonly ILog Logger = 
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /// <summary>
        /// Rips an audio CD to a folder as a collection of MP3 files
        /// with relevant metadata encoded in their ID3 tags.
        /// </summary>
        /// <param name="inPath">The CD drive root folder.</param>
        /// <param name="outPath">The folder to save MP3 files to.</param>
        public void RipCdToFolder(string inPath, string outPath, ICdRipViewUpdater viewUpdater)
        {
            try
            {
                string[] inFiles = Directory.GetFiles(inPath);
                string outFileFormat = outPath + "\\{0}.mp3";
                List<AudioFileMetaData> cdMetadata = CdMetadata.GetAudioFileMetaDataForCd(inPath[0]);
                int maxIdx = inFiles.Length;

                Thread viewerThread = new Thread(delegate()
                {
                    viewUpdater.Text = "Dukebox - MP3 Ripping Progress";
                    viewUpdater.Show();
                    Dispatcher.Run();
                });

                viewerThread.SetApartmentState(ApartmentState.STA); // needs to be STA or throws exception
                viewerThread.Start();

                // Rip each track.
                for (int i = 0; i < maxIdx; i++)
                {
                    Track t = MusicLibrary.GetInstance().GetTrackFromFile(inFiles[i], cdMetadata[i]);
                    string outFile = string.Format(outFileFormat, t.ToString());

                    MediaPlayer.ConvertCdaFileToMp3(inFiles[i], outFile, new BaseEncoder.ENCODEFILEPROC((a, b) => CdRippingProgressMonitor(viewUpdater, a, b, t, maxIdx, i)), true);

                    // Wait until track has been ripped.
                    while (viewUpdater.ProgressBarValue != viewUpdater.ProgressBarMaximum)
                    {
                        Thread.Sleep(10);
                    }
                       
                    // Save the track metadata details to the MP3 file. BROKEN as of rev. 25   
                    /*                 
                    Track mp3Track = MusicLibrary.GetInstance().GetTrackFromFile(outFile);
                    mp3Track.Metadata.Album = t.Metadata.Album;
                    mp3Track.Metadata.Artist = t.Metadata.Artist;
                    mp3Track.Metadata.Title = t.Metadata.Title;

                    mp3Track.Metadata.CommitChangesToFile();
                     */

                    viewUpdater.ResetProgressBar();
                }

                viewUpdater.Invoke(new ValueUpdateDelegate(() => viewUpdater.Hide()));
                viewUpdater.Invoke(new ValueUpdateDelegate(() => viewUpdater.Dispose()));
            }
            catch (Exception ex)
            {
                string msg = "Error ripping music from Audio CD: " + ex.Message;

                Logger.Info(msg);
                MessageBox.Show(msg, "Dukebox - Error Ripping from CD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="totalBytesToEncode"></param>
        /// <param name="bytesEncodedSoFar"></param>
        /// <param name="track"></param>
        private void CdRippingProgressMonitor(ICdRipViewUpdater cdRipViewUpdater, long totalBytesToEncode, long bytesEncodedSoFar, 
                                              Track track, int totalTracksToRip, int currentTrackIndex)
        {
            if (cdRipViewUpdater.ProgressBarMaximum != 100)
            {
                cdRipViewUpdater.ResetProgressBar();
                cdRipViewUpdater.ProgressBarMaximum = 100;
            }

            int percentComplete = (int)(bytesEncodedSoFar / (totalBytesToEncode / 100));
            int currentTrackNumber = currentTrackIndex + 1;

            cdRipViewUpdater.ProgressBarValue = percentComplete;
            cdRipViewUpdater.NotifcationLabelUpdate(string.Format("[{0}/{1}] Converting {2} to MP3 - {3}%", currentTrackNumber, totalTracksToRip, track, percentComplete));
        }

        /// <summary>
        /// Default delegate for value updates in the Dukebox UI.
        /// </summary>
        public delegate void ValueUpdateDelegate();
    }
}
