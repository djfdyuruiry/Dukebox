using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library;
using Dukebox.Model.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Dukebox.Desktop.ViewModel
{
    public class AudioCdViewModel : ViewModelBase, ITrackListingViewModel
    {
        public ICommand ClearSearch { get; private set; }
        public string SearchText { get; set; }
        public List<Track> Tracks { get; private set; }

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
            Tracks = new List<Track>()
            {
                new Track(){ Artist = new artist {name= "Bob Dylan"}, Album = new album {name = "Times"}, Song = new song { title = "Are a changin'"} },
                new Track(){ Artist = new artist {name= "Marky Mark"}, Album = new album {name = "Rave Madness"}, Song = new song { title = "Good Vibrations"} },
                new Track(){ Artist = new artist {name= "VNV Nation"}, Album = new album {name = "Matter+Form"}, Song = new song { title = "Lightwave"} },
                new Track(){ Artist = new artist {name= "Tracy Chapman"}, Album = new album {name = "Jolata True"}, Song = new song { title = "Fast Car"} }
            };
        }
    }
}
