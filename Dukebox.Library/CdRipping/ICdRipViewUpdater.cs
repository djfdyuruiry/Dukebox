using System;

namespace Dukebox.Library.CdRipping
{
    public interface ICdRipViewUpdater
    {
        string Text { get; set; }
        void NotifcationLabelUpdate(string text);

        int ProgressBarMaximum { get; set; }
        int ProgressBarValue { get; set; }

        void ResetProgressBar();
        
        object Invoke(Delegate _delegate);

        void Show();
        void Hide();
        void Dispose();
    }
}
