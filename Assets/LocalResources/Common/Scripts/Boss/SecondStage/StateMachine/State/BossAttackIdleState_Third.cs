using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackIdleState_Third : IBossStateThirdStage
{
    private BossThirdStateMachine _stateMachine;
    private Coroutine _attackIdleCoroutine;
    private Transform _chooseTrans;

    public void EnterState(BossThirdStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _stateMachine.CurrentMoveSpeed = _stateMachine.MoveSpeed_ToIdle; // 设置待机移动速度

        _attackIdleCoroutine = _stateMachine.StartCoroutine(AttackIdleRoutine());
    }

    private void ChooseBossPos()
    {
        int num = Random.Range(0, 2);
        _chooseTrans = (num == 0) ? _stateMachine.IdleLeftTransfrom : _stateMachine.IdleRightTransfrom;
    }

    private IEnumerator AttackIdleRoutine()
    {
        // 1️选择待机目标位置
        ChooseBossPos();

        if (_chooseTrans == null)
        {
            Debug.LogWarning("[BossAttackIdle] No Idle target assigned.");
            yield break;
        }

        // 2️使用 Pathfinding 计算路径
        if (_stateMachine.pathfinding == null)
        {
            Debug.LogError("[BossAttackIdle] Pathfinding reference missing!");
            yield break;
        }

        List<Vector3> path = _stateMachine.pathfinding.FindPath(
            _stateMachine.transform.position,
            _chooseTrans.position
        );

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("[BossAttackIdle] Pathfinding returned empty path.");
            yield break;
        }

        // 3️沿路径逐点移动（保证水平/竖直）
        foreach (var node in path)
        {
            Vector3 targetPos = new Vector3(node.x, node.y, _stateMachine.transform.position.z);
            while (Vector2.Distance(_stateMachine.transform.position, targetPos) > 0.05f)
            {
                if (!_stateMachine.IsMove) yield break; // 若中途被打断
                _stateMachine.transform.position = Vector3.MoveTowards(
                    _stateMachine.transform.position,
                    targetPos,
                    _stateMachine.CurrentMoveSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        // 4️到达目标位置，等待一段时间
        yield return new WaitForSeconds(_stateMachine.IdleTime);

        // 5️选择下一种攻击
        _stateMachine.AttackStateChoose();
    }

    public void ExitState()
    {
        if (_attackIdleCoroutine != null)
        {
            _stateMachine.StopCoroutine(_attackIdleCoroutine);
            _attackIdleCoroutine = null;
        }
    }

    public void UpdateState() { }
    public void FixedUpdateState() { }
    public void OnAnimationEvent(string eventName) { }
}
