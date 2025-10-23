using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 83行写瞬移
/// </summary>

public class BossEatBeansState_First : IBossStateFirstStage
{
    private BossFirstStateMachine _stateMachine;

    private Bean _targetBean;
    private float NoBeansWaitTime = 0.2f;                             // 没有豆子时的等待时间
    private List<Vector3> _path;
    private int targetIndex;

    private Coroutine _noBeansCoroutine;
    private Coroutine _followPath;

    // 进入状态时调用（初始化）
    public void EnterState(BossFirstStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _stateMachine.CurrentMoveSpeed = _stateMachine.EatBeanMoveSpeed;
        _stateMachine.IsMove = true;

        targetIndex = 0;
        _path = null;

        if (GridManager.Ins != null)
        {
            GridManager.Ins.CreateGrid();
        }

        EatBeansMove();
    }

    private void EatBeansMove()
    {
        SelectNextTarget();
        MoveToTarget();
    }

    #region FindPath & MoveToTarget

    /// <summary>
    /// 找到_targetBean
    /// </summary>
    private void SelectNextTarget()
    {
        if (_noBeansCoroutine != null)
        {
            _stateMachine.StopCoroutine(_noBeansCoroutine);
            _noBeansCoroutine = null;
        }

        Bean[] beans = UnityEngine.Object.FindObjectsOfType<Bean>();

        if (beans == null || beans.Length == 0)
        {
            // 若尚未运行等待协程，则启动等待；已经在等待中则直接返回
            if (_noBeansCoroutine == null)
            {
                _noBeansCoroutine = _stateMachine.StartCoroutine(NoBeansWaitCoroutine());
            }
            return;
        }

        Bean closestBean = null;
        float closestDistance = float.MaxValue;
        Vector3 currentPos = _stateMachine.transform.position;

        int validBeanCount = 0;
        foreach (Bean bean in beans)
        {
            validBeanCount++;
            float distance = Vector3.Distance(currentPos, bean.transform.position);
            if (distance < closestDistance)
            {
                closestBean = bean;
                closestDistance = distance;
            }
        }

        _targetBean = closestBean;

        // 如果还是没找到有效豆子
        if (_targetBean == null && _noBeansCoroutine == null)
        {
            _noBeansCoroutine = _stateMachine.StartCoroutine(NoBeansWaitCoroutine());
        }

        //if (_targetBean != null)
        //{
        //    Debug.Log($"选择了豆子，位置: {_targetBean.transform.position}, 距离: {Vector3.Distance(_stateMachine.transform.position, _targetBean.transform.position)}");
        //}
        //else
        //{
        //    Debug.Log("没有找到有效的豆子");
        //}
    }

    /// <summary>
    /// 找不到豆子时的等待协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator NoBeansWaitCoroutine()
    {
        yield return new WaitForSeconds(NoBeansWaitTime);

        // 等待结束后再次检查场上是否有豆子
        Bean[] beansAfter = UnityEngine.Object.FindObjectsOfType<Bean>();
        _noBeansCoroutine = null;

        if (beansAfter == null || beansAfter.Length == 0)
        {
            _stateMachine.ChangeState(BossState.AttackIdle);
        }
        else
        {
            // 有豆子则重新选目标
            SelectNextTarget();
            if (_targetBean != null)
            {
                MoveToTarget();
            }
        }
    }

    /// <summary>
    /// 移动到目标豆子
    /// </summary>
    /// <exception></exception>
    private void MoveToTarget()
    {
        if (_targetBean == null || _targetBean.gameObject == null || !_targetBean.gameObject.activeInHierarchy)
        {
            SelectNextTarget();
            return;
        }

        if (_followPath != null)
        {
            _stateMachine.StopCoroutine(_followPath);
            _followPath = null;
        }

        if (Pathfinding.Ins == null)
        {
            //Debug.LogError("Pathfinding instance is null!");
            // 路径查找不可用，直接向豆子移动
            _followPath = _stateMachine.StartCoroutine(MoveDirectlyToBean());
            return;
        }

        _path = Pathfinding.Ins.FindPath(_stateMachine.transform.position, _targetBean.transform.position);

        if (_path != null && _path.Count > 0)
        {
            targetIndex = 0;
            _followPath = _stateMachine.StartCoroutine(FollowPath());
        }
        else
        {
            //Debug.LogWarning("路径查找失败，直接向豆子移动");
            _followPath = _stateMachine.StartCoroutine(MoveDirectlyToBean());
        }
    }

