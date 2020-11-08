﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    [ApplicationKey(ApplicationKeyConstant.REMOTE_AUDIO)]
    public class AudioAdapterHandler : ApplicationAdapterHandler
    {
        public event Action<AudioAdapterHandler, bool, bool> OnOpenDeviceStatusEventHandler;

        public event Action<AudioAdapterHandler, byte[]> OnPlayerEventHandler;

        [PacketHandler(MessageHead.C_AUDIO_DEVICE_OPENSTATE)]
        private void RemoteDeveiceStatusHandler(SessionProviderContext session)
        {
            var statesPack = session.GetMessageEntity<AudioDeviceStatesPacket>();
            this.OnOpenDeviceStatusEventHandler?.Invoke(this, statesPack.PlayerEnable, statesPack.RecordEnable);
        }

        [PacketHandler(MessageHead.C_AUDIO_DATA)]
        private void PlayerData(SessionProviderContext session)
        {
            var payload = session.GetMessage();
            this.OnPlayerEventHandler?.Invoke(this, payload);
        }

        public void StartRemoteAudio(int samplesPerSecond, int bitsPerSample, int channels)
        {
            CurrentSession.SendTo(MessageHead.S_AUDIO_START,
                new AudioOptionsPacket()
                {
                    SamplesPerSecond = samplesPerSecond,
                    BitsPerSample = bitsPerSample,
                    Channels = channels
                });
        }

        /// <summary>
        /// 发送声音到远程
        /// </summary>
        /// <param name="payload"></param>
        public void SendVoiceDataToRemote(byte[] payload)
        {
            CurrentSession.SendTo(MessageHead.S_AUDIO_DATA, payload);
        }


        /// <summary>
        /// 设置远程启用发送语音流
        /// </summary>
        /// <param name="enabled"></param>
        public void SetRemotePlayerStreamEnabled(bool enabled)
        {
            CurrentSession.SendTo(MessageHead.S_AUDIO_DEIVCE_ONOFF, enabled ? "1" : "0");
        }
    }
}