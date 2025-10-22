using System.Collections;
using UnityEngine;

public class BossDashAttackState_Third : IBossStateThirdStage
{
    private BossThirdStateMachine _stateMachine;
    private Coroutine _dashCoroutine;

    public void EnterState(BossThirdStateMachine stateMachine)
    {
        _stateMachine = stateMachine;

        //启动冲刺逻辑
        _dashCoroutine = _stateMachine.StartCoroutine(DashAttackRoutine());
    }

    private IEnumerator DashAttackRoutine()
    {
        Transform boss = _stateMachine.transform;
        Transform player = _stateMachine.Player_Third;

        //进入蓄力阶段
        _stateMachine.IsCharging = true;

        //计算并锁定冲刺方向
        Vector2 dashDirection = (player.position - boss.position).normalized;
        float chargeTime = _stateMachine.DashChargeTime;

        //可选动画触发
        // _stateMachine.Animator_Third?.SetTrigger("DashCharge");

        yield return new WaitForSeconds(chargeTime);
        _stateMachine.IsCharging = false;

        //开始冲刺阶段
        Vector2 startPosition = boss.position;
        float dashDistance = 0f;
        float trailTimer = 0f;

        //可选动画触发
        // _stateMachine.Animator_Third?.SetTrigger("DashStart");

        while (dashDistance < _stateMachine.DashMaxDistance)
        {
            float step = _stateMachine.DashSpeed * Time.deltaTime;
            boss.Translate(dashDirection * step, Space.World);
            dashDistance += step;
            trailTimer += Time.deltaTime;

            //旋转Boss朝向冲刺方向
            float angle = Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg;
            boss.rotation = Quaternion.Euler(0, 0, angle);

            // 定期生成路径标记
            if (trailTimer >= _stateMachine.DashTrailSpawnInterval)
            {
                trailTimer = 0f;
                if (_stateMachine.DashTrailPrefab != null)
                {
                    GameObject trail = Object.Instantiate(
                        _stateMachine.DashTrailPrefab,
                        boss.position,
                        Quaternion.identity);
                }
            }

            // 检测是否撞到墙体（停止冲刺）
            RaycastHit2D hit = Physics2D.Raycast(boss.position, dashDirection, 0.5f, _stateMachine.WallLayerMask);
            if (hit.collider != null)
            {
                // 撞墙
                break;
            }

            // 如果撞到玩家则造成冲刺伤害
            if (Vector2.Distance(boss.position, player.position) <= 1f)
            {
                var health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamageByEnemy(_stateMachine.DashImpactDamage);
                }
            }

            yield return null;
        }

        // 3️⃣ 冲刺结束，停顿后选择下一个状态
        yield return new WaitForSeconds(0.4f);
        _stateMachine.AttackStateChoose();
    }

    public void ExitState()
    {
        if (_dashCoroutine != null)
        {
            _stateMachine.StopCoroutine(_dashCoroutine);
            _dashCoroutine = null;
        }
        _stateMachine.IsCharging = false; // 退出时确保状态重置
        _stateMachine = null;
    }

    public void UpdateState() { }
    public void FixedUpdateState() { }
    public void OnAnimationEvent(string eventName) { }
}
