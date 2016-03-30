using System;
using System.ComponentModel;
using System.Threading;

namespace MicrophoneSwitch
{
    public static class HotkeyManager
    {
        private static int _lastKeyRequested;
        private static int _lastKeySent;
        private static BackgroundWorker _worker;
        private static double _delaySeconds;
        private static IntPtr _appWindowHandle;
        private static bool _noiseReduction = true;

        private static int _lastKeyAttempt;

        static HotkeyManager()
        {
            _worker = new BackgroundWorker();
            _worker.DoWork += OnWorkerDoWork;
            _worker.RunWorkerAsync();
            _delaySeconds = 2;
        }
        
        private static void OnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(TimeSpan.FromSeconds(_delaySeconds));
        }

        public static void HandleHotkey(int key)
        {
            if (key == _lastKeyAttempt || !_noiseReduction) // We got two key attempts in a row, or we're ignoring noise reduction
            {
                _lastKeyRequested = key;
                if (_lastKeyRequested != _lastKeySent && !_worker.IsBusy)
                {
                    _lastKeySent = _lastKeyRequested;
                    Win32.SetForegroundWindow(_appWindowHandle);
                    Win32.PostMessage(_appWindowHandle, Win32.WM_KEYDOWN, Win32.NumberToVK(_lastKeySent), 0);
                    _worker.RunWorkerAsync();
                }
            }

            _lastKeyAttempt = key;
        }

        public static void SetDelaySeconds(double delaySeconds)
        {
            _delaySeconds = delaySeconds;
        }

        public static void SetNoiseReduction(bool value)
        {
            _noiseReduction = value;
        }

        public static void SetAppWindowHandle(IntPtr windowHandle)
        {
            _appWindowHandle = windowHandle;
        }
    }
}
