using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WindowStripControls;

namespace Dukebox.Tests.UI.Controls
{
    public class MenuBarControl
    {
        private readonly MenuBar _menuBar;

        public FileMenuControl FileMenu { get; private set; }

        public MenuBarControl(MenuBar menuBar)
        {
            _menuBar = menuBar;

            var fileMenu = _menuBar.MenuItemBy(SearchCriteria.ByAutomationId("FileMenu"));
            FileMenu = new FileMenuControl(fileMenu);
        }
    }
}
