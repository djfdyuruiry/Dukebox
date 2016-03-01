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
                Track.BuildTrackInstance(new album {name = "Times"}, new artist {name= "Bob Dylan"}, new song { title = "Are a changin'"}),
                Track.BuildTrackInstance(new album {name = "Times"}, new artist {name= "Bob Dylan"}, new song { title = "Are a changin'"}),
                Track.BuildTrackInstance(new album {name = "Times"}, new artist {name= "Bob Dylan"}, new song { title = "Are a changin'"}),
                Track.BuildTrackInstance(new album {name = "Times"}, new artist {name= "Bob Dylan"}, new song { title = "Are a changin'"}),
                Track.BuildTrackInstance(new album {name = "Times"}, new artist {name= "Bob Dylan"}, new song { title = "Are a changin'"})
            };
        }
    }
}
