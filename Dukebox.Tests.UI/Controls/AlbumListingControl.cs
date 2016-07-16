using System.Windows.Automation;
using TestStack.White.UIItems.Actions;
using TestStack.White.UIItems.Custom;
using TestStack.White.UIItems.WPFUIItems;

namespace Dukebox.Tests.UI.Controls
{
    [ControlTypeMapping(CustomUIItemType.Custom)]
    public class AlbumListingControl : CustomUIItem
    {
        public SearchControl SearchControl { get; private set; }

        public AlbumListingControl(AutomationElement automationElement, ActionListener actionListener) 
            : base(automationElement, actionListener)
        {
            SearchControl = this.Get<SearchControl>("SearchUserControl");
        }

        public AlbumListingControl()
        {
        }
    }
}
