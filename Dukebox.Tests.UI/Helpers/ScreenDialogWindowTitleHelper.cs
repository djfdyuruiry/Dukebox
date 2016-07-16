using System;
using System.Collections.Generic;
using Dukebox.Tests.UI.Dialogs;
using Dukebox.Tests.UI.Screens;

namespace Dukebox.Tests.UI.Helpers
{
    public static class ScreenDialogWindowTitleHelper
    {
        private static readonly Dictionary<Type, string> screenDialogTypeToWindowTitleMap;

        static ScreenDialogWindowTitleHelper()
        {
            screenDialogTypeToWindowTitleMap = new Dictionary<Type, string>
            {
                {typeof(HotkeyWarningDialog), "Dukebox - Error Registering Hot Keys"},
                {typeof(InitalImportScreen), "Dukebox - Welcome"},
                {typeof(MainScreen), "Dukebox"},
            };
        }

        public static string GetWindowTitleForScreenDialog<T>()
        {
            var type = typeof(T);
            var title = screenDialogTypeToWindowTitleMap[type];

            return title;
        }

    }
}
