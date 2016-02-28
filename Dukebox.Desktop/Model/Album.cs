using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dukebox.Desktop.Model
{
    public class Album
    {
        public string Title { get; set; }

        public static bool Filter(Album s, string searchFilter)
        {
            return s.Title.ToLower().Contains(searchFilter.ToLower());
        }
    }
}
