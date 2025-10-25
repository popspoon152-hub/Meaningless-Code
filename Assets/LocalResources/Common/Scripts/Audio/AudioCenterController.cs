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
        #region Player
        EventCenter.Ins.AddListener(EPlayerFirstAttack.on, HandlePlayerFirstAttackOn);
        EventCenter.Ins.AddListener(EPlayerFirstAttack.off, HandlePlayerFirstAttackOff);

        EventCenter.Ins.AddListener(EPlayerSecondAttack.on, HandlePlayerSecondAttackOn);
        EventCenter.Ins.AddListener(EPlayerSecondAttack.off, HandlePlayerSecondAttackOff);

        EventCenter.Ins.AddListener(EPlayerThirdAttack.on, HandlePlayerThirdAttackOn);
        EventCenter.Ins.AddListener(EPlayerThirdAttack.off, HandlePlayerThirdAttackOff);

        EventCenter.Ins.AddListener(EPlayerDash.on, HandlePlayerDashOn);
        EventCenter.Ins.AddListener(EPlayerDash.off, HandlePlayerDashOff);

        EventCenter.Ins.AddListener(EPlayerBack.on, HandlePlayerBackOn);

        EventCenter.Ins.AddListener(EPlayerLaser.on, HandlePlayerLaserOn);

        EventCenter.Ins.AddListener(EPlayerLaser.on, HandlePlayerLaserOn);

        EventCenter.Ins.AddListener(EPlayerBirth.on, HandlePlayerBirthOn);

        EventCenter.Ins.AddListener(EPlayerHurt.on, HandlePlayerHurtOn);

        EventCenter.Ins.AddListener(EPlayerDeath.on, HandlePlayerDeathOn);
        #endregion

        #region Boss
        EventCenter.Ins.AddListener(EBossDash.on, HandleBossDashOn);

        EventCenter.Ins.AddListener(EBossEatBeansMove.on, HandleBossEatBeansMoveOn);

        EventCenter.Ins.AddListener(EBossEatBean.on, HandleBossEatBeanOn);

        EventCenter.Ins.AddListener(EBossRangedAttack.on, HandleBossRangedAttackOn);
        #endregion
    }

    #region PlayerAudios
    /// <summary>
    /// 一段平a
    /// </summary>
    /// <param name="list"></param>
    private void HandlePlayerFirstAttackOn(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Play, new List<object> { "一二段平A" });
    }

    private void HandlePlayerFirstAttackOff(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Stop, new List<object> { "一二段平A" });
    }

    /// <summary>
    /// 二段平a
    /// </summary>
    /// <param name="list"></param>
    private void HandlePlayerSecondAttackOn(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Play, new List<object> { "一二段平A" });
    }

    private void HandlePlayerSecondAttackOff(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Stop, new List<object> { "一二段平A" });
    }

    /// <summary>
    /// 三段平a
    /// </summary>
    /// <param name="list"></param>
    private void HandlePlayerThirdAttackOn(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Play, new List<object> { "第三段平A" });
    }

    private void HandlePlayerThirdAttackOff(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Stop, new List<object> { "第三段平A" });
    }

    /// <summary>
    /// 冲刺
    /// </summary>
    /// <param name="list"></param>
    private void HandlePlayerDashOn(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Play, new List<object> { "冲刺音效" });
    }

    private void HandlePlayerDashOff(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Stop, new List<object> { "冲刺音效" });
    }

    /// <summary>
    /// 回溯
    /// </summary>
    /// <param name="list"></param>
    private void HandlePlayerBackOn(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Play, new List<object> { "回溯音效" });
    }

    /// <summary>
    /// 激光
    /// </summary>
    /// <param name="list"></param>
    private void HandlePlayerLaserOn(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Play, new List<object> { "激光音效" });
    }

    /// <summary>
    /// 角色出生
    /// </summary>
    /// <param name="list"></param>
    private void HandlePlayerBirthOn(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Play, new List<object> { "角色出生音效" });
    }

    /// <summary>
    /// 角色受伤
    /// </summary>
    /// <param name="list"></param>
    private void HandlePlayerHurtOn(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Play, new List<object> { "角色受伤" });
    }

    /// <summary>
    /// 角色死亡
    /// </summary>
    /// <param name="list"></param>
    private void HandlePlayerDeathOn(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Play, new List<object> { "角色受伤" });
    }
    #endregion

    #region BossAudio
    /// <summary>
    /// Boss冲刺
    /// </summary>
    /// <param name="list"></param>
    private void HandleBossDashOn(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Play, new List<object> { "boss冲刺音效+接近豆子加速音效" });
    }

    /// <summary>
    /// Boss吃豆移动
    /// </summary>
    /// <param name="list"></param>
    private void HandleBossEatBeansMoveOn(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Play, new List<object> { "boss冲刺音效+接近豆子加速音效" });
    }

    /// <summary>
    /// Boss吃豆
    /// </summary>
    /// <param name="list"></param>
    private void HandleBossEatBeanOn(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Play, new List<object> { "boss吃豆音效" });
    }

    /// <summary>
    /// boss弹幕发射
    /// </summary>
    /// <param name="list"></param>
    private void HandleBossRangedAttackOn(List<object> list)
    {
        EventCenter.Ins.Dispatch(EAudioControl.Play, new List<object> { "boss弹幕发射" });
    }
    #endregion
}
