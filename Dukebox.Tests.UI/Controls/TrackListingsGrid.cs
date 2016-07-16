using System.Windows.Automation;
using TestStack.White.UIItems.Actions;
using TestStack.White.UIItems.Custom;

namespace Dukebox.Tests.UI.Controls
{
    [ControlTypeMapping(CustomUIItemType.DataGrid)]
    public class TrackListingsGrid : CustomUIItem
    {   
        public TrackListingsGrid(AutomationElement automationElement, ActionListener actionListener) 
            : base(automationElement, actionListener)
        {
        }

        public TrackListingsGrid()
        {
        }
    }
}
