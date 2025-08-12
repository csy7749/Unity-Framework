using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using YooAsset;

namespace UnityFramework
{
    /// <summary>
    /// 音频模块：
    /// - 提供主音量/总开关与分类音量（Mixer dB）控制
    /// - 支持资源预热与对象池
    /// - 设备/配置切换时自动重放音量设置（避免切换蓝牙导致静音失效）
    /// </summary>
    internal class AudioModule : Module, IAudioModule, IUpdateModule
    {
        public const string MUSIC_VOLUME_NAME    = "MusicVolume";
        public const string SOUND_VOLUME_NAME    = "SoundVolume";       // ★ 新增统一常量
        public const string UI_SOUND_VOLUME_NAME = "UISoundVolume";
        public const string VOICE_VOLUME_NAME    = "VoiceVolume";

        private AudioMixer _audioMixer;
        private Transform  _instanceRoot = null;
        private AudioGroupConfig[] _audioGroupConfigs = null;

        private float _volume = 1f;
        private bool  _enable = true;
        private readonly AudioCategory[] _audioCategories = new AudioCategory[(int)AudioType.Max];
        private readonly float[] _categoriesVolume        = new float[(int)AudioType.Max];
        private bool _bUnityAudioDisabled = false;

        private IResourceModule _resourceModule;

        #region 事件订阅（设备切换/配置变化时重放音量）

        public override void OnInit()
        {
            _resourceModule = ModuleSystem.GetModule<IResourceModule>();
            Initialize(Settings.AudioSetting.audioGroupConfigs);

            // 订阅音频配置变化（设备切换 / 采样率等）
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
            // 订阅焦点变化（某些设备在切换时会失焦再恢复）
            Application.focusChanged += OnAppFocusChanged;
        }

        public override void Shutdown()
        {
            StopAll(fadeout: false);
            CleanSoundPool();

            // 销毁分类与根节点
            for (int i = 0; i < (int)AudioType.Max; ++i)
            {
                var cat = _audioCategories[i];
                if (cat != null)
                {
                    foreach (var ag in cat.AudioAgents) ag?.Destroy();
                    cat.Dispose();
                    _audioCategories[i] = null;
                }
            }

            AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
            Application.focusChanged -= OnAppFocusChanged;
        }

        private void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            // 无论是否换设备，统一重放设置最安全
            ApplyAllVolumes();
        }

        private void OnAppFocusChanged(bool hasFocus)
        {
            if (hasFocus) ApplyAllVolumes();
        }

        #endregion

        #region IAudioModule 属性

        public Dictionary<string, AssetHandle> AudioClipPool { get; private set; } = new Dictionary<string, AssetHandle>();

        /// <summary>
        /// 音频混响器。
        /// </summary>
        public AudioMixer AudioMixer => _audioMixer;

        /// <summary>
        /// 实例化根节点。
        /// </summary>
        public Transform InstanceRoot => _instanceRoot;

        /// <summary>
        /// 总音量（0~1），与总开关一起决定 AudioListener.volume。
        /// </summary>
        public float Volume
        {
            get => _bUnityAudioDisabled ? 0f : _volume;
            set
            {
                if (_bUnityAudioDisabled) return;
                _volume = Mathf.Clamp01(value);
                ApplyAllVolumes(); // 统一重放主音量
            }
        }

        /// <summary>
        /// 总开关。
        /// </summary>
        public bool Enable
        {
            get => !_bUnityAudioDisabled && _enable;
            set
            {
                if (_bUnityAudioDisabled) return;
                _enable = value;
                ApplyAllVolumes();
            }
        }

        /// <summary>
        /// 音乐音量（分类 dB）。
        /// </summary>
        public float MusicVolume
        {
            get => _bUnityAudioDisabled ? 0f : _categoriesVolume[(int)AudioType.Music];
            set
            {
                if (_bUnityAudioDisabled) return;
                var v = Mathf.Clamp(value, 0.0001f, 1f);
                _categoriesVolume[(int)AudioType.Music] = v;
                if (_audioMixer != null)
                    _audioMixer.SetFloat(MUSIC_VOLUME_NAME, Mathf.Log10(v) * 20f);
            }
        }

        /// <summary>
        /// 音效音量（分类 dB）。
        /// </summary>
        public float SoundVolume
        {
            get => _bUnityAudioDisabled ? 0f : _categoriesVolume[(int)AudioType.Sound];
            set
            {
                if (_bUnityAudioDisabled) return;
                var v = Mathf.Clamp(value, 0.0001f, 1f);
                _categoriesVolume[(int)AudioType.Sound] = v;
                if (_audioMixer != null)
                    _audioMixer.SetFloat(SOUND_VOLUME_NAME, Mathf.Log10(v) * 20f);
            }
        }

        /// <summary>
        /// UI 音效音量（分类 dB）。
        /// </summary>
        public float UISoundVolume
        {
            get => _bUnityAudioDisabled ? 0f : _categoriesVolume[(int)AudioType.UISound];
            set
            {
                if (_bUnityAudioDisabled) return;
                var v = Mathf.Clamp(value, 0.0001f, 1f);
                _categoriesVolume[(int)AudioType.UISound] = v;
                if (_audioMixer != null)
                    _audioMixer.SetFloat(UI_SOUND_VOLUME_NAME, Mathf.Log10(v) * 20f);
            }
        }

