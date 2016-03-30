using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VoiceRecorder.Audio;

namespace MicrophoneSwitch
{
    public class MicrophoneViewModel : DependencyObject
    {
        private AudioRecorder _recorder;

        public static readonly DependencyProperty CurrentInputLevelProperty = DependencyProperty.Register("CurrentInputLevel", typeof(float), typeof(MicrophoneViewModel));
        public float CurrentInputLevel
        {
            get { return (float)GetValue(CurrentInputLevelProperty); }
            set { SetValue(CurrentInputLevelProperty, value); }
        }        

        public static readonly DependencyProperty MicNameProperty = DependencyProperty.Register("MicName", typeof(string), typeof(MicrophoneViewModel));

        internal void Disable()
        {
            _recorder.StopMonitoring();
            _recorder.SampleAggregator.MaximumCalculated -= OnSampleCalculated;
        }

        public string MicName
        {
            get { return (string)GetValue(MicNameProperty); }
            set { SetValue(MicNameProperty, value); }
        }

        public static readonly DependencyProperty KeyBindingProperty = DependencyProperty.Register("KeyBinding", typeof(int), typeof(MicrophoneViewModel));
        public int KeyBinding
        {
            get { return (int)GetValue(KeyBindingProperty); }
            set { SetValue(KeyBindingProperty, value); }
        }

        public static readonly DependencyProperty TriggerLevelProperty = DependencyProperty.Register("TriggerLevel", typeof(int), typeof(MicrophoneViewModel), new FrameworkPropertyMetadata(20));
        public int TriggerLevel
        {
            get { return (int)GetValue(TriggerLevelProperty); }
            set { SetValue(TriggerLevelProperty, value); }
        }

        public MicrophoneViewModel(int deviceId)
        {
            MicName = WaveIn.GetCapabilities(deviceId).ProductName;            
            KeyBinding = deviceId;

            _recorder = new AudioRecorder();
            _recorder.BeginMonitoring(deviceId);
            _recorder.SampleAggregator.MaximumCalculated += OnSampleCalculated;
        }

        private void OnSampleCalculated(object sender, MaxSampleEventArgs e)
        {
            CurrentInputLevel = Math.Max(e.MaxSample, Math.Abs(e.MinSample)) * 100;
            if (Win32.IsEnabled && CurrentInputLevel > TriggerLevel)
            {
                HotkeyManager.HandleHotkey(KeyBinding + 1);
            }
        }
    }
}
