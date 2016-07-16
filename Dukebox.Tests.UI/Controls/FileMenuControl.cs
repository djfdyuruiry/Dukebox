using System.Linq;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.MenuItems;
using TestStack.White.UIItems.WindowStripControls;

namespace Dukebox.Tests.UI.Controls
{
    public class FileMenuControl
    {
        private readonly Menu _fileMenu;

        public Menu PlayFolder { get; private set; }
        public Menu Exit { get; private set; }

        public FileMenuControl(MenuBar menuBar)
        {
            _fileMenu = menuBar.MenuItemBy(SearchCriteria.ByAutomationId("FileMenu"));

            Exit = _fileMenu.ChildMenus.First(m => m.AutomationElement.Current.AutomationId.Equals("ExitMenuItem"));
            PlayFolder = _fileMenu.ChildMenus.First(m => m.AutomationElement.Current.AutomationId.Equals("PlayFolderMenuItem")); 
        }
    }
}
