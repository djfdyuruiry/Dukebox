using System;
using System.Linq;
using System.Threading;
using TestStack.White;
using TestStack.White.Configuration;
using TestStack.White.Factory;
using TestStack.White.ScreenObjects;
using Dukebox.Tests.UI.Dialogs;
using Dukebox.Tests.UI.Screens;
using Dukebox.Tests.UI.Helpers;

namespace Dukebox.Tests.UI.Applciations
{
    public class DukeboxApplication : IDisposable
    {
        private const string binPath = @"..\..\..\..\Dukebox.Desktop\bin\x64\Release\Dukebox.exe";

        public Application ApplicationHandle { get; private set; }
        public ScreenRepository ScreenRepository { get; private set; }
        public bool HotkeyWarningDialogWasDisplayedOnLaunch { get; private set; }
        public bool AppHasExited
        {
            get
            {
                return ApplicationHandle?.ApplicationSession?.Application?.HasExited ?? false;
            }
        }

        public void Launch(bool dismissHotkeyWarningDialog)
        {
            ApplicationHandle = Application.Launch(binPath);
            ScreenRepository = new ScreenRepository(ApplicationHandle.ApplicationSession);

            WaitForFirstWindow();

            if (dismissHotkeyWarningDialog)
            {
                DismissHotkeyWarningDialogIfPresent();
            }
        }

        private void WaitForFirstWindow()
        {
            while (!ApplicationHandle.GetWindows().Any())
            {
                Thread.Sleep(100);
            }
        }

        private void DismissHotkeyWarningDialogIfPresent()
        {
            try
            {
                using (CoreAppXmlConfiguration.Instance.ApplyTemporarySetting(c => c.FindWindowTimeout = 100))
                {
                    var warningDialog = GetScreen<HotkeyWarningDialog>();
                    warningDialog.Dismiss();

                    HotkeyWarningDialogWasDisplayedOnLaunch = true;
                }
            }
            catch (Exception)
            {
                // Dialog is not present
                HotkeyWarningDialogWasDisplayedOnLaunch = false;
            }
        }

        public T GetScreen<T>() where T : AppScreen
        {
            var windowTitle = ScreenDialogWindowTitleHelper.GetWindowTitleForScreenDialog<T>();
            var screen = ScreenRepository.Get<T>(windowTitle, InitializeOption.NoCache);

            return screen;
        }

        public T GetModal<T>() where T : AppScreen
        {
            var modalTitle = ScreenDialogWindowTitleHelper.GetWindowTitleForScreenDialog<T>();
            var mainWindow = ApplicationHandle.GetWindows()
                .First(w => w.Title.Equals(ScreenDialogWindowTitleHelper.GetWindowTitleForScreenDialog<MainScreen>()));

            var modal = ScreenRepository.GetModal<T>(modalTitle, mainWindow, InitializeOption.NoCache);

            return modal;
        }

        public void SkipInitalImport()
        {
            var initalImportScreen = GetScreen<InitalImportScreen>();
            initalImportScreen.SkipImport();
        }

        public void Dispose()
        {
            if(!AppHasExited)
            {
                ApplicationHandle.Close();
            }
        }
    }
}
