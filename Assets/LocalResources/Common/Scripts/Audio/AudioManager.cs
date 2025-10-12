using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Audio
{
    public AudioClip Clip;
    public bool Loop;
    public bool PlayOnAwake;
    public AudioTag Tag;
}

public enum AudioTag
{
    BGM,
    FX,
}

public enum EAudioControl
{
    Play,
    Pause,
    Stop,
}


public class AudioManager : MonoBehaviour
{
    public List<Audio> audioClips = new List<Audio>();

    // 存储子物体和对应的AudioSource组件的字典
    private static Dictionary<string, AudioSource> _audioSources;

    private void Awake()
    {
        PlayerSettings.Ins.LoadPlayerSettings();
        // 初始化字典
        _audioSources = new Dictionary<string, AudioSource>();
        var bgmVolume = PlayerSettings.Ins.BGMVolume / 100f;
        var fxVolume = PlayerSettings.Ins.FXVolume / 100f;
        // 为每个音频文件创建一个子物体和AudioSource组件
        foreach (Audio clip in audioClips)
        {
            GameObject audioObject = new GameObject(clip.Clip.name);
            audioObject.transform.parent = transform; // 设置父物体为音频管理器
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();

            audioSource.clip = clip.Clip;               // 设置音频剪辑
            audioSource.playOnAwake = clip.PlayOnAwake; // 禁止自动播放
            audioSource.volume = clip.Tag == AudioTag.BGM ? bgmVolume : fxVolume;
            audioSource.loop = clip.Loop;

            // 将AudioSource添加到字典中
            _audioSources.Add(clip.Clip.name, audioSource);
            if (audioSource.playOnAwake)
            {
                audioSource.Play();
            }
        }
        EventCenter.Ins.AddListener(EAudioControl.Play, PlayAudio);
        EventCenter.Ins.AddListener(EAudioControl.Pause, PauseAudio);
        EventCenter.Ins.AddListener(EAudioControl.Stop, StopAudio);
    }

    // 一个简单的播放音频的方法，可以通过音频名称来播放
    public void PlayAudio(List<object> args)
    {
        PlayAudio((string)args[0]);
    }

    public void PlayAudio(string audioName)
    {
        if (_audioSources.ContainsKey(audioName))
        {
            _audioSources[audioName].Play();
        }
    }

    public void StopAudio(List<object> args)
    {
        StopAudio((string)args[0]);
    }

    // 一个简单的停止音频的方法，可以通过音频名称来停止
    public void StopAudio(string audioName)
    {
        if (_audioSources.ContainsKey(audioName))
        {
            _audioSources[audioName].Stop();
        }
    }

    public void PauseAudio(List<object> args)
    {
        PauseAudio((string)args[0]);
    }

    // 一个简单的停止音频的方法，可以通过音频名称来停止
    public void PauseAudio(string audioName)
    {
        if (_audioSources.ContainsKey(audioName))
        {
            _audioSources[audioName].Pause();
        }
    }
}
