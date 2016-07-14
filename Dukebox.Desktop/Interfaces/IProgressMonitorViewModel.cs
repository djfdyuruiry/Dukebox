namespace Dukebox.Desktop.Interfaces
{
    public interface IProgressMonitorViewModel
    {
        string Title { get; set; }
        string HeaderText { get; set; }
        string NotificationText { get; set; }
        string StatusText { get; set; }
        int MaximumProgressValue { get; set; }
        int CurrentProgressValue { get; set; }
    }
}
