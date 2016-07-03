using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Dukebox.Library.Model;
using Dukebox.Library.Interfaces;

namespace Dukebox.Library.Services
{
    public class PlaylistGeneratorService : IPlaylistGeneratorService
    {
        public Playlist GetPlaylistFromFile(string playlistFile)
        {
            if (!File.Exists(playlistFile))
            {
                throw new FileNotFoundException(string.Format("The playlist file '{0}' does not exist on this system", playlistFile));
            }

            using (var playlistFileReader = new StreamReader(playlistFile))
            {
                var jsonTracks = playlistFileReader.ReadToEnd();

                var files = JsonConvert.DeserializeObject<List<string>>(jsonTracks);

                var playlist = new Playlist() { Id = -1, FilenamesCsv = string.Join(",", files) };
                return playlist;
            }
        }
    }
}
