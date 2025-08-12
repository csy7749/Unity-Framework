using UnityEngine;
using UnityEngine.Audio;
using YooAsset;

namespace UnityFramework
{
    /// <summary>
    /// 音频代理辅助器。
    /// </summary>
    public class AudioAgent
    {
        private int _instanceId;
        private AudioSource _source;
        private AudioData   _audioData;
        private IAudioModule    _audioModule;
        private IResourceModule _resourceModule;
        private Transform _transform;
        private float _volume = 1.0f;
        private float _duration;
        private float _fadeoutTimer;
        private const float FADEOUT_DURATION = 0.2f;
        private bool _inPool;

        /// <summary>
        /// 运行时状态。
        /// </summary>
        AudioAgentRuntimeState _audioAgentRuntimeState = AudioAgentRuntimeState.None;

        /// <summary>
        /// 加载请求（当播放中又来了新的 Load）。
        /// </summary>
        class LoadRequest
        {
            public string Path;
            public bool   BAsync;
            public bool   BInPool;
        }
        LoadRequest _pendingLoad = null;

        public int      InstanceId => _instanceId;
        public AudioData AudioData => _audioData;

        public float Volume
        {
            set { if (_source != null) { _volume = value; _source.volume = _volume; } }
            get => _volume;
        }

        public bool  IsFree   => _source == null || _audioAgentRuntimeState == AudioAgentRuntimeState.End;
        public float Duration => _duration;

        public float Length => (_source != null && _source.clip != null) ? _source.clip.length : 0f;

        public Vector3 Position { get => _transform.position; set => _transform.position = value; }

        public bool IsLoop
        {
            get => _source != null && _source.loop;
            set { if (_source != null) _source.loop = value; }
        }

        internal bool IsPlaying => _source != null && _source.isPlaying;

        public AudioSource AudioResource() => _source;

        public static AudioAgent Create(string path, bool bAsync, AudioCategory audioCategory, bool bInPool = false)
        {
            var agent = new AudioAgent();
            agent.Init(audioCategory, 0);
            agent.Load(path, bAsync, bInPool);
            return agent;
        }

        public void Init(AudioCategory audioCategory, int index = 0)
        {
            _audioModule = ModuleSystem.GetModule<IAudioModule>();
            _resourceModule = ModuleSystem.GetModule<IResourceModule>();

            GameObject host = new GameObject(Utility.Text.Format("Audio Agent Helper - {0} - {1}", audioCategory.AudioMixerGroup.name, index));
            host.transform.SetParent(audioCategory.InstanceRoot);
            host.transform.localPosition = Vector3.zero;
            _transform = host.transform;

            _source = host.AddComponent<AudioSource>();
            _source.playOnAwake = false;

            AudioMixerGroup[] groups = audioCategory.AudioMixer
                .FindMatchingGroups(Utility.Text.Format("Master/{0}/{1}", audioCategory.AudioMixerGroup.name, $"{audioCategory.AudioMixerGroup.name} - {index}"));
            _source.outputAudioMixerGroup = groups.Length > 0 ? groups[0] : audioCategory.AudioMixerGroup;

            _source.rolloffMode = audioCategory.AudioGroupConfig.audioRolloffMode;
            _source.minDistance = audioCategory.AudioGroupConfig.minDistance;
            _source.maxDistance = audioCategory.AudioGroupConfig.maxDistance;

            _instanceId = _source.GetInstanceID();
        }

