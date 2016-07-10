namespace Dukebox.Desktop.Model
{
    public class PreviewTracksMessage
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public bool IsArtist { get; set; }
        public bool IsAlbum { get; set; }
        public bool IsPlaylist { get; set; }
    }
}
