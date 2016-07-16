using System.Windows.Automation;
using TestStack.White.UIItems.Actions;
using TestStack.White.UIItems.Custom;

namespace Dukebox.Tests.UI.Controls
{
    [ControlTypeMapping(CustomUIItemType.Custom)]
    public class AlbumListingControl : CustomUIItem
    {   
        public AlbumListingControl(AutomationElement automationElement, ActionListener actionListener) 
            : base(automationElement, actionListener)
        {
        }

        public AlbumListingControl()
        {
        }
    }
}