    private IEnumerator FollowPath()
    {
        if (_path == null || _path.Count == 0)
        {
            //Debug.LogWarning("路径为空，无法跟随");
            yield break;
        }

        Vector3 currentWaypoint = _path[0];
        targetIndex = 0;

        while (true)
        {

            // 路径点到达检查
            if (Vector3.Distance(_stateMachine.transform.position, currentWaypoint) <= 0.1f)
            {
                targetIndex++;
                if (targetIndex >= _path.Count)
                {
                    // 路径走完后，直接向豆子移动，直到吃到豆子
                    yield return _stateMachine.StartCoroutine(MoveDirectlyToBean());
                    yield break;
                }
                currentWaypoint = _path[targetIndex];
            }

            // 移动
            _stateMachine.transform.position = Vector3.MoveTowards(_stateMachine.transform.position, currentWaypoint, _stateMachine.CurrentMoveSpeed * Time.deltaTime);

            yield return null;
        }
    }

    private IEnumerator MoveDirectlyToBean()
    {
        if (_targetBean == null) yield break;

        while (_targetBean != null && _targetBean.gameObject != null && _targetBean.gameObject.activeInHierarchy)
        {
            float distToBean = GetDistanceToBean();

            if (distToBean <= _stateMachine.EatDistance)
            {
                EatBean();
                break;
            }

            // 直接向豆子位置移动
            _stateMachine.transform.position = Vector3.MoveTowards(
                _stateMachine.transform.position,
                _targetBean.transform.position,
                _stateMachine.CurrentMoveSpeed * Time.deltaTime
            );

            yield return null;
        }
    }

    // 统一的距离计算方法
    private float GetDistanceToBean()
    {
        if (_targetBean == null) return float.MaxValue;

        return Vector2.Distance(
            new Vector2(_stateMachine.transform.position.x, _stateMachine.transform.position.y),
            new Vector2(_targetBean.transform.position.x, _targetBean.transform.position.y)
        );
    }
    #endregion

    public void UpdateState()
    {
        if (_stateMachine == null) return;

        if (_targetBean == null && _noBeansCoroutine == null)
        {
            EatBeansMove();
            return;
        }


        if (_targetBean != null)
        {
            if (_targetBean.gameObject == null || !_targetBean.gameObject.activeInHierarchy)
            {
                _targetBean = null;

                // 停止移动
                if (_followPath != null)
                {
                    _stateMachine.StopCoroutine(_followPath);
                    _followPath = null;
                }

                EatBeansMove();
                return;
            }

            // 吃豆检查 - 只在没有路径跟随时检查（路径跟随会在内部处理吃豆）
            //if (_followPath == null)
            //{
            //    float distToBean = GetDistanceToBean();

            //    if (distToBean <= _stateMachine.EatDistance)
            //    {
            //        EatBean();
            //    }
            //    else
            //    {
            //        MoveToTarget();
            //    }
            //}
        }
    }

    // 统一的吃豆方法
    private void EatBean()
    {
        if (_followPath != null)
        {
            _stateMachine.StopCoroutine(_followPath);
            _followPath = null;
        }

        if (_targetBean != null && _targetBean.gameObject != null)
        {
            try
            {
                Bean bean = _targetBean.GetComponent<Bean>();
                if (bean != null)
                {
                    bean.BeEat();
                }
                else
                {
                    UnityEngine.Object.Destroy(_targetBean.gameObject);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"豆子销毁失败: {e.Message}");
                UnityEngine.Object.Destroy(_targetBean.gameObject);
            }
        }

        _targetBean = null;

        // 切换状态
        _stateMachine.ChangeState(BossState.Grow);
    }

    // 固定时间步长更新（物理相关）
    public void FixedUpdateState()
    {
        if (_stateMachine == null) return;

        if (_stateMachine.IsMove) _stateMachine.SegmentsMove();
    }

    // 退出状态时调用（清理）
    public void ExitState()
    {
        // 清理所有协程
        if (_noBeansCoroutine != null)
        {
            _stateMachine.StopCoroutine(_noBeansCoroutine);
            _noBeansCoroutine = null;
        }

        if (_followPath != null)
        {
            _stateMachine.StopCoroutine(_followPath);
            _followPath = null;
        }

        _stateMachine = null;
        _targetBean = null;
        _path = null;
        targetIndex = 0;
    }

