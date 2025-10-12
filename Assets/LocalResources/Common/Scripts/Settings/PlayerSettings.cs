using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
internal class PlayerSettings : SingletonPatternBase<PlayerSettings>
{
    public bool IsFullScreen { get; set; } = false;

    public int BGMVolume { get; set; } = 100;

    public int FXVolume { get; set; } = 100;

    public void SavePlayerSettings()
    {
        var json = JsonConvert.SerializeObject(this);
        File.WriteAllText(SettingsPath(), json);
    }

    public void LoadPlayerSettings()
    {
        var filePath = SettingsPath();
        if (!File.Exists(filePath))
        {
            GetDefaultSettings();
            SavePlayerSettings();
        }
        var json = File.ReadAllText(filePath);
        PlayerSettings ans = JsonConvert.DeserializeObject<PlayerSettings>(json);
        BGMVolume = ans.BGMVolume;
        FXVolume = ans.FXVolume;
        IsFullScreen = ans.IsFullScreen;
    }

    private void GetDefaultSettings()
    {
        BGMVolume = 100;
        FXVolume = 100;
        IsFullScreen = false;
    }

    private static string SettingsPath()
    {
        return Application.persistentDataPath + "/playerSettings.json";
    }
}
