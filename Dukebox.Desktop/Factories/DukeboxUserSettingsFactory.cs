using Dukebox.Desktop.Interfaces;

namespace Dukebox.Desktop.Factories
{
    public class DukeboxUserSettingsFactory
    {
        public IDukeboxUserSettings GetInstance()
        {
            return DesktopContainer.GetInstance<IDukeboxUserSettings>();
        }
    }
}
