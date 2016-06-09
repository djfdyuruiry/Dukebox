using System;
using System.Diagnostics;
using Dukebox.Audio;
using Dukebox.Configuration;
using Dukebox.Library.Helper;

namespace Dukebox.Tests.Utils
{
    public class AudioLibraryLoader
    {
        public static DukeboxInitialisationHelper LoadBass()
        {
            var settings = new DukeboxSettings();
            var audioFileFormats = new AudioFileFormats();

            var helper = new DukeboxInitialisationHelper(settings, audioFileFormats);

            try
            {
                helper.InitaliseAudioLibrary();
                helper.RegisterSupportedAudioFileFormats();
                // TODO: load more stuff as required
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return helper;
        }
    }
}
