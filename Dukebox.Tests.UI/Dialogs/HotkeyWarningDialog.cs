using TestStack.White.ScreenObjects;
using TestStack.White.ScreenObjects.ScreenAttributes;
using TestStack.White.UIItems;
using TestStack.White.UIItems.WindowItems;

namespace Dukebox.Tests.UI.Dialogs
{
    public class HotkeyWarningDialog : AppScreen
    {
        [AutomationId("6")]
        protected Button Yes;
        [AutomationId("7")]
        protected Button No;

        public HotkeyWarningDialog(Window window, ScreenRepository screenRepository) : base(window, screenRepository)
        {
        }

        public virtual void Dismiss()
        {
            No.Click();
        }
    }
}
