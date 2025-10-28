using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class StartPageSettings : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    private const string VolumePrefKey = "GameVolume";

    private void Start()
    {
        // 从本地读取保存的音量值
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);

        // 初始化 slider
        volumeSlider.value = savedVolume;

        // 应用音量
        SetVolume(savedVolume);

        // 注册事件监听
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumePrefKey, value);
    }
}
