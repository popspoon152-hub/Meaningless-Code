using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class BossAttackRandomMoveState_First : IBossStateFirstStage
{
    private BossFirstStateMachine _stateMachine;

    private Coroutine _attackRandomMove;
    private int targetIndex;

    private bool _isAtLeft;

    // 进入状态时调用（初始化）
    public void EnterState(BossFirstStateMachine stateMachine)
    {
        this._stateMachine = stateMachine;
        //if (_stateMachine.Animator != null)
        //{
        //    _stateMachine.Animator.SetTrigger("AttackRandomMove");
        //}

        _stateMachine.IsMove = true;
        _stateMachine.CurrentMoveSpeed = _stateMachine.AttackRandomMoveSpeed;
        _attackRandomMove = _stateMachine.StartCoroutine(AttackRandomMove());
    }

    #region AttackRandomMove
    private IEnumerator AttackRandomMove()
    {
        _isAtLeft = _stateMachine.transform.position.x <= _stateMachine.AttackRandomMoveJumpPostion.position.x;

        List<Vector3> path = Pathfinding.Ins.FindPath(_stateMachine.transform.position, _stateMachine.AttackRandomMoveJumpPostion.position);
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

        targetIndex = 0;
        if (_isAtLeft)
        {

        }
        path = _isAtLeft ? Pathfinding.Ins.FindPath(_stateMachine.transform.position, _stateMachine.AttackRandomMoveEndPostionRightPoint.position)
                         : Pathfinding.Ins.FindPath(_stateMachine.transform.position, _stateMachine.AttackRandomMoveEndPostionLeftPoint.position);

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

        if (UnityEngine.Object.FindObjectsOfType<Bean>() != null)
        {
            _stateMachine.ChangeState(BossState.EatBeans);
        }
        else
        {
            _stateMachine.AttackStateChoose();
        }
    }

    #endregion

    // 每帧更新
    public void UpdateState()
    {
        if (_attackRandomMove != null)
        {
            if (_isAtLeft)
            {
                CheckStateChange(_stateMachine.AttackRandomMoveEndPostionRightPoint.position);
            }
            else
            {
                CheckStateChange(_stateMachine.AttackRandomMoveEndPostionLeftPoint.position);
            }
        }
    }

    private void CheckStateChange(Vector2 position)
    {
        float distToTarget = Vector2.Distance(_stateMachine.transform.position, new Vector2(position.x, position.y));
        if (distToTarget <= _stateMachine.DashTargetCheckLength)
        {
            if (UnityEngine.Object.FindObjectsOfType<Bean>() != null)
            {
                _stateMachine.ChangeState(BossState.EatBeans);
            }
            else
            {
                _stateMachine.AttackStateChoose();
            }

        }
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
        _stateMachine = null;
        if (_attackRandomMove != null)
        {
            _stateMachine.StopCoroutine(AttackRandomMove());
            _attackRandomMove = null;
        }
    }














    // 处理动画事件
    public void OnAnimationEvent(string eventName)
    {

    }
}
