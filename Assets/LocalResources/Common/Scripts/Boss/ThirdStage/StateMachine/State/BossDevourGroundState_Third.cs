using System.Collections;
using UnityEngine;

public class BossDevourGroundState_Third : IBossStateThirdStage
{
    private BossThirdStateMachine _stateMachine;
    private Coroutine _devourCoroutine;

    // 进入状态时调用（初始化）
    public void EnterState(BossThirdStateMachine stateMachine)
    {
        _stateMachine = stateMachine;

        // 启动吞噬地面逻辑
        _devourCoroutine = _stateMachine.StartCoroutine(DevourGround());
    }

    private IEnumerator DevourGround()
    {
        //吞噬前的前摇（包括预警）
        Transform targetTile = _stateMachine.GroundTileManager.GetRandomGroundTile();
        if (targetTile == null) yield break;

        // 前摇：Boss移动到目标地面的位置，开始预警
        Vector3 startPos = _stateMachine.transform.position;
        Vector3 targetPos = targetTile.position;
        float preDevourTime = _stateMachine.DevourPreWarnTime;

        while (Vector2.Distance(_stateMachine.transform.position, targetPos) > 0.1f)
        {
            // 朝目标位置移动（可以使用直接的水平或垂直方向，保持平台相同高度）
            Vector3 moveDirection = (targetPos - startPos).normalized;
            _stateMachine.transform.position = Vector3.MoveTowards(_stateMachine.transform.position, targetPos, _stateMachine.MoveSpeed_Normal * Time.deltaTime);
            yield return null;
        }

        // 吞噬前摇完成，Boss开始吞噬地面
        yield return new WaitForSeconds(preDevourTime); // 预警时间

        //吞噬动作：Boss快速横向穿过平台并吞噬
        Vector3 devourEndPos = targetPos + new Vector3(targetTile.localScale.x + 1f, 0, 0); // 假设吞噬范围是目标地块的宽度 + 1单位
        Vector3 devourDirection = (devourEndPos - targetPos).normalized;

        float devourSpeed = _stateMachine.DevourSpeed;  // 吞噬速度与攻击速度一致
        float devourDistance = 0f;

        // 吞噬过程中Boss移动
        while (devourDistance < targetTile.localScale.x + 1f)  // 按平台宽度来算完全吞噬
        {
            _stateMachine.transform.position += devourDirection * devourSpeed * Time.deltaTime;
            devourDistance += devourSpeed * Time.deltaTime;
            yield return null;
        }

        //完成吞噬：标记地面为已吞噬并重新生成
        GroundTile groundTile = targetTile.GetComponent<GroundTile>();
        if (groundTile != null)
        {
            groundTile.Devour(); // 吞噬该地面
        }

        // 吞噬完成，切换到下一个攻击状态
        _stateMachine.AttackStateChoose();
    }

    public void ExitState()
    {
        if (_devourCoroutine != null)
        {
            _stateMachine.StopCoroutine(_devourCoroutine);
            _devourCoroutine = null;
        }
        _stateMachine = null;
    }

    public void UpdateState() { }
    public void FixedUpdateState() { }
    public void OnAnimationEvent(string eventName) { }
}
