using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class BossDashAttackState_First : IBossStateFirstStage
{
    private BossFirstStateMachine _stateMachine;

    private Transform _chooseTrans;
    private bool _isLeft;
    private Transform _targetTrans;
    private int targetIndex;

    private Coroutine _dashAttack;

    // 进入状态时调用（初始化）
    public void EnterState(BossFirstStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        //if (_stateMachine.Animator != null)
        //{
        //    _stateMachine.Animator.SetTrigger("DashAttack");
        //}

        DashAttack();
    }



    #region Dash
    private void DashAttack()
    {
        _stateMachine.IsMove = false;
        _stateMachine.CurrentMoveSpeed = _stateMachine.DashSpeed;


        _chooseTrans = SelectPoint();
        TelePort((int)_chooseTrans.position.x, (int)_chooseTrans.position.y);

        _stateMachine.IsMove = true;
        _dashAttack = _stateMachine.StartCoroutine(Dash());
    }

    private Transform SelectPoint()
    {
        int num = UnityEngine.Random.Range(0, 2);
        if(num == 0)
        {
            _isLeft = true;
            return _stateMachine.TelePortLeftPoint;
        }
        else
        {
            _isLeft = false;
            return _stateMachine.TelePortRightPoint;
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
                    Node segmentsTargetNode = GridManager.Ins.grid[gridX - i, gridY];
                    _stateMachine.Segments[i].position = segmentsTargetNode.worldPosition;
                }

                _targetTrans.position = GridManager.Ins.grid[gridX + _stateMachine.DashLength, gridY].worldPosition;
                _stateMachine._isAtLeft = false;
            }
            else
            {
                for (int i = 1; i < _stateMachine.Segments.Count; i++)
                {
                    Node segmentsTargetNode = GridManager.Ins.grid[gridX + i, gridY];
                    _stateMachine.Segments[i].position = segmentsTargetNode.worldPosition;
                }

                _targetTrans.position = GridManager.Ins.grid[gridX - _stateMachine.DashLength, gridY].worldPosition;
                _stateMachine._isAtLeft = true;
            }
        }
    }

    private IEnumerator Dash()
    {
        List<Vector3> path = _stateMachine.pathfinding.FindPath(_stateMachine.transform.position, _targetTrans.position);
        if (path != null && path.Count > 0)
        {
            Vector3 currentWaypoint = path[0];

            while (true)
            {
                if (_stateMachine.transform.position == currentWaypoint)
                {
                    targetIndex++;
                    if (targetIndex >= path.Count)
                    {
                        yield break;
                    }
                    currentWaypoint = path[targetIndex];
                }

                _stateMachine.transform.position = Vector3.MoveTowards(_stateMachine.transform.position, currentWaypoint, _stateMachine.CurrentMoveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }

    #endregion

    // 退出状态时调用（清理）
    public void ExitState()
    {
        _stateMachine = null;
        if (_dashAttack != null)
        {
            _stateMachine.StopCoroutine(Dash());
            _dashAttack = null;
        }
    }


    // 固定时间步长更新（物理相关）
    public void FixedUpdateState()
    {
        if (_stateMachine == null) return;

        //跟随移动
        if (_stateMachine.IsMove) _stateMachine.SegmentsMove();
    }

    // 每帧更新
    public void UpdateState()
    {
        if (_targetTrans != null)
        {
            float distToTarget = Vector2.Distance(_stateMachine.transform.position, new Vector2(_targetTrans.position.x, _targetTrans.position.y));
            if (distToTarget <= _stateMachine.DashTargetCheckLength)
            {
                if(UnityEngine.Object.FindObjectsOfType<Bean>() != null)
                {
                    _stateMachine.ChangeState(BossState.EatBeans);
                }
                else
                {
                    _stateMachine.AttackStateChoose();
                }

            }
        }
    }










    // 处理动画事件
    public void OnAnimationEvent(string eventName)
    {

    }
}
