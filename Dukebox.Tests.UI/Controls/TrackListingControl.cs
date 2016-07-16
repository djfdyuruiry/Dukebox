using System.Windows.Automation;
using TestStack.White.UIItems.Actions;
using TestStack.White.UIItems.Custom;
using TestStack.White.UIItems.WPFUIItems;

namespace Dukebox.Tests.UI.Controls
{
    [ControlTypeMapping(CustomUIItemType.Custom)]
    public class TrackListingControl : CustomUIItem
    {
        public SearchControl SearchControl { get; private set; }
        public TrackListingsGrid TrackListingsGrid { get; private set; }

        public TrackListingControl(AutomationElement automationElement, ActionListener actionListener) 
            : base(automationElement, actionListener)
        {
            SearchControl = this.Get<SearchControl>("SearchUserControl");
            TrackListingsGrid = this.Get<TrackListingsGrid>("TrackListingsGrid");
        }

        public TrackListingControl()
        {
        }
    }
}
