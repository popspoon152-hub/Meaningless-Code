using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackRandomMoveState_Third : IBossStateThirdStage
{
    private BossThirdStateMachine _stateMachine;
    private Coroutine _moveRoutine;
    private bool _hasHitPlayer;

    public void EnterState(BossThirdStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _hasHitPlayer = false;

        // 确保Boss允许移动
        _stateMachine.IsMove = true;

        _moveRoutine = _stateMachine.StartCoroutine(MovePathfindingRoutine());
    }

    private IEnumerator MovePathfindingRoutine()
    {
        Transform player = _stateMachine.Player_Third;
        float moveSpeed = _stateMachine.MoveSpeed_Attack;

        while (_stateMachine.CurrentState != BossState_Third.Hurt) // 受伤后立即退出
        {
            if (!_stateMachine.IsMove) yield break;
            if (_hasHitPlayer) yield break;

            if (player == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // 获取寻路路径
            List<Vector3> path = _stateMachine.pathfinding.FindPath(
                _stateMachine.transform.position,
                player.position
            );

            if (path == null || path.Count == 0)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // 逐点移动
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 nextPos = path[i];

                while (Vector2.Distance(_stateMachine.transform.position, nextPos) > 0.05f)
                {
                    if (_stateMachine.CurrentState == BossState_Third.Hurt)
                        yield break;

                    if (_hasHitPlayer)
                        yield break;

                    if (!_stateMachine.IsMove)
                        yield break;

                    _stateMachine.transform.position = Vector2.MoveTowards(
                        _stateMachine.transform.position,
                        nextPos,
                        moveSpeed * Time.deltaTime
                    );

                    yield return null;
                }

                // 抵达终点后检查是否到达最后节点
                if (i == path.Count - 1)
                {
                    _stateMachine.AttackStateChoose();
                    yield break;
                }
            }

            yield return null;
        }
    }

    // Boss被玩家攻击时状态机会自动切换到 Hurt 状态，我们无需手动检测，但可以响应事件或检查 CurrentState

    public void ExitState()
    {
        if (_moveRoutine != null)
        {
            _stateMachine.StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }

        _stateMachine.IsMove = false;
        _stateMachine = null;
    }

    // 当Boss碰到玩家时，BossThirdStateMachine会调用TryDealCollisionDamage
    // 我们可以通过一个事件方式或简单调用通知状态中断
    public void OnBossHitPlayer()
    {
        _hasHitPlayer = true;
    }

    public void UpdateState() { }
    public void FixedUpdateState() { }
    public void OnAnimationEvent(string eventName) { }
}
