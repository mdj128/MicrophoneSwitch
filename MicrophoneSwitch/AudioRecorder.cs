using System;
using System.Linq;
using NAudio.Wave;
using NAudio.Mixer;

namespace VoiceRecorder.Audio
{
    public class AudioRecorder
    {
        WaveIn waveIn;
        readonly SampleAggregator sampleAggregator;
        WaveFormat recordingFormat;
        
        public AudioRecorder()
        {
            sampleAggregator = new SampleAggregator();
            RecordingFormat = new WaveFormat(44100, 1);
        }

        public WaveFormat RecordingFormat
        {
            get
            {
                return recordingFormat;
            }
            set
            {
                recordingFormat = value;
                sampleAggregator.NotificationCount = value.SampleRate / 10;
            }
        }

        public void StopMonitoring()
        {
            waveIn.DataAvailable -= OnDataAvailable;
            waveIn.StopRecording();
        }

        public void BeginMonitoring(int recordingDevice)
        {
            waveIn = new WaveIn();
            waveIn.DeviceNumber = recordingDevice;
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.WaveFormat = recordingFormat;
            waveIn.StartRecording();
        }
                       
        public SampleAggregator SampleAggregator
        {
            get
            {
                return sampleAggregator;
            }
        }
                
        void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesRecorded = e.BytesRecorded;

            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((buffer[index + 1] << 8) |
                                        buffer[index + 0]);
                float sample32 = sample / 32768f;
                sampleAggregator.Add(sample32);
            }
        }       
    }
}
