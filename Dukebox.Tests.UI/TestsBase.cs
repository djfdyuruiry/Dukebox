using System;
using Dukebox.Tests.UI.Applciations;
using Dukebox.Tests.UI.Model;

namespace Dukebox.Tests.UI
{
    public class TestsBase : IDisposable
    {
        protected readonly DukeboxApplication _dukeboxApp;

        public TestsBase() : this(new TestsBaseOptions())
        {
        }

        public TestsBase(TestsBaseOptions options)
        {
            _dukeboxApp = new DukeboxApplication();
            _dukeboxApp.Launch(options.DismissHotkeyWarningDialog);

            if (options.SkipInitalImport)
            {
                _dukeboxApp.SkipInitalImport();
            }
        }

        public void Dispose()
        {
            _dukeboxApp.Dispose();
        }
    }
}