    // 处理动画事件
    public void OnAnimationEvent(string eventName)
    {

    }

























    //// 进入状态时调用（初始化）
    //public void EnterState(BossFirstStateMachine stateMachine)
    //{
    //    _stateMachine = stateMachine;
    //    _path = new NavMeshPath();
    //    _currentCornerIndex = 0;
    //    _repathTimer = 0f;
    //    _maxVerticalSpeed = _stateMachine.EatBeanMoveSpeed;

    //    _stateMachine.IsMove = true;

    //    SelectNextTarget();
    //}

    //// 每帧更新
    //public void UpdateState()
    //{
    //    //如果没有目标豆子，尝试选择下一个
    //    if(_targetBean == null)
    //    {
    //        SelectNextTarget();
    //        return;
    //    }

    //    //如果被玩家或其他因素破坏,则重新选择目标
    //    if(_targetBean.gameObject == null || !_targetBean.gameObject.activeInHierarchy)
    //    {
    //        _targetBean = null;
    //        SelectNextTarget();
    //        return;
    //    }

    //    //周期重算路径
    //    _repathTimer += Time.deltaTime;
    //    if (_repathTimer >= RepathInterval)
    //    {
    //        _repathTimer = 0f;
    //        TryRepathToTarget();
    //    }
    //}

    //// 固定时间步长更新（物理相关）
    //public void FixedUpdateState()
    //{


    //    #region Move

    //    if (_targetBean == null) return;

    //    // 移动到目标豆子
    //    Vector3 bossPos = _stateMachine.transform.position;
    //    Vector3 targetPos;

    //    if (_path != null && _path.status == NavMeshPathStatus.PathComplete && _path.corners != null && _currentCornerIndex < _path.corners.Length)
    //    {
    //        targetPos = _path.corners[_currentCornerIndex];
    //    }
    //    else
    //    {
    //        // 没有可用路径时直接走向 bean 的位置（可能被障碍阻挡）
    //        targetPos = _targetBean.transform.position;
    //    }

    //    // 限制竖直速度（保持 Y 轴平滑）
    //    float desiredY = Mathf.MoveTowards(bossPos.y, targetPos.y, _maxVerticalSpeed * Time.fixedDeltaTime);
    //    targetPos.y = desiredY;

    //    Vector3 dir3 = targetPos - bossPos;
    //    // 只在 X/Y 平面判断 corner 到达（2D 游戏常用）
    //    Vector2 dir2 = new Vector2(dir3.x, dir3.y);
    //    float sqrDist = dir2.sqrMagnitude;
    //    if (sqrDist <= _reachCornerThreshold * _reachCornerThreshold)
    //    {
    //        // 到达当前 corner，推进到下一个 corner
    //        if (_path != null && _path.corners != null && _currentCornerIndex < _path.corners.Length - 1)
    //        {
    //            _currentCornerIndex++;
    //        }
    //    }

    //    //move(限制了速度) —— 使用 Rigidbody2D.MovePosition（接受 Vector2）
    //    if (dir2.sqrMagnitude > 1e-6f)
    //    {
    //        Vector3 moveDir3 = dir3.normalized;
    //        Vector2 moveDir2 = new Vector2(moveDir3.x, moveDir3.y);
    //        float speed = _stateMachine.EatBeanMoveSpeed;
    //        _stateMachine.CurrentMoveSpeed = speed;

    //        Vector3 newPos3 = bossPos + moveDir3 * speed * Time.fixedDeltaTime;
    //        Vector2 newPos2 = new Vector2(newPos3.x, newPos3.y);

    //        _stateMachine.Rb.MovePosition(newPos2);

    //        // 2D 朝向：绕 Z 轴旋转（避免使用 LookRotation）
    //        if (moveDir2 != Vector2.zero)
    //        {
    //            float angle = Mathf.Atan2(moveDir2.y, moveDir2.x) * Mathf.Rad2Deg;
    //            Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);

    //            // 平滑旋转，避免瞬间俯仰
    //            _stateMachine.transform.rotation = Quaternion.Slerp(_stateMachine.transform.rotation, targetRot, Mathf.Clamp01(8f * Time.fixedDeltaTime));
    //        }
    //    }

