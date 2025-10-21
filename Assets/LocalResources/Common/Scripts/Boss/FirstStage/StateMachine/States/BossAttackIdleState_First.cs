using System;
using System.Collections;
using System.Collections.Generic;
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

        //瞬移或移动
        TelePort((int)_chooseTrans.position.x, (int)_chooseTrans.position.y);

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

    private void TelePort(int gridX, int gridY)
    {
        if (GridManager.Ins != null)
        {
            // 确保坐标在网格范围内
            gridX = Mathf.Clamp(gridX, 0, GridManager.Ins.gridSizeX - 1);
            gridY = Mathf.Clamp(gridY, 0, GridManager.Ins.gridSizeY - 1);

            // 获取该坐标的节点
            Node targetNode = GridManager.Ins.grid[gridX, gridY];

            // 如果节点可行走，则瞬移
            if (targetNode.walkable)
            {
                _stateMachine.transform.position = targetNode.worldPosition;
            }
            else
            {
                Debug.LogWarning($"目标网格 ({gridX}, {gridY}) 不可行走！");
            }


            //设置后面的尾巴与目标地点
            if (_isLeft)
            {
                for (int i = 1; i < _stateMachine.Segments.Count; i++)
                {
                    _stateMachine.Segments[i].position = GridManager.Ins.grid[gridX - 1, gridY - i + 1].worldPosition;
                }
            }
            else
            {
                for (int i = 1; i < _stateMachine.Segments.Count; i++)
                {
                    _stateMachine.Segments[i].position = GridManager.Ins.grid[gridX + 1, gridY - i + 1].worldPosition;
                }
            }
        }
    }
    #endregion



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
