using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackIdleState_First : IBossStateFirstStage
{
    private BossFirstStateMachine _stateMachine;
    private Coroutine _AttackIdle;
    private Transform _chooseTrans;

    // 进入状态时调用（初始化）
    public void EnterState(BossFirstStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        //if(_stateMachine.Animator != null)
        //{
        //    _stateMachine.Animator.SetTrigger("AttackIdle");
        //}
        _AttackIdle = _stateMachine.StartCoroutine(AttackIdle());
    }

    private void ChooseBossPos()
    {
        int num = UnityEngine.Random.Range(0, 2);
        if (num == 0)
        {
            _chooseTrans = _stateMachine.IdleLeftTransfrom;
        }
        else 
        {
            _chooseTrans = _stateMachine.IdleRightTransfrom;
        }
    }

    private IEnumerator AttackIdle()
    {
        ChooseBossPos();

        //瞬移或移动


        yield return new WaitForSeconds(_stateMachine.IdleTime);

        if (UnityEngine.Object.FindObjectsOfType<Bean>() != null)
        {
            _stateMachine.ChangeState(BossState.EatBeans);
        }
        else
        {
            _stateMachine.AttackStateChoose();
        }
    }


    // 退出状态时调用（清理）
    public void ExitState()
    {
        _stateMachine = null;
        if (_AttackIdle != null)
        {
            _stateMachine.StopCoroutine(_AttackIdle);
            _AttackIdle = null;
        }
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
