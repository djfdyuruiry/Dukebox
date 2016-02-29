using Dukebox.Audio.Interfaces;
using Dukebox.Audio.Services;
using log4net;
using SimpleInjector;
using SimpleInjector.Packaging;
using System;

namespace Dukebox.Audio
{
    public class AudioPackage : IPackage
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AudioPackage));
        private static Container container;

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
    }
}
