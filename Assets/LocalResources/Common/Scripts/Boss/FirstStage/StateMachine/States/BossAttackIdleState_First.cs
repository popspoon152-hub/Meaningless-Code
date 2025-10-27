using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class BossAttackIdleState_First : IBossStateFirstStage
{
    private BossFirstStateMachine _stateMachine;
    private Coroutine _AttackIdle;
    private Transform _chooseTrans;
    private bool _isLeft;

    // 进入状态时调用（初始化）
    public void EnterState(BossFirstStateMachine stateMachine)
    {
        _stateMachine = stateMachine;

        //if(_stateMachine.Animator != null)
        //{
        //    _stateMachine.Animator.SetTrigger("AttackIdle");
        //}
        
        _stateMachine.IsMove = false;
        for(int i = 0; i < _stateMachine.Segments.Count; i++)
        {
            Rigidbody2D rb = _stateMachine.Segments[i].GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
        }

        _AttackIdle = _stateMachine.StartCoroutine(AttackIdle());
    }

    #region Idle
    private void ChooseBossPos()
    {
        int num = UnityEngine.Random.Range(0, 2);
        if (num == 0)
        {
            _isLeft = true;
            _chooseTrans = _stateMachine.IdleLeftTransfrom;
        }
        else 
        {
            _isLeft = false;
            _chooseTrans = _stateMachine.IdleRightTransfrom;
        }
    }

    private IEnumerator AttackIdle()
    {
        ChooseBossPos();

        _stateMachine.IsMove = false;
        //瞬移或移动
        if (TelePort(_chooseTrans.position))
        {
            yield return new WaitForSeconds(_stateMachine.IdleTime);

            // 检查是否有豆子存在
            Bean[] beans = UnityEngine.Object.FindObjectsOfType<Bean>();
            if (beans != null && beans.Length > 0)
            {
                _stateMachine.ChangeState(BossState.EatBeans);
            }
            else
            {
                //分为远程攻击，冲锋攻击，先上在下的随机移动三种
                int randonNum = UnityEngine.Random.Range(0, 3);
                if (randonNum == 0)
                {
                    _stateMachine.ChangeState(BossState.RangedAttack);
                }
                else if (randonNum == 1)
                {
                    _stateMachine.ChangeState(BossState.DashAttack);
                }
                else
                {
                    _stateMachine.ChangeState(BossState.AttackRandomMove);
                }
            }
        }
        else
        {
            // 瞬移失败，直接选择攻击状态

            //分为远程攻击，冲锋攻击，先上在下的随机移动三种
            int randonNum = UnityEngine.Random.Range(0, 3);
            if (randonNum == 0)
            {
                _stateMachine.ChangeState(BossState.RangedAttack);
            }
            else if (randonNum == 1)
            {
                _stateMachine.ChangeState(BossState.DashAttack);
            }
            else
            {
                _stateMachine.ChangeState(BossState.AttackRandomMove);
            }
        }
    }

    private bool TelePort(Vector2 targetPosition)
    {
        _stateMachine.transform.position = targetPosition;

        Rigidbody2D rb = _stateMachine.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        return SetupSegmentsPosition();
    }

    private bool SetupSegmentsPosition()
    {
        for (int i = 1; i < _stateMachine.Segments.Count; i++)
        {
            // 使用相对位置
            Vector3 segmentOffset = _isLeft ?
                new Vector3(-i * 4.5f, 0, 0) :
                new Vector3(i * 4.5f, 0, 0);

            _stateMachine.Segments[i].position = _stateMachine.transform.position + segmentOffset;
        }

        return true;
    }
    #endregion



    // 退出状态时调用（清理）
    public void ExitState()
    {
        if (_AttackIdle != null)
        {
            _stateMachine.StopCoroutine(_AttackIdle);
            _AttackIdle = null;
        }
        _stateMachine.IsMove = true;
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