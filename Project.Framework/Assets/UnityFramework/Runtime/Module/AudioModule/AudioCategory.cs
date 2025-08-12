using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace UnityFramework
{
    /// <summary>
    /// 音频轨道（类别）。
    /// </summary>
    [Serializable]
    public class AudioCategory
    {
        [SerializeField] private AudioMixer audioMixer = null;

        public  List<AudioAgent> AudioAgents;
        private readonly AudioMixerGroup _audioMixerGroup;
        private AudioGroupConfig _audioGroupConfig;
        private int  _maxChannel;
        private bool _bEnable = true;

        public AudioMixer      AudioMixer      => audioMixer;
        public AudioMixerGroup AudioMixerGroup => _audioMixerGroup;
        public AudioGroupConfig AudioGroupConfig => _audioGroupConfig;
        public Transform InstanceRoot { private set; get; }

        /// <summary>
        /// 是否启用（关闭会停止全部 Agent）。
        /// </summary>
        public bool Enable
        {
            get => _bEnable;
            set
            {
                if (_bEnable != value)
                {
                    _bEnable = value;
                    if (!_bEnable)
                    {
                        foreach (var audioAgent in AudioAgents)
                            audioAgent?.Stop();
                    }
                }
            }
        }

        public AudioCategory(int maxChannel, AudioMixer audioMixer, AudioGroupConfig audioGroupConfig)
        {
            var audioModule = ModuleSystem.GetModule<IAudioModule>();

            this.audioMixer = audioMixer;
            _maxChannel = maxChannel;
            _audioGroupConfig = audioGroupConfig;

            AudioMixerGroup[] groups = audioMixer.FindMatchingGroups(Utility.Text.Format("Master/{0}", audioGroupConfig.AudioType.ToString()));
            _audioMixerGroup = groups.Length > 0 ? groups[0] : audioMixer.FindMatchingGroups("Master")[0];

            AudioAgents = new List<AudioAgent>(Mathf.Max(4, maxChannel));
            InstanceRoot = new GameObject(Utility.Text.Format("Audio Category - {0}", _audioMixerGroup.name)).transform;
            InstanceRoot.SetParent(audioModule.InstanceRoot);
            InstanceRoot.localPosition = Vector3.zero;

            for (int index = 0; index < _maxChannel; index++)
            {
                var audioAgent = new AudioAgent();
                audioAgent.Init(this, index);
                AudioAgents.Add(audioAgent);
            }
        }

        /// <summary>
        /// 销毁分类根节点（用于 Restart/Shutdown）。
        /// </summary>
        public void Dispose()
        {
            if (InstanceRoot != null)
                UnityEngine.Object.Destroy(InstanceRoot.gameObject);
            InstanceRoot = null;
        }

        /// <summary>
        /// 增加音频并发通道。
        /// </summary>
        public void AddAudio(int num)
        {
            _maxChannel += num;
            for (int i = 0; i < num; i++)
                AudioAgents.Add(null);
        }

        /// <summary>
        /// 播放音频（空闲优先；否则复用播放最久的）。
        /// </summary>
        public AudioAgent Play(string path, bool bAsync, bool bInPool = false)
        {
            if (!_bEnable) return null;

            int   freeChannel = -1;
            float duration    = -1f;

            for (int i = 0; i < AudioAgents.Count; i++)
            {
                // ★ 修复：要判断 AudioAgents[i] 是否为 null
                if (AudioAgents[i] == null || AudioAgents[i].AudioData?.AssetHandle == null || AudioAgents[i].IsFree)
                {
                    freeChannel = i;
                    break;
                }
                else if (AudioAgents[i].Duration > duration)
                {
                    duration = AudioAgents[i].Duration;
                    freeChannel = i;
                }
            }

            if (freeChannel >= 0)
            {
                if (AudioAgents[freeChannel] == null)
                    AudioAgents[freeChannel] = AudioAgent.Create(path, bAsync, this, bInPool);
                else
                    AudioAgents[freeChannel].Load(path, bAsync, bInPool);

                return AudioAgents[freeChannel];
            }
            else
            {
                Log.Error($"Here is no channel to play audio {path}");
                return null;
            }
        }

        public void Stop(bool fadeout)
        {
            for (int i = 0; i < AudioAgents.Count; ++i)
                AudioAgents[i]?.Stop(fadeout);
        }

        public void Update(float elapseSeconds)
        {
            for (int i = 0; i < AudioAgents.Count; ++i)
                AudioAgents[i]?.Update(elapseSeconds);
        }
    }
}
