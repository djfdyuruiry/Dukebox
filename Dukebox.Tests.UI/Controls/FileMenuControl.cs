using System.Linq;
using TestStack.White.UIItems.MenuItems;

namespace Dukebox.Tests.UI.Controls
{
    public class FileMenuControl
    {
        private readonly Menu _fileMenu;

        public Menu PlayFolder { get; private set; }
        public Menu Exit { get; private set; }

        public FileMenuControl(Menu fileMenu)
        {
            _fileMenu = fileMenu;

            Exit = _fileMenu.ChildMenus.First(m => m.AutomationElement.Current.AutomationId.Equals("ExitMenuItem"));
            PlayFolder = _fileMenu.ChildMenus.First(m => m.AutomationElement.Current.AutomationId.Equals("PlayFolderMenuItem")); 
        }
    }
}
