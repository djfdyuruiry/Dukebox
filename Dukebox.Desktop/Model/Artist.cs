using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dukebox.Desktop.Model
{
    public class Artist
    {
        public string Title { get; set; }

        public static bool Filter(Artist s, string searchFilter)
        {
            return s.Title.ToLower().Contains(searchFilter.ToLower());
        }
    }
}
