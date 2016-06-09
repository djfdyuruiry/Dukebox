using SimpleInjector;
using SimpleInjector.Packaging;
using Dukebox.Configuration.Interfaces;

namespace Dukebox.Configuration
{
    public class ConfigurationPackage : IPackage
    {
        private static void Configure(Container container)
        {
            container.RegisterSingleton<IDukeboxSettings, DukeboxSettings>();
        }

        public void RegisterServices(Container container)
        {
            Configure(container);
        }
    }
}
