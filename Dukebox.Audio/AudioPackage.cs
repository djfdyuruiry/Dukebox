using Dukebox.Audio.Interfaces;
using Dukebox.Audio.Services;
using log4net;
using SimpleInjector;
using SimpleInjector.Packaging;
using System;
using System.Reflection;

namespace Dukebox.Audio
{
    public class AudioPackage : IPackage
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Container container;

        public static bool ExecutingForUnitTests { get; set; }

        static AudioPackage()
        {
            container = new Container();
            Configure(container);            
        }

        private static void Configure(Container container)
        {
            container.RegisterSingleton<AudioFileFormats>();
            container.RegisterSingleton<IAudioCdService, AudioCdService>();
            container.RegisterSingleton<IAudioConverterService, AudioConverterService>();
            container.RegisterSingleton<IMediaPlayer, MediaPlayer>();
        }

        public static TService GetInstance<TService>() where TService : class
        {
            return container.GetInstance<TService>();
        }

        public void RegisterServices(Container container)
        {
            Configure(container);
        }

        public static Container GetContainerForTestOverrides()
        {
            if (!ExecutingForUnitTests)
            {
                throw new InvalidOperationException("Accessing the internal container is only valid when ExecutingForUnitTests is true");
            }

            return container;
        }
    }
}
