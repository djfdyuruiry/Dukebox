using System.Windows.Automation;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Actions;
using TestStack.White.UIItems.Custom;
using TestStack.White.UIItems.WPFUIItems;

namespace Dukebox.Tests.UI.Controls
{
    [ControlTypeMapping(CustomUIItemType.Custom)]
    public class SearchControl : CustomUIItem
    {
        public TextBox SearchText { get; private set; }
        public Button SearchButton { get; private set; }

        public SearchControl(AutomationElement automationElement, ActionListener actionListener) 
            : base(automationElement, actionListener)
        {
            SearchText = this.Get<TextBox>("SearchText");
            SearchButton = this.Get<Button>("SearchButton");
        }

        public SearchControl()
        {
        }
    }
}
