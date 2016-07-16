using TestStack.White.UIItems.WindowItems;
using TestStack.White.UIItems.WindowStripControls;

namespace Dukebox.Tests.UI.Controls
{
    public class MenuBarControl
    {
        private readonly MenuBar _menuBar;

        public FileMenuControl FileMenu { get; private set; }

        public MenuBarControl(Window window)
        {
            _menuBar = window.Get<MenuBar>("MenuBar");
                        
            FileMenu = new FileMenuControl(_menuBar);
        }
    }
}