        public void Load(string path, bool bAsync, bool bInPool = false)
        {
            _inPool = bInPool;

            if (_audioAgentRuntimeState == AudioAgentRuntimeState.None || _audioAgentRuntimeState == AudioAgentRuntimeState.End)
            {
                _duration = 0;
                if (string.IsNullOrEmpty(path)) return;

                // 命中池：不要复用池里句柄对象，仅将其作为“已缓存”的提示，自己再申请新句柄（YooAsset 会命中缓存）
                if (bInPool && _audioModule.AudioClipPool.TryGetValue(path, out var pooled))
                {
                    if (bAsync)
                    {
                        _audioAgentRuntimeState = AudioAgentRuntimeState.Loading;
                        var handle = _resourceModule.LoadAssetAsyncHandle<AudioClip>(path);
                        if (handle.IsDone) OnAssetLoadComplete(handle);
                        else handle.Completed += OnAssetLoadComplete;
                        return;
                    }
                    else
                    {
                        var handle = _resourceModule.LoadAssetSyncHandle<AudioClip>(path);
                        OnAssetLoadComplete(handle);
                        return;
                    }
                }

                if (bAsync)
                {
                    _audioAgentRuntimeState = AudioAgentRuntimeState.Loading;
                    var handle = _resourceModule.LoadAssetAsyncHandle<AudioClip>(path);
                    handle.Completed += OnAssetLoadComplete;
                }
                else
                {
                    var handle = _resourceModule.LoadAssetSyncHandle<AudioClip>(path);
                    OnAssetLoadComplete(handle);
                }
            }
            else
            {
                // 正在播放/加载：记录 pending 请求；若在播放中则先淡出
                _pendingLoad = new LoadRequest { Path = path, BAsync = bAsync, BInPool = bInPool };
                if (_audioAgentRuntimeState == AudioAgentRuntimeState.Playing)
                    Stop(true);
            }
        }

        public void Stop(bool fadeout = false)
        {
            if (_source == null) return;

            if (fadeout)
            {
                _fadeoutTimer = FADEOUT_DURATION;
                _audioAgentRuntimeState = AudioAgentRuntimeState.FadingOut;
            }
            else
            {
                _source.Stop();
                _audioAgentRuntimeState = AudioAgentRuntimeState.End;
            }
        }

        public void Pause()   { _source?.Pause(); }
        public void UnPause() { _source?.UnPause(); }

        void OnAssetLoadComplete(AssetHandle handle)
        {
            // 若之前已经有待切换的加载请求，当前 handle 属于被淘汰的任务
            if (_pendingLoad != null)
            {
                if (!_inPool && handle != null) handle.Dispose(); // 非池化播放：释放无用句柄
                _audioAgentRuntimeState = AudioAgentRuntimeState.End;

                // 取出 pending 并立即触发
                string path = _pendingLoad.Path;
                bool bAsync = _pendingLoad.BAsync;
                bool bInPool = _pendingLoad.BInPool;
                _pendingLoad = null;
                Load(path, bAsync, bInPool);
                return;
            }

            if (handle == null || handle.Status != EOperationStatus.Succeed || handle.AssetObject == null)
            {
                _audioAgentRuntimeState = AudioAgentRuntimeState.End;
                return;
            }

            if (_audioData != null)
            {
                AudioData.DeAlloc(_audioData);
                _audioData = null;
            }

            _audioData = AudioData.Alloc(handle, _inPool);
            _source.clip = handle.AssetObject as AudioClip;

            if (_source.clip != null)
            {
                _source.Play();
                _audioAgentRuntimeState = AudioAgentRuntimeState.Playing;
            }
            else
            {
                _audioAgentRuntimeState = AudioAgentRuntimeState.End;
            }
        }

        public void Update(float elapseSeconds)
        {
            if (_audioAgentRuntimeState == AudioAgentRuntimeState.Playing)
            {
                if (!_source.isPlaying)
                    _audioAgentRuntimeState = AudioAgentRuntimeState.End;
            }
            else if (_audioAgentRuntimeState == AudioAgentRuntimeState.FadingOut)
            {
                if (_fadeoutTimer > 0f)
                {
                    _fadeoutTimer -= elapseSeconds;
                    _source.volume = _volume * Mathf.Clamp01(_fadeoutTimer / FADEOUT_DURATION);
                }
                else
                {
                    Stop();
                    if (_pendingLoad != null)
                    {
                        string path = _pendingLoad.Path;
                        bool bAsync = _pendingLoad.BAsync;
                        bool bInPool = _pendingLoad.BInPool;
                        _pendingLoad = null;
                        Load(path, bAsync, bInPool);
                    }
                    _source.volume = _volume;
                }
            }

            _duration += elapseSeconds;
        }

        public void Destroy()
        {
            if (_transform != null)
                Object.Destroy(_transform.gameObject);

            if (_audioData != null)
                AudioData.DeAlloc(_audioData);

            _transform = null;
            _source = null;
            _audioData = null;
        }
    }
}
