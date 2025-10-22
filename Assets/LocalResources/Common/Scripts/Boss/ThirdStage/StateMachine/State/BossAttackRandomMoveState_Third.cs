using System.Collections;
using UnityEngine;

public class BossAttackRandomMoveState_Third : IBossStateThirdStage
{
    private BossThirdStateMachine _stateMachine;
    private Coroutine _attackRandomMoveCoroutine;

    public void EnterState(BossThirdStateMachine stateMachine)
    {
        _stateMachine = stateMachine;

        // 可选：播放动画
        // _stateMachine.Animator_Third?.SetTrigger("AttackRandomMove");

        _attackRandomMoveCoroutine = _stateMachine.StartCoroutine(AttackRandomMove());
    }

    private IEnumerator AttackRandomMove()
    {
        float moveSpeed = _stateMachine.MoveSpeed_Attack;

        while (true)
        {
            // 获取玩家位置
            Vector2 targetPos = _stateMachine.Player_Third.position;
            Vector2 currentPos = _stateMachine.transform.position;
            Vector2 direction = (targetPos - currentPos).normalized;

            // Boss向玩家移动
            _stateMachine.transform.position = Vector2.MoveTowards(
                currentPos,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            // Boss面向玩家
            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                _stateMachine.transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            // 如果与玩家距离很近，可以选择结束移动，进入下一状态
            if (Vector2.Distance(currentPos, targetPos) <= 1.5f)
            {
                yield return new WaitForSeconds(0.5f); // 稍微停顿
                break;
            }

            yield return null;
        }

        // 移动完成后随机选择下一攻击状态
        _stateMachine.AttackStateChoose();
    }

    public void ExitState()
    {
        if (_attackRandomMoveCoroutine != null)
        {
            _stateMachine.StopCoroutine(_attackRandomMoveCoroutine);
            _attackRandomMoveCoroutine = null;
        }
        _stateMachine = null;
    }

    // 每帧更新
    public void UpdateState()
    {
        // 可以在此添加每帧的检查（例如目标是否更新）
    }

    // 固定时间步长更新（物理相关）
    public void FixedUpdateState()
    {
        // 如果需要物理更新，可以在这里实现
    }

    // 处理动画事件
    public void OnAnimationEvent(string eventName)
    {
        // 处理动画事件（例如：碰撞、攻击等）
    }
}
