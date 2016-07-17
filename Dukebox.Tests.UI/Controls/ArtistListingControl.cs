using System.Windows.Automation;
using TestStack.White.UIItems.Actions;
using TestStack.White.UIItems.Custom;
using TestStack.White.UIItems.WPFUIItems;

namespace Dukebox.Tests.UI.Controls
{
    [ControlTypeMapping(CustomUIItemType.Custom)]
    public class ArtistListingControl : CustomUIItem
    {
        public SearchControl SearchControl { get; private set; }

        public ArtistListingControl(AutomationElement automationElement, ActionListener actionListener) 
            : base(automationElement, actionListener)
        {
            SearchControl = this.Get<SearchControl>("SearchUserControl");
        }

        public ArtistListingControl()
        {
        }
    }
}
