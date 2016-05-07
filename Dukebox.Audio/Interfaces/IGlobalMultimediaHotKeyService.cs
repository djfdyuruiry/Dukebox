using System;

namespace Dukebox.Audio.Interfaces
{
    public interface IGlobalMultimediaHotKeyService
    {
        event EventHandler NextTrackPressed;
        event EventHandler PlayPausePressed;
        event EventHandler PreviousTrackPressed;
        event EventHandler StopPressed;

        bool RegisterMultimediaHotKeys(bool shouldRetryOnFailure, Func<bool> retryHandler);
    }
}