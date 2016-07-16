using System.Windows.Automation;
using TestStack.White.UIItems.Actions;
using TestStack.White.UIItems.Custom;
using TestStack.White.UIItems.WPFUIItems;

namespace Dukebox.Tests.UI.Controls
{
    [ControlTypeMapping(CustomUIItemType.Custom)]
    public class AudioCdListingControl : CustomUIItem
    {
        public SearchControl SearchControl { get; private set; }

        public AudioCdListingControl(AutomationElement automationElement, ActionListener actionListener) 
            : base(automationElement, actionListener)
        {
            SearchControl = this.Get<SearchControl>("SearchUserControl");
        }

        public AudioCdListingControl()
        {
        }
    }
}
