using System.Windows.Automation;
using TestStack.White.UIItems.Actions;
using TestStack.White.UIItems.Custom;
using TestStack.White.UIItems.WPFUIItems;

namespace Dukebox.Tests.UI.Controls
{
    [ControlTypeMapping(CustomUIItemType.Custom)]
    public class AudioCdListingControl : CustomUIItem
    {
        public TrackListingControl TrackListing { get; private set; }

        // TODO: Add top control for selecting audio CD and ripping etc.

        public AudioCdListingControl(AutomationElement automationElement, ActionListener actionListener) 
            : base(automationElement, actionListener)
        {
            TrackListing = this.Get<TrackListingControl>("TrackListingControl");
        }

        public AudioCdListingControl()
        {
        }
    }
}
