using System;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Desktop.ViewModel
{
    public class CdRippingProgressMonitorViewModel : ProgressMonitorViewModel, ICdRipViewUpdater
    {
        public string Text
        {
            get
            {
                return HeaderText;
            }
            set
            {
                HeaderText = value;
            }
        }

        public int ProgressBarMaximum
        {
            get
            {
                return MaximumProgressValue;
            }
            set
            {
                MaximumProgressValue = value;
            }
        }

        public int ProgressBarValue
        {
            get
            {
                return CurrentProgressValue;
            }
            set
            {
                CurrentProgressValue = value;
            }
        }

        public event EventHandler<GenericEventArgs<string>> OnNotificationUpdate;
        public event EventHandler OnResetProgressBar;
        public event EventHandler OnComplete;

        public CdRippingProgressMonitorViewModel()
            : base()
        {
            OnNotificationUpdate += (o, s) => StatusText = s.Data;
            OnResetProgressBar += (o, e) => CurrentProgressValue = 0;
        }

        public void NotificationUpdate(string notification)
        {
            OnNotificationUpdate?.Invoke(this, new GenericEventArgs<string> { Data = notification });
        }

        public void ResetProgressBar()
        {
            OnResetProgressBar?.Invoke(this, EventArgs.Empty);
        }

        public void Complete()
        {
            OnComplete?.Invoke(this, EventArgs.Empty);
        }
    }
}
