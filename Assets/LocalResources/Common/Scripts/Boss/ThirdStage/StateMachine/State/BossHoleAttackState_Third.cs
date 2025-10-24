using System.Collections;
using UnityEngine;

public class BossHoleAttackState_Third : IBossStateThirdStage
{
    private BossThirdStateMachine _stateMachine;
    private Coroutine _routine;

    public void EnterState(BossThirdStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _routine = _stateMachine.StartCoroutine(HoleAttackRoutine());
    }

    private IEnumerator HoleAttackRoutine()
    {
        // 1️前摇阶段
        _stateMachine.IsCharging = true;

        // 可选动画触发，例如:
        // _stateMachine.Animator_Third?.SetTrigger("Hole_Prepare");

        yield return new WaitForSeconds(_stateMachine.HolePreWarnTime);

        _stateMachine.IsCharging = false;

        // 2️在Boss当前位置生成黑洞
        if (_stateMachine.HolePrefab != null)
        {
            Vector3 spawnPos = _stateMachine.transform.position;
            GameObject.Instantiate(_stateMachine.HolePrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("[BossHoleAttack] HolePrefab is null - cannot spawn black hole.");
        }

        // 3️ 短暂等待后切换到下一个状态
        yield return new WaitForSeconds(_stateMachine.HoleSpawnEndDelay);

        _stateMachine.AttackStateChoose();
        //_stateMachine.ChangeState(BossState_Third.AttackIdle);  // 使Boss在完成当前待机后重新进入AttackIdle状态
        //测试用，记得删
    }

    public void ExitState()
    {
        if (_routine != null)
        {
            _stateMachine.StopCoroutine(_routine);
            _routine = null;
        }

        if (_stateMachine != null)
        {
            _stateMachine.IsCharging = false;
            _stateMachine = null;
        }
    }

    public void UpdateState() { }
    public void FixedUpdateState() { }
    public void OnAnimationEvent(string eventName) { }
}
