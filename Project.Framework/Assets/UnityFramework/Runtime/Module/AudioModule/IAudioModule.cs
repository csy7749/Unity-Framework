using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using YooAsset;

namespace UnityFramework
{
    public interface IAudioModule
    {
        float Volume { get; set; }
        bool  Enable { get; set; }

        float MusicVolume   { get; set; }
        float SoundVolume   { get; set; }
        float UISoundVolume { get; set; }
        float VoiceVolume   { get; set; }

        bool MusicEnable   { get; set; }
        bool SoundEnable   { get; set; }
        bool UISoundEnable { get; set; }
        bool VoiceEnable   { get; set; }

        AudioMixer AudioMixer  { get; }
        Transform  InstanceRoot { get; }

        Dictionary<string, AssetHandle> AudioClipPool { get; }

        void Initialize(AudioGroupConfig[] audioGroupConfigs, Transform instanceRoot = null, AudioMixer audioMixer = null);
        void Restart();

        AudioAgent Play(AudioType type, string path, bool bLoop = false, float volume = 1.0f, bool bAsync = false, bool bInPool = false);

        void Stop   (AudioType type, bool fadeout);
        void StopAll(bool fadeout);

        void PutInAudioPool(List<string> list);
        void RemoveClipFromPool(List<string> list);
        void CleanSoundPool();
    }
}