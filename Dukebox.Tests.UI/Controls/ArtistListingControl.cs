using System.Windows.Automation;
using TestStack.White.UIItems.Actions;
using TestStack.White.UIItems.Custom;

namespace Dukebox.Tests.UI.Controls
{
    [ControlTypeMapping(CustomUIItemType.Custom)]
    public class ArtistListingControl : CustomUIItem
    {   
        public ArtistListingControl(AutomationElement automationElement, ActionListener actionListener) 
            : base(automationElement, actionListener)
        {
        }

        public ArtistListingControl()
        {
        }
    }
}
