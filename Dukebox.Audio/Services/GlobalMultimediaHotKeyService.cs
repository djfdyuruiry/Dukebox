using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using GlobalHotKey;
using log4net;
using Dukebox.Audio.Interfaces;

namespace Dukebox.Audio.Services
{
    public class GlobalMultimediaHotKeyService : IGlobalMultimediaHotKeyService, IDisposable
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly List<Key> keysToRegister = new List<Key> { Key.MediaPlayPause, Key.MediaNextTrack, Key.MediaPreviousTrack, Key.MediaStop };

        private readonly HotKeyManager _hotKeyManager;

        private bool _hotKeysRegistered;

        public event EventHandler PlayPausePressed;
        public event EventHandler StopPressed;
        public event EventHandler PreviousTrackPressed;
        public event EventHandler NextTrackPressed;

        public GlobalMultimediaHotKeyService()
        {
            _hotKeyManager = new HotKeyManager();
        }

        public bool RegisterMultimediaHotKeys(bool shouldRetryOnFailure, Func<bool> retryHandler)
        {
            if (_hotKeysRegistered)
            {
                return true;
            }
            
            try
            {
                keysToRegister.ForEach(k => _hotKeyManager.Register(new HotKey(k, ModifierKeys.None)));
                _hotKeyManager.KeyPressed += OnKeyPress;

                logger.Info("Registered multimedia hot keys successfully");

                _hotKeysRegistered = true;
            }
            catch (Win32Exception ex)
            {
                var attemptRetry = retryHandler != null ? retryHandler() : false;

                logger.WarnFormat("Failed to register multimedia hot keys, {0}attempting retry: {1}", attemptRetry ? string.Empty : "not ", ex);

                _hotKeysRegistered = attemptRetry ? RegisterMultimediaHotKeys(shouldRetryOnFailure, retryHandler) : false;
            }

            return _hotKeysRegistered;
        }

        private void OnKeyPress(object sender, KeyPressedEventArgs eventArgs)
        {
            var keyPressed = eventArgs.HotKey.Key;

            switch(keyPressed)
            {
                case (Key.MediaPlayPause):
                {
                    PlayPausePressed?.Invoke(this, null);
                    break;
                }
                case (Key.MediaStop):
                {
                    StopPressed?.Invoke(this, null);
                    break;
                }
                case (Key.MediaPreviousTrack):
                {
                    PreviousTrackPressed?.Invoke(this, null);
                    break;
                }
                case (Key.MediaNextTrack):
                {
                    NextTrackPressed?.Invoke(this, null);
                    break;
                }
            }
        }

        protected virtual void Dispose(bool cleanAllResources)
        {
            if (cleanAllResources)
            {
                _hotKeyManager.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
