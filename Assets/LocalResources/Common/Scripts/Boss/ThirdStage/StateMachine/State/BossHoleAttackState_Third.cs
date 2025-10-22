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
        // 1️⃣ 前摇 / 预警
        _stateMachine.IsCharging = true;
        // 可触发动画或特效
        // _stateMachine.Animator_Third?.SetTrigger("HolePrewarn");

        yield return new WaitForSeconds(_stateMachine.HolePreWarnTime);
        _stateMachine.IsCharging = false;

        // 2️⃣ 计算生成位置
        Vector3 spawnPos = GetHoleSpawnPosition();

        // 3️⃣ 实例化黑洞（空中背景中）
        if (_stateMachine.HolePrefab != null)
        {
            GameObject.Instantiate(_stateMachine.HolePrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("BossHoleAttackState_Third: HolePrefab is null - cannot spawn black hole.");
        }

        // 4️⃣ 短暂后摇后切换下个状态
        yield return new WaitForSeconds(_stateMachine.HoleSpawnEndDelay);
        _stateMachine.AttackStateChoose();
    }

    /// <summary>
    /// 计算黑洞生成位置
    /// 默认：在玩家附近随机偏移一段距离
    /// </summary>
    private Vector3 GetHoleSpawnPosition()
    {
        Transform player = _stateMachine.Player_Third;
        Vector3 basePos = player != null ? player.position : _stateMachine.transform.position;

        // 随机偏移（上下左右）
        float offsetX = Random.Range(-_stateMachine.HoleSpawnRangeX, _stateMachine.HoleSpawnRangeX);
        float offsetY = Random.Range(-_stateMachine.HoleSpawnRangeY, _stateMachine.HoleSpawnRangeY);

        return new Vector3(basePos.x + offsetX, basePos.y + offsetY, basePos.z);
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
