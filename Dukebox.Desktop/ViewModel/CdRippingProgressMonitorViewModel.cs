using Dukebox.Desktop.ViewModel;
using Dukebox.Library.Interfaces;
using System;

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

        public event EventHandler<string> OnNotificationUpdate;
        public event EventHandler OnResetProgressBar;
        public event EventHandler OnComplete;

        public CdRippingProgressMonitorViewModel()
            : base()
        {
            OnNotificationUpdate += (o, s) => StatusText = s;
            OnResetProgressBar += (o, e) => CurrentProgressValue = 0;
        }

        public void NotificationUpdate(string notification)
        {
            if (OnNotificationUpdate != null)
            {
                OnNotificationUpdate(this, notification);
            }
        }

        public void ResetProgressBar()
        {
            if (OnResetProgressBar != null)
            {
                OnResetProgressBar(this, EventArgs.Empty);
            }
        }

        public void Complete()
        {
            if (OnComplete != null)
            {
                OnComplete(this, EventArgs.Empty);
            }
        }
    }
}
