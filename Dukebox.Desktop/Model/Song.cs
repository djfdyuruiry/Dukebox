using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dukebox.Desktop.Model
{
    public class Song
    {
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }

        public static bool Filter(Song s, string searchFilter)
        {
            return s.Album.ToLower().Contains(searchFilter.ToLower()) ||
                s.Artist.ToLower().Contains(searchFilter.ToLower()) ||
                s.Title.ToLower().Contains(searchFilter.ToLower());
        }
    }
}
