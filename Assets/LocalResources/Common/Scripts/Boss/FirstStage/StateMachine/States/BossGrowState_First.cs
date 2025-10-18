using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGrowState_First : IBossStateFirstStage
{
    private BossFirstStateMachine _stateMachine;

    // ����״̬ʱ���ã���ʼ����
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

    // ÿ֡����
    public void UpdateState()
    {

    }

    // �̶�ʱ�䲽�����£�������أ�
    public void FixedUpdateState()
    {
        //�����ƶ�
        _stateMachine.SegmentsMove();
    }

    // �˳�״̬ʱ���ã�����
    public void ExitState()
    {
        _stateMachine = null;
    }

    // �������¼�
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
