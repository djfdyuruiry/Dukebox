using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Dukebox.Desktop.ViewModel
{
    public class AudioCdViewModel : ViewModelBase, ISongListingViewModel
    {
        public ICommand ClearSearch { get; private set; }
        public string SearchText { get; set; }
        public List<Song> Songs { get; private set; }

        public bool EditingListingsDisabled
        {
            get 
            { 
                return false; 
            }
        }
        public bool SearchEnabled
        {
            get
            {
                return false;
            }
        }

        public AudioCdViewModel() : base()
        {
            Songs = new List<Song>()
            {
                new Song(){ Artist = "Bob Dylan", Album = "Times", Title = "Are a changin'" },
                new Song(){ Artist = "VNV Nation", Album = "Future Perfect", Title = "Airships" },
                new Song(){ Artist = "Metallica", Album = "One", Title = "Enter Sandman" },
                new Song(){ Artist = "Tracy Chapman", Album = "Jolata True", Title = "Fast Car" }
            };
        }
    }
}
