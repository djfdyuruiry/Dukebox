using Dukebox.Audio;
using Dukebox.Library.Config;
using Dukebox.Library.Helper;
using System;
using System.Diagnostics;

namespace Dukebox.Tests.Utils
{
    public class BassLoader
    {
        public static DukeboxInitialisationHelper LoadBass()
        {
            var settings = new DukeboxSettings();
            var audioFileFormats = new AudioFileFormats();

            var helper = new DukeboxInitialisationHelper(settings, audioFileFormats);

            try
            {
                helper.InitaliseBassLibrary();
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
