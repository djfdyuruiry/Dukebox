using System.Linq;
using TestStack.White.ScreenObjects;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WindowItems;
using TestStack.White.UIItems.WindowStripControls;

namespace Dukebox.Tests.UI.Screens
{
    public class MainScreen : AppScreen
    {
        protected MenuBar ToolbarMenu;

        public MainScreen(Window window, ScreenRepository screenRepository) : base(window, screenRepository)
        {
        }

        public virtual void Exit()
        {
            var fileMenu = ToolbarMenu.MenuItemBy(SearchCriteria.ByAutomationId("FileMenu"));

            fileMenu
                .ChildMenus
                .First(m => m.AutomationElement.Current.AutomationId == "ExitMenuItem")
                .Click();
        }
    }
}
