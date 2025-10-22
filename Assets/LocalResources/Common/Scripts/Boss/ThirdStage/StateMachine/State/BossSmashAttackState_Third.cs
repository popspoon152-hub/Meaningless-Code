using System.Collections;
using UnityEngine;

public class BossSmashAttackState_Third : IBossStateThirdStage
{
    private BossThirdStateMachine _stateMachine;
    private Coroutine _smashCoroutine;

    public void EnterState(BossThirdStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _smashCoroutine = _stateMachine.StartCoroutine(SmashAttackRoutine());
    }

    private IEnumerator SmashAttackRoutine()
    {
        //  前摇 / 预警
        _stateMachine.IsCharging = true;
        // 可播放前摇动画或特效: _stateMachine.Animator_Third?.SetTrigger("SmashPrewarn");
        yield return new WaitForSeconds(_stateMachine.SmashPreWarnTime);
        _stateMachine.IsCharging = false;

        // 锁定玩家位置，并检查目标位置是否有地面
        Vector3 targetPos = _stateMachine.Player_Third != null ? _stateMachine.Player_Third.position : _stateMachine.transform.position;

        // 检查目标位置下方是否有地面
        if (!CheckGroundUnderTarget(targetPos))
        {
            Debug.LogWarning("No ground under target position, Smash attack failed.");
            yield break;  // 目标位置没有地面，攻击失败
        }

        // Boss闪现到目标位置（玩家附近）
        Vector3 spawnPos = new Vector3(targetPos.x, targetPos.y, _stateMachine.transform.position.z);
        _stateMachine.transform.position = spawnPos; // 快速闪现到目标位置

        // 可选动画触发: _stateMachine.Animator_Third?.SetTrigger("SmashStart");

        //  开始下砸：控制下落速度与效果
        yield return _stateMachine.StartCoroutine(SmashDown(targetPos));

        //  下砸后停滞一段时间
        yield return new WaitForSeconds(_stateMachine.SmashPostDelay);

        //执行范围伤害（在Boss周围）
        ApplySmashAreaDamage(targetPos);

        // 攻击结束，选择下一个状态
        _stateMachine.AttackStateChoose();
    }

    private bool CheckGroundUnderTarget(Vector3 targetPos)
    {
        // 向下发射射线检查是否有地面
        RaycastHit2D hit = Physics2D.Raycast(targetPos, Vector2.down, 5f, _stateMachine.HoleGroundLayerMask);
        return hit.collider != null;  // 有地面，返回true
    }

    private IEnumerator SmashDown(Vector3 targetPos)
    {
        // 下砸：Boss从当前位置直接下落到目标地面
        Vector3 fallStartPos = _stateMachine.transform.position;
        Vector3 fallEndPos = new Vector3(targetPos.x, targetPos.y - 0.5f, targetPos.z); // 假设下砸目标是在目标位置稍微下方

        // 计算下砸的速度
        float fallSpeed = _stateMachine.SmashFallSpeed;
        float fallDistance = Vector3.Distance(fallStartPos, fallEndPos);

        while (Vector3.Distance(_stateMachine.transform.position, fallEndPos) > 0.1f)
        {
            _stateMachine.transform.position = Vector3.MoveTowards(_stateMachine.transform.position, fallEndPos, fallSpeed * Time.deltaTime);
            yield return null;
        }

        // 确保精确到达地面位置
        _stateMachine.transform.position = fallEndPos;
    }

    private void ApplySmashAreaDamage(Vector3 targetPos)
    {
        // 扩展范围，施加范围伤害（可以选择固定范围，或者按 `SmashAreaRadius` 控制）
        Collider2D[] colliders = Physics2D.OverlapCircleAll(targetPos, _stateMachine.SmashAreaRadius, _stateMachine.HoleGroundLayerMask);
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                var health = col.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamageByEnemy(_stateMachine.SmashImpactDamage); // 直接伤害
                }
            }
        }
    }

    public void ExitState()
    {
        if (_smashCoroutine != null)
        {
            _stateMachine.StopCoroutine(_smashCoroutine);
            _smashCoroutine = null;
        }
        _stateMachine = null;
    }

    public void UpdateState() { }
    public void FixedUpdateState() { }
    public void OnAnimationEvent(string eventName) { }
}
