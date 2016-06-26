namespace Dukebox.Library.Model
{
    public class MusicLibraryEvent
    {
        public static readonly MusicLibraryEvent SongAdded = new MusicLibraryEvent { Name = "SongAdded" };
        public static readonly MusicLibraryEvent ArtistAdded = new MusicLibraryEvent { Name = "ArtistAdded" };
        public static readonly MusicLibraryEvent AlbumAdded = new MusicLibraryEvent { Name = "AlbumAdded" };
        public static readonly MusicLibraryEvent PlaylistAdded = new MusicLibraryEvent { Name = "PlaylistAdded" };
        public static readonly MusicLibraryEvent AlbumCacheRefreshed = new MusicLibraryEvent { Name = "AlbumCacheRefreshed" };
        public static readonly MusicLibraryEvent ArtistCacheRefreshed = new MusicLibraryEvent { Name = "ArtistCacheRefreshed" };
        public static readonly MusicLibraryEvent PlaylistCacheRefreshed = new MusicLibraryEvent { Name = "PlaylistCacheRefreshed" };
        public static readonly MusicLibraryEvent CachesRefreshed = new MusicLibraryEvent { Name = "CachesRefreshed" };
        public static readonly MusicLibraryEvent DatabaseChangesSaved = new MusicLibraryEvent { Name = "DatabaseChangesSaved" };

        public string Name { get; private set; }

        private MusicLibraryEvent()
        {
        }
    }
}