        /// <summary>
        /// 语音音量（分类 dB）。
        /// </summary>
        public float VoiceVolume
        {
            get => _bUnityAudioDisabled ? 0f : _categoriesVolume[(int)AudioType.Voice];
            set
            {
                if (_bUnityAudioDisabled) return;
                var v = Mathf.Clamp(value, 0.0001f, 1f);
                _categoriesVolume[(int)AudioType.Voice] = v;
                if (_audioMixer != null)
                    _audioMixer.SetFloat(VOICE_VOLUME_NAME, Mathf.Log10(v) * 20f);
            }
        }

        /// <summary>
        /// 音乐开关（语义：关闭=拉低到 -80dB，同时标记分类禁用）。
        /// </summary>
        public bool MusicEnable
        {
            get
            {
                if (_bUnityAudioDisabled) return false;
                if (_audioMixer != null && _audioMixer.GetFloat(MUSIC_VOLUME_NAME, out var db))
                    return db > -80f;
                return false;
            }
            set
            {
                if (_bUnityAudioDisabled) return;
                _audioCategories[(int)AudioType.Music].Enable = value;
                if (_audioMixer != null)
                {
                    if (value)
                        _audioMixer.SetFloat(MUSIC_VOLUME_NAME, Mathf.Log10(_categoriesVolume[(int)AudioType.Music]) * 20f);
                    else
                        _audioMixer.SetFloat(MUSIC_VOLUME_NAME, -80f);
                }
            }
        }

        /// <summary>
        /// 音效开关（拦截播放）。
        /// </summary>
        public bool SoundEnable
        {
            get => !_bUnityAudioDisabled && _audioCategories[(int)AudioType.Sound].Enable;
            set { if (!_bUnityAudioDisabled) _audioCategories[(int)AudioType.Sound].Enable = value; }
        }

        /// <summary>
        /// UI 音效开关（拦截播放）。
        /// </summary>
        public bool UISoundEnable
        {
            get => !_bUnityAudioDisabled && _audioCategories[(int)AudioType.UISound].Enable;
            set { if (!_bUnityAudioDisabled) _audioCategories[(int)AudioType.UISound].Enable = value; }
        }

        /// <summary>
        /// 语音开关（拦截播放）。
        /// </summary>
        public bool VoiceEnable
        {
            get => !_bUnityAudioDisabled && _audioCategories[(int)AudioType.Voice].Enable;
            set { if (!_bUnityAudioDisabled) _audioCategories[(int)AudioType.Voice].Enable = value; }
        }

        #endregion

        #region 初始化 / 重启

        /// <summary>
        /// 初始化音频模块。
        /// </summary>
        public void Initialize(AudioGroupConfig[] audioGroupConfigs, Transform instanceRoot = null, AudioMixer audioMixer = null)
        {
            if (_instanceRoot == null)
                _instanceRoot = instanceRoot;

            if (audioGroupConfigs == null)
                throw new GameFrameworkException("AudioGroupConfig[] is invalid.");

            _audioGroupConfigs = audioGroupConfigs;

            if (_instanceRoot == null)
            {
                _instanceRoot = new GameObject("[AudioModule Instances]").transform;
                _instanceRoot.localScale = Vector3.one;
                UnityEngine.Object.DontDestroyOnLoad(_instanceRoot);
            }

#if UNITY_EDITOR
            // Editor 内部禁用 Unity Audio 的场景下，直接退出
            try
            {
                TypeInfo ti = typeof(AudioSettings).GetTypeInfo();
                PropertyInfo pi = ti.GetDeclaredProperty("unityAudioDisabled");
                _bUnityAudioDisabled = (bool)pi.GetValue(null);
                if (_bUnityAudioDisabled) return;
            }
            catch (Exception e)
            {
                Log.Warning($"Check unityAudioDisabled failed: {e}");
            }
#endif
            _audioMixer = audioMixer ?? Resources.Load<AudioMixer>("AudioMixer");
            if (_audioMixer == null)
                throw new GameFrameworkException("AudioMixer not found. Please place an AudioMixer at Resources/AudioMixer or pass one via Initialize().");

            for (int index = 0; index < (int)AudioType.Max; ++index)
            {
                AudioType audioType = (AudioType)index;
                var audioGroupConfig = _audioGroupConfigs.First(t => t.AudioType == audioType);
                _audioCategories[index] = new AudioCategory(audioGroupConfig.AgentHelperCount, _audioMixer, audioGroupConfig);
                _categoriesVolume[index] = Mathf.Clamp(audioGroupConfig.Volume, 0.0001f, 1f);
            }

            // ★ 初始化完毕立即重放一次所有音量（避免刚启动时 dB 未生效）
            ApplyAllVolumes();
        }

