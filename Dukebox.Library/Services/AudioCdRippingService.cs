using Dukebox.Audio;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using Dukebox.Library.Repositories;
using Dukebox.Library.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Linq;
using Un4seen.Bass.Misc;
using Dukebox.Audio.Interfaces;
using System.Threading.Tasks;

namespace Dukebox.Library.Services
{
    /// <summary>
    /// Contains CD ripping execution and montioring logic.
    /// </summary>
    public class AudioCdRippingService : IAudioCdRippingService
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ICdMetadataService _cdMetadataService;
        private readonly IAudioConverterService _audioConverterService;

        public AudioCdRippingService(ICdMetadataService cdMetadataService, IAudioConverterService audioConverterService)
        {
            _cdMetadataService = cdMetadataService;
            _audioConverterService = audioConverterService;
        }
        
        /// <summary>
        /// Rips an audio CD to a folder as a collection of MP3 files
        /// with relevant metadata encoded in their ID3 tags.
        /// </summary>
        /// <param name="inPath">The CD drive root folder.</param>
        /// <param name="outPath">The folder to save MP3 files to.</param>
        /// <param name="viewUpdater">A ICdRipViewUpdater implementation object that will recieve messages on ripping progress.</param>
        public async Task RipCdToFolder(string inPath, string outPath, ICdRipViewUpdater viewUpdater)
        {
            try
            {
                if (string.IsNullOrEmpty(inPath) || !Directory.Exists(inPath) || string.IsNullOrEmpty(outPath) || !Directory.Exists(outPath))
                {
                    throw new Exception(string.Format("Directory '{0}' does not exist on this system", inPath));
                }

                if (viewUpdater == null)
                {
                    throw new ArgumentNullException("viewUpdater");
                }

                string[] inFiles = Directory.GetFiles(inPath);
                string outFileFormat = outPath + "\\{0}.mp3";
                List<IAudioFileMetadata> cdMetadata = _cdMetadataService.GetAudioFileMetaDataForCd(inPath[0]);
                int numTracks = inFiles.Length;

                viewUpdater.Text = "Dukebox - MP3 Ripping Progress";

                // Rip each track.
                for (int trackIdx = 0; trackIdx < numTracks; trackIdx++)
                {
                    try
                    {
                        ITrack t = Track.BuildTrackInstance(inFiles[trackIdx], cdMetadata[trackIdx]);
                        string outFile = string.Format(outFileFormat, t.ToString());

                        await _audioConverterService.ConvertCdaFileToMp3(inFiles[trackIdx], outFile,
                            new BaseEncoder.ENCODEFILEPROC((a, b) => CdRippingProgressMonitor(viewUpdater, a, b, t, numTracks, trackIdx)), true);

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
                    catch (Exception ex)
                    {
                        string msg = string.Format("Error ripping music track #{0} from Audio CD", trackIdx + 1);

                        logger.Error(msg, ex);
                        MessageBox.Show(msg, "Dukebox - Error Ripping from CD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                viewUpdater.Complete();
            }
            catch (Exception ex)
            {
                string msg = "Error ripping music from Audio CD";

                logger.Error(msg, ex);
                MessageBox.Show(msg, "Dukebox - Error Ripping from CD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Updates the CD rip view with the curent progress.
        /// </summary>
        /// <param name="cdRipViewUpdater">The view updater to notify.</param>
        /// <param name="totalBytesToEncode">The total bytes to encode.</param>
        /// <param name="bytesEncodedSoFar">The bytes ripped so far.</param>
        /// <param name="track">Information for track being ripped.</param>
        /// <param name="totalTracksToRip">Total tracks being ripped.</param>
        /// <param name="currentTrackIndex">The index of the track currently being ripped.</param>
        private void CdRippingProgressMonitor(ICdRipViewUpdater cdRipViewUpdater, long totalBytesToEncode, long bytesEncodedSoFar, 
                                              ITrack track, int totalTracksToRip, int currentTrackIndex)
        {
            if (cdRipViewUpdater == null)
            {
                throw new ArgumentNullException("cdRipViewUpdater");
            }
            else if (track == null)
            {
                throw new ArgumentNullException("track");
            }

            if (cdRipViewUpdater.ProgressBarMaximum != 100)
            {
                cdRipViewUpdater.ResetProgressBar();
                cdRipViewUpdater.ProgressBarMaximum = 100;
            }

            int percentComplete = (int)(bytesEncodedSoFar / (totalBytesToEncode / 100));
            int currentTrackNumber = currentTrackIndex + 1;

            cdRipViewUpdater.ProgressBarValue = percentComplete;
            cdRipViewUpdater.NotificationUpdate(string.Format("[{0}/{1}] Converting '{2}' to MP3 - {3}%", 
                currentTrackNumber, totalTracksToRip, track, percentComplete));
        }

        /// <summary>
        /// Default delegate for value updates.
        /// </summary>
        public delegate void ValueUpdateDelegate();
    }
}
