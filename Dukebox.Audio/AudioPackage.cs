using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using SimpleInjector;
using SimpleInjector.Packaging;
using Dukebox.Audio.Interfaces;
using Dukebox.Audio.Services;
using Dukebox.Configuration;

namespace Dukebox.Audio
{
    public class AudioPackage : IPackage
    {
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
            container.RegisterSingleton<IGlobalMultimediaHotKeyService, GlobalMultimediaHotKeyService>();
            container.RegisterSingleton<IAudioService, AudioService>();
            container.RegisterSingleton<IMediaPlayer, MediaPlayer>();

            var assemblies = new List<Assembly> { Assembly.GetAssembly(typeof(ConfigurationPackage)) };

            container.RegisterPackages(assemblies);
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
