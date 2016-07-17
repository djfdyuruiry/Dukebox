using TestStack.White.ScreenObjects;
using TestStack.White.UIItems;
using TestStack.White.UIItems.WindowItems;

namespace Dukebox.Tests.UI.Screens
{
    public class InitalImportScreen : AppScreen
    {
        protected Hyperlink SkipImportLink;

        public InitalImportScreen(Window window, ScreenRepository screenRepository) : base(window, screenRepository)
        {
        }

        public virtual void SkipImport()
        {
            SkipImportLink.Click();
        }
    }
}
