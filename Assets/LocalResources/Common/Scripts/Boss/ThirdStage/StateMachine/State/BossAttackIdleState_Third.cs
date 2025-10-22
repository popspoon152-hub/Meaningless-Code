using System.Collections;
using UnityEngine;

public class BossAttackIdleState_Third : IBossStateThirdStage
{
    private BossThirdStateMachine _stateMachine;
    private Coroutine _attackIdleCoroutine;
    private Transform _chooseTrans;

    // 进入状态时调用（初始化）
    public void EnterState(BossThirdStateMachine stateMachine)
    {
        _stateMachine = stateMachine;

        // 如果有动画，触发攻击待机动画
        // _stateMachine.Animator?.SetTrigger("AttackIdle");

        _attackIdleCoroutine = _stateMachine.StartCoroutine(AttackIdle());
    }

    private void ChooseBossPos()
    {
        // 随机选择攻击待机位置
        int num = UnityEngine.Random.Range(0, 2);
        if (num == 0)
        {
            _chooseTrans = _stateMachine.IdleLeftTransfrom;  // 可替换为适当的攻击待机位置
        }
        else
        {
            _chooseTrans = _stateMachine.IdleRightTransfrom;  // 可替换为适当的攻击待机位置
        }
    }

    private IEnumerator AttackIdle()
    {
        // 选择Boss的待机位置
        ChooseBossPos();

        // Boss移动到选择的位置
        while (Vector2.Distance(_stateMachine.transform.position, _chooseTrans.position) > 0.1f)
        {
            // Move towards the chosen position
            _stateMachine.transform.position = Vector2.MoveTowards(_stateMachine.transform.position, _chooseTrans.position, _stateMachine.CurrentMoveSpeed * Time.deltaTime);
            yield return null;
        }

        // Boss达到目标位置后停留片刻
        yield return new WaitForSeconds(_stateMachine.IdleTime);

        // 选择一种攻击方式
        _stateMachine.AttackStateChoose();
    }

    // 退出状态时调用（清理）
    public void ExitState()
    {
        _stateMachine = null;
        if (_attackIdleCoroutine != null)
        {
            _stateMachine.StopCoroutine(_attackIdleCoroutine);
            _attackIdleCoroutine = null;
        }
    }

    // 每帧更新
    public void UpdateState()
    {
        // 可以添加每帧的更新逻辑（如AI行为检查等）
    }

    // 固定时间步长更新（物理相关）
    public void FixedUpdateState()
    {
        // 物理相关的处理
    }

    // 处理动画事件
    public void OnAnimationEvent(string eventName)
    {
        // 处理动画事件（例如：Boss攻击动画）
    }
}
