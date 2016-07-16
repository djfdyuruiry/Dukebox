using System.Collections.Generic;
using System.Windows.Automation;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Actions;
using TestStack.White.UIItems.Custom;
using TestStack.White.UIItems.WPFUIItems;
using Dukebox.Tests.UI.Model;

namespace Dukebox.Tests.UI.Controls
{
    [ControlTypeMapping(CustomUIItemType.Custom)]
    public class TrackListingControl : CustomUIItem
    {
        public SearchControl SearchControl { get; private set; }
        public ListView TrackListingsGrid { get; private set; }

        public TrackListingControl(AutomationElement automationElement, ActionListener actionListener) 
            : base(automationElement, actionListener)
        {
            SearchControl = this.Get<SearchControl>("SearchUserControl");
            TrackListingsGrid = this.Get<ListView>("TrackListingsGrid");
        }

        public TrackListingControl()
        {
        }

        public virtual List<Track> GetTracks()
        {
            var tracks = new List<Track>();
            var propertyMap = new Dictionary<int, string>();

            for (var i = 0; i < TrackListingsGrid.Header.Columns.Count; i++)
            {
                propertyMap[i] = TrackListingsGrid.Header.Columns[i].Text;
            }

            foreach (var row in TrackListingsGrid.Rows)
            {
                var track = GetTrackForRow(row, propertyMap);
                tracks.Add(track);
            }

            return tracks;
        }

        private Track GetTrackForRow(ListViewRow row, Dictionary<int, string> propertyMap)
        {
            var track = new Track();

            for (var c = 0; c < row.Cells.Count; c++)
            {
                var value = row.Cells[c].Text;

                switch (propertyMap[c])
                {
                    case "Artist":
                    {
                        track.Artist = value;
                        break;
                    }
                    case "Album":
                    {
                        track.Album = value;
                        break;
                    }
                    case "Title":
                    {
                        track.Title = value;
                        break;
                    }
                }
            }

            return track;
        }
    }
}
