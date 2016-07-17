using System;
using System.IO;
using System.Linq;
using TestStack.White;
using TestStack.White.ScreenObjects;
using TestStack.White.ScreenObjects.ScreenAttributes;
using TestStack.White.UIItems;
using TestStack.White.UIItems.TreeItems;
using TestStack.White.UIItems.WindowItems;
using Dukebox.Tests.UI.Helpers;

namespace Dukebox.Tests.UI.Dialogs
{
    public class SelectFolderDialog : AppScreen
    {
        private const string nodeNotVisibleText = "expand button not visible";

        private static readonly string rootComputerNodeName;

        [AutomationId("100")]
        protected Tree FolderTree;

        [AutomationId("1")]
        public Button Ok;
        [AutomationId("2")]
        public Button Cancel;

        static SelectFolderDialog()
        {
            rootComputerNodeName = OperatingSystemVersionHelper.OsIsWindows8OrAbove ? "This PC" : "Computer";
        }

        public SelectFolderDialog(Window window, ScreenRepository screenRepository) : base(window, screenRepository)
        {
        }

        public virtual void SetFolderPathAndClickOk(string path)
        {
            path = Path.GetFullPath(path);

            var explodedPath = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, 
                StringSplitOptions.RemoveEmptyEntries).ToList();
            var drive = explodedPath[0];

            explodedPath[0] = $"Local Disk ({drive})";
            explodedPath.Insert(0, rootComputerNodeName);

            var targetNode = FolderTree.Nodes.First();

            for (var i = 0; i < explodedPath.Count; i++)
            {
                var nodeText = explodedPath[i];

                try
                {
                    targetNode = targetNode.GetItem(new[] { nodeText });
                    targetNode.Click();
                }
                catch (AutomationException ex)
                {
                    if (!ex.Message.Contains(nodeNotVisibleText))
                    {
                        throw;
                    }

                    targetNode.Focus();
                    i--;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error while looking up Select Folder Dialog node with name '{nodeText}': {ex}");
                    throw;
                }
            }

            Ok.Click();
        }
    }
}
