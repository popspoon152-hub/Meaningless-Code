using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossDieState_First : IBossStateFirstStage
{
    private BossFirstStateMachine _stateMachine;

    private Coroutine _bossDie;

    // 进入状态时调用（初始化）
    public void EnterState(BossFirstStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        //if (_stateMachine.Animator != null)
        //{
        //    _stateMachine.Animator.SetTrigger("Die");
        //}

        _bossDie = _stateMachine.StartCoroutine(BossDie());
    }

    private IEnumerator BossDie()
    {
        yield return new WaitForSeconds(_stateMachine.DieInvulnerableTime);

        //跳转到gal环节
        SceneManager.LoadScene("GalPage");
    }

    // 退出状态时调用（清理）
    public void ExitState()
    {
        if (_bossDie != null)
        {
            _stateMachine.StopCoroutine(_bossDie);
            _bossDie = null;
        }
        _stateMachine = null;
    }











    // 每帧更新
    public void UpdateState()
    {

    }

    // 固定时间步长更新（物理相关）
    public void FixedUpdateState()
    {

    }

    // 处理动画事件
    public void OnAnimationEvent(string eventName)
    {

    }
}
