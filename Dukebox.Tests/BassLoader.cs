﻿using Dukebox.Audio;
using Dukebox.Library.Config;
using Dukebox.Library.Helper;
using Dukebox.Library.Interfaces;
using FakeItEasy;
using System;
using System.Diagnostics;

namespace Dukebox.Tests
{
    public class BassLoader
    {
        public static void LoadBass()
        {
            var settings = new DukeboxSettings();
            var audioFileFormats = new AudioFileFormats();

            var helper = new DukeboxInitialisationHelper(settings, audioFileFormats);

            try
            {
                helper.InitaliseBassLibrary();
                // TODO: load more stuff as required
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}