        /// <summary>
        /// 重启音频模块（清理一切并重新 Initialize）。
        /// </summary>
        public void Restart()
        {
            if (_bUnityAudioDisabled) return;

            CleanSoundPool();

            for (int i = 0; i < (int)AudioType.Max; ++i)
            {
                var cat = _audioCategories[i];
                if (cat != null)
                {
                    foreach (var ag in cat.AudioAgents) ag?.Destroy();
                    cat.Dispose();
                    _audioCategories[i] = null;
                }
            }
            Initialize(_audioGroupConfigs, _instanceRoot, _audioMixer);
        }

        #endregion

        #region 播放 / 停止

        /// <summary>
        /// 播放（若超最大并发，复用当前播放时长最长的 Agent，先淡出）。
        /// </summary>
        public AudioAgent Play(AudioType type, string path, bool bLoop = false, float volume = 1.0f, bool bAsync = false, bool bInPool = false)
        {
            if (_bUnityAudioDisabled) return null;

            var agent = _audioCategories[(int)type].Play(path, bAsync, bInPool);
            if (agent != null)
            {
                agent.IsLoop = bLoop;
                agent.Volume = volume;
            }
            return agent;
        }

        public void Stop(AudioType type, bool fadeout)
        {
            if (_bUnityAudioDisabled) return;
            _audioCategories[(int)type].Stop(fadeout);
        }

        public void StopAll(bool fadeout)
        {
            if (_bUnityAudioDisabled) return;
            for (int i = 0; i < (int)AudioType.Max; ++i)
                _audioCategories[i]?.Stop(fadeout);
        }

        #endregion

        #region 资源池（预热/清理）

        /// <summary>
        /// 预先加载 AudioClip，并放入对象池（并发安全，失败不泄漏）。
        /// </summary>
        public void PutInAudioPool(List<string> list)
        {
            if (_bUnityAudioDisabled || list == null) return;

            foreach (string path in list)
            {
                if (!AudioClipPool.ContainsKey(path))
                {
                    var handle = _resourceModule.LoadAssetAsyncHandle<AudioClip>(path);
                    handle.Completed += h =>
                    {
                        if (h.Status == EOperationStatus.Succeed)
                        {
                            if (!AudioClipPool.TryAdd(path, h))
                                h.Dispose(); // 已存在，释放重复句柄
                        }
                        else
                        {
                            h.Dispose();   // 失败释放句柄，避免泄漏
                        }
                    };
                }
            }
        }

        /// <summary>
        /// 将部分 AudioClip 从对象池移出（若正被播放，建议延迟回收）。
        /// </summary>
        public void RemoveClipFromPool(List<string> list)
        {
            if (_bUnityAudioDisabled || list == null) return;

            foreach (string path in list)
            {
                if (AudioClipPool.TryGetValue(path, out var h))
                {
                    h.Dispose();
                    AudioClipPool.Remove(path);
                }
            }
        }

        /// <summary>
        /// 清空 AudioClip 对象池。
        /// </summary>
        public void CleanSoundPool()
        {
            if (_bUnityAudioDisabled) return;

            foreach (var kv in AudioClipPool)
                kv.Value.Dispose();
            AudioClipPool.Clear();
        }

        #endregion

        #region Update

        /// <summary>
        /// 音频模块轮询。
        /// </summary>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (_bUnityAudioDisabled) return;

            foreach (var audioCategory in _audioCategories)
                audioCategory?.Update(elapseSeconds);
        }

        #endregion

        #region 内部：统一重放音量设置

        /// <summary>
        /// 将当前「主开关/主音量」与「分类音量」统一应用到 AudioListener 与 Mixer。
        /// - 设备/配置变化、初始化后、属性 Setter 调用此方法
        /// </summary>
        private void ApplyAllVolumes()
        {
            if (_bUnityAudioDisabled) return;

            // 主音量 + 总开关（设备切换后可能被 Unity 重置，这里强制重放）
            AudioListener.volume = _enable ? _volume : 0f;

            if (_audioMixer == null) return;

            // 分类音量（dB）
            _audioMixer.SetFloat(MUSIC_VOLUME_NAME,    Mathf.Log10(Mathf.Clamp(_categoriesVolume[(int)AudioType.Music],   0.0001f, 1f)) * 20f);
            _audioMixer.SetFloat(SOUND_VOLUME_NAME,    Mathf.Log10(Mathf.Clamp(_categoriesVolume[(int)AudioType.Sound],   0.0001f, 1f)) * 20f);
            _audioMixer.SetFloat(UI_SOUND_VOLUME_NAME, Mathf.Log10(Mathf.Clamp(_categoriesVolume[(int)AudioType.UISound], 0.0001f, 1f)) * 20f);
            _audioMixer.SetFloat(VOICE_VOLUME_NAME,    Mathf.Log10(Mathf.Clamp(_categoriesVolume[(int)AudioType.Voice],   0.0001f, 1f)) * 20f);

            // 若音乐分类被关闭，强制拉到 -80dB（“静音不打断”语义）
            if (_audioCategories[(int)AudioType.Music] != null &&
                !_audioCategories[(int)AudioType.Music].Enable)
            {
                _audioMixer.SetFloat(MUSIC_VOLUME_NAME, -80f);
            }
        }

        #endregion
    }
}
