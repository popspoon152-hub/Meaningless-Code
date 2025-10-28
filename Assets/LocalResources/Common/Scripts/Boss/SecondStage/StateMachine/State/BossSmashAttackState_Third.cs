using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSmashAttackState_Third : IBossStateThirdStage
{
    private BossThirdStateMachine _stateMachine;
    private Coroutine _smashRoutine;

    public void EnterState(BossThirdStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _smashRoutine = _stateMachine.StartCoroutine(SmashRoutine());
    }

    private IEnumerator SmashRoutine()
    {
        // 1) 基础检查
        if (_stateMachine.Player_Third == null)
        {
            Debug.LogWarning("[BossSmash] Player reference missing.");
            yield break;
        }
        if (_stateMachine.pathfinding == null)
        {
            Debug.LogWarning("[BossSmash] Pathfinding missing.");
            yield break;
        }

        Vector3 bossPos = _stateMachine.transform.position;
        Vector3 playerPos = _stateMachine.Player_Third.position;

        // ---- 修正：目标对齐 X（让玩家在Boss正上方/正下方） ----
        Vector3 targetAlignPos = new Vector3(playerPos.x, bossPos.y, bossPos.z);

        // 2) 用寻路移动到对齐位置（保证水平/竖直移动）
        List<Vector3> path = _stateMachine.pathfinding.FindPath(bossPos, targetAlignPos);
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("[BossSmash] No path to align vertically with player (target X).");
            _stateMachine.AttackStateChoose();
            yield break;
        }

        foreach (var node in path)
        {
            Vector3 nodePos = new Vector3(node.x, node.y, _stateMachine.transform.position.z);
            while (Vector2.Distance(_stateMachine.transform.position, nodePos) > 0.05f)
            {
                // 如果被打断或不允许移动则退出
                if (!_stateMachine.IsMove)
                {
                    _stateMachine.AttackStateChoose();
                    yield break;
                }
                    

                _stateMachine.transform.position = Vector3.MoveTowards(
                    _stateMachine.transform.position,
                    nodePos,
                    _stateMachine.MoveSpeed_Attack * Time.deltaTime);
                yield return null;
            }
        }

        // 3) 前摇：蓄力并向上移动一小段（SmashChargeRiseHeight）
        _stateMachine.IsCharging = true;
        Vector3 riseTarget = _stateMachine.transform.position + Vector3.up * _stateMachine.SmashChargeRiseHeight;
        float timer = 0f;
        float riseSpeed = _stateMachine.SmashRiseSpeed; // 也可以用单独参数
        while (timer < _stateMachine.SmashPreWarnTime)
        {
            if (!_stateMachine.IsMove)
            {
                _stateMachine.IsCharging = false;
                _stateMachine.AttackStateChoose();
                yield break;
            }

            _stateMachine.transform.position = Vector3.MoveTowards(
                _stateMachine.transform.position,
                riseTarget,
                riseSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }
        _stateMachine.IsCharging = false;

        // 4) 找到下方的地面目标（fallTarget）
        Vector3 fallStart = _stateMachine.transform.position;
        Vector3 fallTarget = FindGroundBelow(fallStart);
        if (fallTarget == Vector3.zero)
        {
            Debug.LogWarning("[BossSmash] No ground found below, canceling smash.");
            _stateMachine.AttackStateChoose();
            yield break;
        }

        // 5) 在真正下落前：使用 GridManager 检查“竖直路径”是否有障碍节点
        bool blocked = false;
        if (GridManager.Ins != null)
        {
            float xCheck = _stateMachine.transform.position.x; // 对齐后的 X
            float topY = _stateMachine.transform.position.y;
            float bottomY = fallTarget.y;
            if (bottomY > topY) // 保险：如果地面在上方（奇怪情况），交换
            {
                float t = topY; topY = bottomY; bottomY = t;
            }

            float step = GridManager.Ins.nodeRadius * 2f; // 网格步长
            // 从上（rise）到地面之间按网格步进检查
            for (float y = topY; y >= bottomY; y -= step)
            {
                Node node = GridManager.Ins.NodeFromWorldPoint(new Vector3(xCheck, y, 0f));
                if (node == null) continue;
                if (!node.walkable)
                {
                    blocked = true;
                    break;
                }
            }
        }
        else
        {
            // 没有 GridManager 实例则使用 Raycast 粗略判断（可选）
            RaycastHit2D hit = Physics2D.Raycast(_stateMachine.transform.position, Vector2.down, Mathf.Abs(_stateMachine.transform.position.y - fallTarget.y), _stateMachine.WallLayerMask);
            if (hit.collider != null)
            {
                // 如果在下落路径发现墙体，认为被阻挡（你可以调整此逻辑）
                blocked = true;
            }
        }

        if (blocked)
        {
            Debug.Log("[BossSmash] Vertical path blocked — cancel smash.");
            // 可选择退回待机或直接 AttackStateChoose
            _stateMachine.AttackStateChoose();
            yield break;
        }

        // 6) 执行下砸（一直向 fallTarget 下落）
        float fallSpeed = _stateMachine.SmashFallSpeed;
        while (Vector2.Distance(_stateMachine.transform.position, fallTarget) > 0.05f)
        {
            if (!_stateMachine.IsMove)
            {
                _stateMachine.AttackStateChoose();
                yield break;
            }
                

            _stateMachine.transform.position = Vector3.MoveTowards(
                _stateMachine.transform.position,
                fallTarget,
                fallSpeed * Time.deltaTime);
            yield return null;
        }
        _stateMachine.transform.position = fallTarget;

        // 7) 落地后生成左右冲击波（使用 GameObject.Instantiate，因为本类不是 MonoBehaviour）
        if (_stateMachine.SmashShockwavePrefab != null)
        {
            GameObject leftWave = GameObject.Instantiate(_stateMachine.SmashShockwavePrefab, fallTarget, Quaternion.identity);
            leftWave.GetComponent<Shockwave>()?.Initialize(Vector2.left);

            GameObject rightWave = GameObject.Instantiate(_stateMachine.SmashShockwavePrefab, fallTarget, Quaternion.identity);
            rightWave.GetComponent<Shockwave>()?.Initialize(Vector2.right);
        }

        // 8) 后摇
        yield return new WaitForSeconds(_stateMachine.SmashPostDelay);

        _stateMachine.AttackStateChoose();
        //_stateMachine.ChangeState(BossState_Third.AttackIdle);  // 使Boss在完成当前待机后重新进入AttackIdle状态
        //测试用，记得删
    }

    private Vector3 FindGroundBelow(Vector3 startPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(startPos, Vector2.down, 50f, _stateMachine.HoleGroundLayerMask);
        if (hit.collider != null)
            return hit.point + Vector2.up * 0.5f; // 偏移防止穿地
        return Vector3.zero;
    }

    public void ExitState()
    {
        if (_smashRoutine != null)
        {
            if (_stateMachine != null) _stateMachine.StopCoroutine(_smashRoutine);
            _smashRoutine = null;
        }
        _stateMachine = null;
    }

    public void UpdateState() { }
    public void FixedUpdateState() { }
    public void OnAnimationEvent(string eventName) { }
}
