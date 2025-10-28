using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDevourGroundState_Third : IBossStateThirdStage
{
    private BossThirdStateMachine _stateMachine;
    private Coroutine _devourCoroutine;

    public void EnterState(BossThirdStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _stateMachine.IsMove = true;

        _devourCoroutine = _stateMachine.StartCoroutine(DevourGroundRoutine());

        _stateMachine.CurrentMoveSpeed = _stateMachine.MoveSpeed_Normal; // 设置待机移动速度
    }

    private IEnumerator DevourGroundRoutine()
    {
        // ================================
        // 1️选择一个未被吞噬的地块
        // ================================
        Transform targetTile = null;
        GroundTile groundTileComp = null;

        // 循环直到找到一个未被吞噬的地块
        int safetyCount = 0;
        while (safetyCount < 20)
        {
            var candidate = _stateMachine.GroundTileManager?.GetRandomGroundTile();
            if (candidate == null)
            {
                Debug.LogWarning("[BossDevour] No ground tile found.");
                yield break;
            }

            var tile = candidate.GetComponent<GroundTile>();
            if (tile != null && !tile.IsDevoured)
            {
                targetTile = candidate;
                groundTileComp = tile;
                break;
            }
            safetyCount++;
        }

        if (targetTile == null)
        {
            Debug.LogWarning("[BossDevour] Could not find valid (not devoured) ground tile.");
            yield break;
        }

        // ================================
        // 2️ 移动到平台一侧
        // ================================
        Vector3 bossStart = _stateMachine.transform.position;
        Vector3 tileCenter = targetTile.position;

        bool fromLeft = bossStart.x < tileCenter.x;
        Vector2 devourDir = fromLeft ? Vector2.right : Vector2.left;

        // 起始点距离平台中心 DevourDistance
        float approachDist = _stateMachine.DevourDistance;
        Vector3 startSidePos = tileCenter + new Vector3(fromLeft ? -approachDist : approachDist, 0f, 0f);

        if (_stateMachine.pathfinding == null)
        {
            Debug.LogError("[BossDevour] Pathfinding reference missing.");
            yield break;
        }

        // 使用 Pathfinding 查找路径
        List<Vector3> path = _stateMachine.pathfinding.FindPath(bossStart, startSidePos);
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("[BossDevour] No valid path to approach platform.");
            yield break;
        }

        // 沿路径移动
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 nodeWorld = path[i];
            Vector3 targetNodePos = new Vector3(nodeWorld.x, nodeWorld.y, _stateMachine.transform.position.z);

            while (Vector2.Distance(_stateMachine.transform.position, targetNodePos) > 0.05f)
            {
                if (!_stateMachine.IsMove) yield break;

                _stateMachine.transform.position = Vector3.MoveTowards(
                    _stateMachine.transform.position,
                    targetNodePos,
                    _stateMachine.CurrentMoveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        // ================================
        // 3️ 对齐高度并等待前摇
        // ================================
        float alignY = tileCenter.y;
        while (Mathf.Abs(_stateMachine.transform.position.y - alignY) > 0.02f)
        {
            if (!_stateMachine.IsMove) yield break;
            Vector3 p = _stateMachine.transform.position;
            p.y = Mathf.MoveTowards(p.y, alignY, _stateMachine.CurrentMoveSpeed * Time.deltaTime);
            _stateMachine.transform.position = p;
            yield return null;
        }

        // 吞噬前摇（预警阶段）
        if (_stateMachine.DevourPreWarnTime > 0f)
        {
            // 可在此触发预警动画或特效
            yield return new WaitForSeconds(_stateMachine.DevourPreWarnTime);
        }

        // ================================
        // 4️执行横向吞噬冲刺
        // ================================
        float totalDistance = _stateMachine.DevourDistance * 2f; // 从一边穿到另一边
        float moved = 0f;
        float speed = _stateMachine.DevourSpeed;
        float fixedY = _stateMachine.transform.position.y;

        while (moved < totalDistance)
        {
            if (!_stateMachine.IsMove) yield break;

            float step = speed * Time.deltaTime;
            _stateMachine.transform.position += (Vector3)(devourDir * step);
            Vector3 pos = _stateMachine.transform.position;
            pos.y = fixedY;
            _stateMachine.transform.position = pos;

            moved += step;
            yield return null;
        }

        // ================================
        // 5️吞噬地块并结束
        // ================================
        if (groundTileComp != null)
        {
            groundTileComp.Devour();
        }

        _stateMachine.AttackStateChoose();
        yield break;
    }

    public void ExitState()
    {
        if (_devourCoroutine != null)
        {
            if (_stateMachine != null)
                _stateMachine.StopCoroutine(_devourCoroutine);
            _devourCoroutine = null;
        }

        if (_stateMachine != null)
            _stateMachine.IsMove = false;

        _stateMachine = null;
    }

    public void UpdateState() { }
    public void FixedUpdateState() { }
    public void OnAnimationEvent(string eventName) { }
}
