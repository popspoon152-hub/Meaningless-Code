using CutyRoom.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class AudioCenterController : MonoBehaviour
{
    /// <summary>
    /// 根据AudioManager设定的音频通过寻找名称的方式来播放
    /// </summary>
    private void Awake()
    {
        //EventCenter.Ins.AddListener(SomethingState.On, HandleSomethingOn);
        //EventCenter.Ins.AddListener(SomethingState.Off, HandleSomethingOff);
    }

    //private void HandleSomethingOn(List<object> obj = null)
    //{
    //    EventCenter.Ins.Dispatch(EAudioControl.Play, new List<object> { "Wind" });
    //}

    //private void HandleSomethingOff(List<object> obj = null)
    //{
    //    EventCenter.Ins.Dispatch(EAudioControl.Stop, new List<object> { "Wind" });
    //}
}
