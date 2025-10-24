using System.Collections;
using UnityEngine;

public class BossDashAttackState_Third : IBossStateThirdStage
{
    private BossThirdStateMachine _stateMachine;
    private Coroutine _dashCoroutine;

    public void EnterState(BossThirdStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _dashCoroutine = _stateMachine.StartCoroutine(DashAttackRoutine());
    }

    private IEnumerator DashAttackRoutine()
    {
        Transform boss = _stateMachine.transform;
        Transform player = _stateMachine.Player_Third;

        if (player == null)
        {
            Debug.LogWarning("[BossDash] Player reference missing!");
            yield break;
        }

        // =======================
        //  高度锁定阶段
        // =======================
        float targetY = player.position.y;
        float verticalSpeed = _stateMachine.DashAlignSpeed; // 在状态机中设定（例如 5f）
        float threshold = 0.05f;

        Debug.Log("[BossDash] Aligning vertically with player...");

        while (Mathf.Abs(boss.position.y - targetY) > threshold)
        {
            Vector3 movePos = boss.position;
            float step = verticalSpeed * Time.deltaTime;
            movePos.y = Mathf.MoveTowards(boss.position.y, targetY, step);
            boss.position = movePos;

            yield return null;
        }

        // 可选：视线检测（检查是否有障碍物）
        bool clearLine = true;
        RaycastHit2D wallHit = Physics2D.Raycast(boss.position,
                                                 (player.position - boss.position).normalized,
                                                 Mathf.Abs(player.position.x - boss.position.x),
                                                 _stateMachine.WallLayerMask);
        if (wallHit.collider != null)
        {
            Debug.Log("[BossDash] Line of sight blocked, skipping dash.");
            clearLine = false;
        }

        // 若有阻挡则放弃冲刺（可选逻辑）
        if (!clearLine)
        {
            yield return new WaitForSeconds(0.5f);
            _stateMachine.AttackStateChoose();
            yield break;
        }

        // =======================
        //  蓄力阶段
        // =======================
        _stateMachine.IsCharging = true;

        int directionSign = (player.position.x < boss.position.x) ? -1 : 1;
        Vector2 dashDirection = new Vector2(directionSign, 0f);

        // 固定朝向
        boss.localScale = new Vector3(Mathf.Abs(boss.localScale.x) * directionSign,
                                      boss.localScale.y, boss.localScale.z);

        float chargeTime = _stateMachine.DashChargeTime;
        Debug.Log("[BossDash] Charging before dash...");
        yield return new WaitForSeconds(chargeTime);
        _stateMachine.IsCharging = false;

        // 固定Y坐标（保持水平冲刺）
        float fixedY = boss.position.y;

        // =======================
        // 冲刺阶段
        // =======================
        float dashDistance = 0f;
        float trailTimer = 0f;

        Debug.Log("[BossDash] Start horizontal dash!");

        while (dashDistance < _stateMachine.DashMaxDistance)
        {
            float step = _stateMachine.DashSpeed * Time.deltaTime;

            Vector3 newPosition = boss.position + new Vector3(dashDirection.x * step, 0f, 0f);
            newPosition.y = fixedY;
            boss.position = newPosition;
            dashDistance += step;
            trailTimer += Time.deltaTime;

            // 生成路径标记
            if (trailTimer >= _stateMachine.DashTrailSpawnInterval)
            {
                trailTimer = 0f;
                if (_stateMachine.DashTrailPrefab != null)
                {
                    Object.Instantiate(_stateMachine.DashTrailPrefab,
                        boss.position, Quaternion.identity);
                }
            }

            // 撞墙检测
            RaycastHit2D hit = Physics2D.Raycast(boss.position, dashDirection, 0.6f, _stateMachine.WallLayerMask);
            if (hit.collider != null)
            {
                Debug.Log("[BossDash] Hit wall, dash stop.");
                break;
            }

            // 撞玩家检测
            var health = player.GetComponent<PlayerHealth>();


            yield return null;
        }

        // =======================
        //  结束阶段
        // =======================
        yield return new WaitForSeconds(0.4f);
        _stateMachine.AttackStateChoose();
        //_stateMachine.ChangeState(BossState_Third.AttackIdle);  // 使Boss在完成当前待机后重新进入AttackIdle状态
        //测试用，记得删
    }

    public void ExitState()
    {
        if (_dashCoroutine != null)
        {
            _stateMachine.StopCoroutine(_dashCoroutine);
            _dashCoroutine = null;
        }

        _stateMachine.IsCharging = false;
        _stateMachine = null;
    }

    public void UpdateState() { }
    public void FixedUpdateState() { }
    public void OnAnimationEvent(string eventName) { }
}
