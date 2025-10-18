using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BossEatBeansState_First : IBossStateFirstStage
{
    [Range(0.5f, 2f)] public float RepathInterval = 0.5f;                  // 定期重算路径，防止障碍/豆子移动导致路径失效
    [Range(0.1f, 3f)] public float EatDistance = 0.8f;                     // 到达此距离视为“吃掉”豆子

    private BossFirstStateMachine _stateMachine;
    private Bean _targetBean;
    private NavMeshPath _path;
    private int _currentCornerIndex;
    private float _repathTimer;
    private float NoBeansWaitTime = 0.2f;                             // 没有豆子时的等待时间

    private readonly float _reachCornerThreshold = 0.2f;
    private float _maxVerticalSpeed;                                // 最大竖直速度

    private Coroutine _noBeansCoroutine;

    // 进入状态时调用（初始化）
    public void EnterState(BossFirstStateMachine stateMachine)
    {
        _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        _path = new NavMeshPath();
        _currentCornerIndex = 0;
        _repathTimer = 0f;
        _maxVerticalSpeed = _stateMachine.EatBeanMoveSpeed;

        SelectNextTarget();
    }

    // 每帧更新
    public void UpdateState()
    {
        //如果没有目标豆子，尝试选择下一个
        if(_targetBean == null)
        {
            SelectNextTarget();
            return;
        }

        //如果被玩家或其他因素破坏,则重新选择目标
        if(_targetBean == null || _targetBean.gameObject == null || !_targetBean.gameObject.activeInHierarchy)
        {
            _targetBean = null;
            SelectNextTarget();
            return;
        }

        //周期重算路径
        _repathTimer += Time.deltaTime;
        if (_repathTimer >= RepathInterval)
        {
            _repathTimer = 0f;
            TryRepathToTarget();
        }
    }

    // 固定时间步长更新（物理相关）
    public void FixedUpdateState()
    {
        if (_stateMachine == null) return;

        //跟随移动
        _stateMachine.SegmentsMove();

        if(_targetBean == null) return;

        //移动到目标豆子
        Vector3 bossPos = _stateMachine.Rb.position;
        Vector3 targetPos;

        if (_path != null && _path.status == NavMeshPathStatus.PathComplete && _path.corners != null && _currentCornerIndex < _path.corners.Length)
        {
            targetPos = _path.corners[_currentCornerIndex];
        }
        else
        {
            // 没有可用路径时直接走向 bean 的位置（可能被障碍阻挡）
            targetPos = _targetBean.transform.position;
        }

        float desiredY = Mathf.MoveTowards(bossPos.y, targetPos.y, _maxVerticalSpeed * Time.fixedDeltaTime);
        targetPos.y = desiredY;

        Vector3 dir = targetPos - bossPos;
        float sqrDist = dir.sqrMagnitude;
        if (sqrDist <= _reachCornerThreshold * _reachCornerThreshold)
        {
            // 到达当前 corner，推进到下一个 corner
            if (_path != null && _path.corners != null && _currentCornerIndex < _path.corners.Length - 1)
            {
                _currentCornerIndex++;
            }
        }

        //move(限制了速度)
        if (dir.sqrMagnitude > 1e-6f)
        {
            Vector3 moveDir = dir.normalized;
            float speed = _stateMachine.EatBeanMoveSpeed;
            _stateMachine.CurrentMoveSpeed = speed;
            Vector3 newPos = bossPos + moveDir * speed * Time.fixedDeltaTime;
            _stateMachine.Rb.MovePosition(newPos);

            //朝向
            Vector3 look = moveDir;
            
            if(look != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(look);

                // 平滑旋转，避免瞬间俯仰
                _stateMachine.transform.rotation = Quaternion.Slerp(_stateMachine.transform.rotation, targetRot, Mathf.Clamp01(8f * Time.fixedDeltaTime));
            }
        }

        //检查吃豆条件
        float distToBean = Vector3.Distance(bossPos, _targetBean.transform.position);
        if (distToBean <= EatDistance)
        {
            //没加动画

            UnityEngine.Object.Destroy(_targetBean.gameObject);
            _targetBean = null;
            SelectNextTarget();
        }
        
    }

    // 退出状态时调用（清理）
    public void ExitState()
    {
        if (_noBeansCoroutine != null)
        {
            _stateMachine.StopCoroutine(_noBeansCoroutine);
            _noBeansCoroutine = null;
        }

        _targetBean = null;
        _path = null;
    }

    // 处理动画事件
    public void OnAnimationEvent(string eventName)
    {

    }

    #region Found Path

    private void SelectNextTarget()
    {
        Bean[] beans = UnityEngine.Object.FindObjectsOfType<Bean>();
        if(beans == null || beans.Length == 0)
        {
            // 若尚未运行等待协程，则启动等待；已经在等待中则直接返回
            if (_noBeansCoroutine == null)
            {
                _noBeansCoroutine = _stateMachine.StartCoroutine(NoBeansWaitCoroutine());
            }
            return;
        }

        // 若在等待阶段出现了豆子，取消等待协程
        if (_noBeansCoroutine != null)
        {
            _stateMachine.StopCoroutine(_noBeansCoroutine);
            _noBeansCoroutine = null;
        }

        var ordered = beans.OrderByDescending(b => Vector3.Distance(_stateMachine.transform.position, b.transform.position)).ToArray();

        foreach (var candidate in ordered)
        {
            if (candidate == null || candidate.gameObject == null || !candidate.gameObject.activeInHierarchy) continue;

            // 尝试通过 NavMesh 计算路径
            if (TryComputePath(candidate.transform.position, out NavMeshPath candidatePath) && candidatePath.status == NavMeshPathStatus.PathComplete)
            {
                _targetBean = candidate;
                _path = candidatePath;
                _currentCornerIndex = 0;
                _repathTimer = 0f;
                return;
            }
        }

        // 如果没有任何远的可达 bean，再尝试选择任意最近的（降级处理）
        var fallback = ordered.OrderBy(b => Vector3.Distance(_stateMachine.transform.position, b.transform.position)).FirstOrDefault();
        if (fallback != null)
        {
            // 仍然尝试直线路径（虽然有障碍）
            _targetBean = fallback;
            TryComputePath(_targetBean.transform.position, out _path); // 可能是不可完整路径
            _currentCornerIndex = 0;
            _repathTimer = 0f;
        }
        else
        {
            _stateMachine.ChangeState(BossState.RandomMove);
        }
    }

    private IEnumerator NoBeansWaitCoroutine()
    {
        yield return new WaitForSeconds(NoBeansWaitTime);

        // 等待结束后再次检查场上是否有豆子
        Bean[] beansAfter = UnityEngine.Object.FindObjectsOfType<Bean>();
        _noBeansCoroutine = null;

        if (beansAfter == null || beansAfter.Length == 0)
        {
            _stateMachine.ChangeState(BossState.AttackRandomMove);
        }
        else
        {
            // 有豆子则重新选目标
            SelectNextTarget();
        }
    }

    private bool TryRepathToTarget()
    {
        if (_targetBean == null) return false;
        return TryComputePath(_targetBean.transform.position, out _path);
    }


    private bool TryComputePath(Vector3 destination, out NavMeshPath outPath)
    {
        outPath = new NavMeshPath();
        try
        {
            Vector3 source = _stateMachine.transform.position;
            // 使用 NavMesh 计算完全路径
            bool ok = NavMesh.CalculatePath(source, destination, NavMesh.AllAreas, outPath);
            return ok;
        }
        catch (Exception)
        {
            // 如果 NavMesh 不可用或发生异常，返回失败（上层会做降级移动）
            outPath = null;
            return false;
        }
    }

    #endregion
}
