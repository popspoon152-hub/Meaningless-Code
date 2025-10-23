using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackRandomMoveState_Third : IBossStateThirdStage
{
    private BossThirdStateMachine _stateMachine;
    private Coroutine _moveRoutine;
    private bool _hasHitPlayer;

    // 新增字段：锁定目标位置
    private Vector3 _lockedTargetPosition;

    public void EnterState(BossThirdStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _hasHitPlayer = false;

        // 确保Pathfinding存在
        if (_stateMachine.pathfinding == null)
        {
            Debug.LogError("Pathfinding reference missing in BossThirdStateMachine!");
            return;
        }

        _stateMachine.IsMove = true;
        _stateMachine.CurrentMoveSpeed = _stateMachine.MoveSpeed_Attack;

        // 状态开始时仅记录一次玩家位置
        if (_stateMachine.Player_Third != null)
            _lockedTargetPosition = _stateMachine.Player_Third.position;
        else
            _lockedTargetPosition = _stateMachine.transform.position; // fallback

        // 启动一次性寻路协程
        _moveRoutine = _stateMachine.StartCoroutine(MovePathfindingRoutine());

    }

    private IEnumerator MovePathfindingRoutine()
    {
        //  仅在状态开始时寻路一次
        List<Vector3> path = _stateMachine.pathfinding.FindPath(
            _stateMachine.transform.position,
            _lockedTargetPosition
        );

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("[Boss] No valid path found!");
            yield break;
        }

        // Debug
        // Debug.Log($"[Boss] Locked target: {_lockedTargetPosition}, Path nodes: {path.Count}");

        // 按路径逐点移动
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 nextPos = path[i];

            while (Vector2.Distance(_stateMachine.transform.position, nextPos) > 0.05f)
            {
                if (!_stateMachine.IsMove || _hasHitPlayer)// 撞到玩家中断或被攻击中断
                {
                    // 结束移动状态
                    _stateMachine.AttackStateChoose();
                    //_stateMachine.ChangeState(BossState_Third.AttackIdle);  // 使Boss在完成当前待机后重新进入AttackIdle状态（测试用）
                    yield break;  
                }
               
                //Debug.Log(_hasHitPlayer);

                _stateMachine.transform.position = Vector2.MoveTowards(
                    _stateMachine.transform.position,
                    nextPos,
                    _stateMachine.CurrentMoveSpeed * Time.deltaTime
                );

                yield return null;
            }

            // 到达目标节点
            if (i == path.Count - 1)
            {
                // 结束移动状态
                _stateMachine.AttackStateChoose();
                //_stateMachine.ChangeState(BossState_Third.AttackIdle);  // 使Boss在完成当前待机后重新进入AttackIdle状态（测试用）
                yield break;
            }
        }
    }

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

    public void OnBossHitPlayer() => _hasHitPlayer = true;

    public void UpdateState() { }
    public void FixedUpdateState() { }
    public void OnAnimationEvent(string eventName) { }
}
