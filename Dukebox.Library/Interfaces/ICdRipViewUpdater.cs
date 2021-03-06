﻿using System;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface ICdRipViewUpdater
    {
        string Text { get; set; }
        int ProgressBarMaximum { get; set; }
        int ProgressBarValue { get; set; }

        event EventHandler OnResetProgressBar;
        event EventHandler<GenericEventArgs<string>> OnNotificationUpdate;
        event EventHandler OnComplete;

        void ResetProgressBar();
        void NotificationUpdate(string notification);
        void Complete();
    }
}
