namespace Dukebox.Desktop.Interfaces
{
    public interface IDukeboxUserSettings
    {
        bool Repeat { get; set; }
        bool RepeatAll { get; set; }
        bool Shuffle { get; set; }
    }
}
