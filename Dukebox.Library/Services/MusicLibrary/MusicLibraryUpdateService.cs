using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Services.MusicLibrary
{
    public class MusicLibraryUpdateService : IMusicLibraryUpdateService
    {
        private readonly IMusicLibraryDbContextFactory _dbContextFactory;

        public MusicLibraryUpdateService(IMusicLibraryDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task SaveSongChanges(Song song)
        {
            if (song == null)
            {
                throw new ArgumentNullException("song");
            }

            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                dukeboxData.Songs.Attach(song);

                var dbEntity = dukeboxData.Entry(song);

                if (dbEntity != null)
                {
                    dbEntity.State = EntityState.Modified;
                    await _dbContextFactory.SaveDbChanges(dukeboxData);
                }
            }
        }

        public async Task RemoveTrack(ITrack track)
        {
            if (track == null)
            {
                throw new ArgumentNullException("track");
            }

            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                var song = dukeboxData.Songs.FirstOrDefault(s => s.Id == track.Song.Id);

                if (song == null)
                {
                    return;
                }

                dukeboxData.Songs.Remove(song);
                await _dbContextFactory.SaveDbChanges(dukeboxData);
            }
        }
    }
}