    //    //检查吃豆条件（基于 2D 平面距离）
    //    float distToBean = Vector2.Distance(new Vector2(bossPos.x, bossPos.y), new Vector2(_targetBean.transform.position.x, _targetBean.transform.position.y));

    //    #endregion

    //}

    //// 退出状态时调用（清理）
    //public void ExitState()
    //{
    //    if (_noBeansCoroutine != null)
    //    {
    //        _stateMachine.StopCoroutine(_noBeansCoroutine);
    //        _noBeansCoroutine = null;
    //    }

    //    _targetBean = null;
    //    _path = null;
    //}

    //// 处理动画事件
    //public void OnAnimationEvent(string eventName)
    //{

    //}

    //#region Found Path

    //private void SelectNextTarget()
    //{
    //    Bean[] beans = UnityEngine.Object.FindObjectsOfType<Bean>();
    //    if(beans == null || beans.Length == 0)
    //    {
    //        // 若尚未运行等待协程，则启动等待；已经在等待中则直接返回
    //        if (_noBeansCoroutine == null)
    //        {
    //            _noBeansCoroutine = _stateMachine.StartCoroutine(NoBeansWaitCoroutine());
    //        }
    //        return;
    //    }

    //    // 若在等待阶段出现了豆子，取消等待协程
    //    if (_noBeansCoroutine != null)
    //    {
    //        _stateMachine.StopCoroutine(_noBeansCoroutine);
    //        _noBeansCoroutine = null;
    //    }

    //    var ordered = beans.OrderByDescending(b => Vector3.Distance(_stateMachine.transform.position, b.transform.position)).ToArray();

    //    foreach (var candidate in ordered)
    //    {
    //        if (candidate == null || candidate.gameObject == null || !candidate.gameObject.activeInHierarchy) continue;

    //        // 尝试通过 NavMesh 计算路径
    //        if (TryComputePath(candidate.transform.position, out NavMeshPath candidatePath) && candidatePath.status == NavMeshPathStatus.PathComplete)
    //        {
    //            _targetBean = candidate;
    //            _path = candidatePath;
    //            _currentCornerIndex = 0;
    //            _repathTimer = 0f;
    //            return;
    //        }
    //    }

    //    // 如果没有任何远的可达 bean，再尝试选择任意最近的（降级处理）
    //    var fallback = ordered.OrderBy(b => Vector3.Distance(_stateMachine.transform.position, b.transform.position)).FirstOrDefault();
    //    if (fallback != null)
    //    {
    //        // 仍然尝试直线路径（虽然有障碍）
    //        _targetBean = fallback;
    //        TryComputePath(_targetBean.transform.position, out _path); // 可能是不可完整路径
    //        _currentCornerIndex = 0;
    //        _repathTimer = 0f;
    //    }
    //    else
    //    {
    //        _stateMachine.ChangeState(BossState.AttackRandomMove);
    //    }
    //}

    //private IEnumerator NoBeansWaitCoroutine()
    //{
    //    yield return new WaitForSeconds(NoBeansWaitTime);

    //    // 等待结束后再次检查场上是否有豆子
    //    Bean[] beansAfter = UnityEngine.Object.FindObjectsOfType<Bean>();
    //    _noBeansCoroutine = null;

    //    if (beansAfter == null || beansAfter.Length == 0)
    //    {
    //        _stateMachine.ChangeState(BossState.AttackRandomMove);
    //    }
    //    else
    //    {
    //        // 有豆子则重新选目标
    //        SelectNextTarget();
    //    }
    //}

    //private bool TryRepathToTarget()
    //{
    //    if (_targetBean == null) return false;
    //    return TryComputePath(_targetBean.transform.position, out _path);
    //}


    //private bool TryComputePath(Vector3 destination, out NavMeshPath outPath)
    //{
    //    outPath = new NavMeshPath();
    //    try
    //    {
    //        Vector3 source = _stateMachine.transform.position;
    //        // 使用 NavMesh 计算完全路径
    //        bool ok = NavMesh.CalculatePath(source, destination, NavMesh.AllAreas, outPath);
    //        return ok;
    //    }
    //    catch (Exception)
    //    {
    //        // 如果 NavMesh 不可用或发生异常，返回失败（上层会做降级移动）
    //        outPath = null;
    //        return false;
    //    }
    //}

    //#endregion
}
