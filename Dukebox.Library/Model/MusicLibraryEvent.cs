namespace Dukebox.Library.Model
{
    public class MusicLibraryEvent
    {
        public static readonly MusicLibraryEvent SongsAdded = new MusicLibraryEvent { Name = "SongsAdded" };
        public static readonly MusicLibraryEvent ArtistsAdded = new MusicLibraryEvent { Name = "ArtistsAdded" };
        public static readonly MusicLibraryEvent AlbumsAdded = new MusicLibraryEvent { Name = "AlbumsAdded" };
        public static readonly MusicLibraryEvent PlaylistsAdded = new MusicLibraryEvent { Name = "PlaylistsAdded" };
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
