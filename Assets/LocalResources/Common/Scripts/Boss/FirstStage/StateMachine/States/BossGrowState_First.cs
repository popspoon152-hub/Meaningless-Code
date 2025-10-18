using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGrowState_First : IBossStateFirstStage
{
    private BossFirstStateMachine _stateMachine;

    // 进入状态时调用（初始化）
    public void EnterState(BossFirstStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        if(_stateMachine.Animator != null)
        {
            //_stateMachine.Animator.SetTrigger("Grow");
        }

        DoGrowOnce();
        _stateMachine.ChangeState(BossState.EatBeans);
    }

    // 每帧更新
    public void UpdateState()
    {

    }

    // 固定时间步长更新（物理相关）
    public void FixedUpdateState()
    {
        //跟随移动
        _stateMachine.SegmentsMove();
    }

    // 退出状态时调用（清理）
    public void ExitState()
    {
        _stateMachine = null;
    }

    // 处理动画事件
    public void OnAnimationEvent(string eventName)
    {

    }

    private void DoGrowOnce()
    {
        Transform segment = GameObject.Instantiate(_stateMachine.SegmentPrefab);
        segment.position = _stateMachine._segments[_stateMachine._segments.Count - 1].position;

        _stateMachine._segments.Add(segment);
    }
}
