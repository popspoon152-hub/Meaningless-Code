using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGrowState_First : IBossStateFirstStage
{
    private BossFirstStateMachine _stateMachine;

    private Coroutine _doGrowOnce;
    // 进入状态时调用（初始化）
    public void EnterState(BossFirstStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        //if(_stateMachine.Animator != null)
        //{
        //    _stateMachine.Animator.SetTrigger("Grow");
        //}


        if (_stateMachine.Segments.Count == _stateMachine.MaxSnakeSegments)
        {
            _stateMachine.IsMove = true;
            _stateMachine.ChangeState(BossState.EatBeansRangedAttack);
        }
        else
        {
            _doGrowOnce = _stateMachine.StartCoroutine(DoGrowOnce());
        }
    }

    private IEnumerator DoGrowOnce()
    {
        _stateMachine.IsMove = false;
        
        Transform segment = GameObject.Instantiate(_stateMachine.SegmentPrefab);
        segment.position = _stateMachine.Segments[_stateMachine.Segments.Count - 1].position;

        _stateMachine.Segments.Add(segment);

        yield return new WaitForSeconds(_stateMachine.StateInvulnerableTime);

        _stateMachine.IsMove = true;

        _stateMachine.ChangeState(BossState.EatBeans);
    }

    // 固定时间步长更新（物理相关）
    public void FixedUpdateState()
    {
        if (_stateMachine == null) return;

        //跟随移动
        if (_stateMachine.IsMove) _stateMachine.SegmentsMove();
    }

    // 退出状态时调用（清理）
    public void ExitState()
    {
        if (_doGrowOnce != null)
        {
            _stateMachine.StopCoroutine(_doGrowOnce);
            _doGrowOnce = null;
        }
        _stateMachine = null;
    }










    // 每帧更新
    public void UpdateState()
    {

    }
    // 处理动画事件
    public void OnAnimationEvent(string eventName)
    {

    }
}
