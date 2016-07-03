using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dukebox.Audio;
using Dukebox.Library.Factories;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Services
{
    public class TrackGeneratorService : ITrackGeneratorService
    {
        private readonly AudioFileFormats _audioFormats;
        private readonly IMusicLibrarySearchService _musicLibrarySearchService;
        private readonly TrackFactory _trackFactory;

        public TrackGeneratorService(AudioFileFormats audioFormats, IMusicLibrarySearchService searchService, TrackFactory trackFactory)
        {
            _audioFormats = audioFormats;
            _musicLibrarySearchService = searchService;
            _trackFactory = trackFactory;
        }

        public List<ITrack> GetTracksForDirectory(string directory, bool subDirectories)
        {
            var searchOption = subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var supportedFormats = _audioFormats.SupportedFormats;

            var validFiles = Directory.EnumerateFiles(directory, "*.*", searchOption).Where((f) => supportedFormats.Any(sf => f.EndsWith(sf))).ToList();
            var libraryTracks = validFiles.SelectMany(f => _musicLibrarySearchService.GetTracksByAttributeValue(SearchAreas.Filename, f)).ToList();

            validFiles = validFiles.Except(libraryTracks.Select(t => t.Song.FileName)).ToList();

            var tracksToReturn = validFiles.Select((f) => _trackFactory.BuildTrackInstance(f)).Concat(libraryTracks).ToList();
            return tracksToReturn;
        }

        public List<ITrack> GetTracksForPlaylist(Playlist playlist)
        {
            var libraryTracks = playlist.Files.SelectMany(f => _musicLibrarySearchService.GetTracksByAttributeValue(SearchAreas.Filename, f)).ToList();
            var nonLibraryFiles = playlist.Files.Except(libraryTracks.Select(t => t.Song.FileName)).ToList();

            var tracks = nonLibraryFiles.Select(file =>
            {
                try
                {
                    return _trackFactory.BuildTrackInstance(file);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error while loading track from file '{0}' for playlist {1}", file, ToString()), ex);
                }
            }).Concat(libraryTracks).ToList();

            return tracks;
        }
    }
}
