using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEatBeansRangedAttackState_First : IBossStateFirstStage
{
    private BossFirstStateMachine _stateMachine;

    private Coroutine _RangedAttack;

    private Transform _playerPos;

    // 进入状态时调用（初始化）
    public void EnterState(BossFirstStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        //if (_stateMachine.Animator != null)
        //{
        //    _stateMachine.Animator.SetTrigger("EatBeansRangedAttack");
        //}

        if (_playerPos == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                _playerPos = playerObject.transform;
        }

        _RangedAttack = _stateMachine.StartCoroutine(RangedAttack());

        _stateMachine.IsMove = true;
    }

    #region Attack
    private IEnumerator RangedAttack()
    {
        if(_stateMachine.BulletPrefab == null || _stateMachine.FirePoint == null || _playerPos == null)
        {
            Debug.LogWarning("Bullet Prefab, Fire Point, or Player Position is not assigned.");

            _stateMachine.ChangeState(BossState.EatBeans);
        }
        else if(_stateMachine.BulletPrefab != null && _stateMachine.FirePoint != null && _playerPos != null)
        {
            _stateMachine.IsMove = false;

            GameObject bullet = UnityEngine.Object.Instantiate(_stateMachine.BulletPrefab, _stateMachine.FirePoint.position, _stateMachine.FirePoint.rotation);

            Vector2 direction = (_playerPos.position - _stateMachine.FirePoint.position).normalized;

            if(bullet.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.velocity = direction * _stateMachine.BulletSpeed;
            }

            _stateMachine.IsMove = true;
        }

        yield return new WaitForSeconds(_stateMachine.EatBeansRangedAttackInvulnerableTime);

        _stateMachine.ChangeState(BossState.EatBeans);
    }

    #endregion

    // 固定时间步长更新（物理相关）
    public void FixedUpdateState()
    {
        if (_stateMachine == null) return;

        //跟随移动
        if (_stateMachine.IsMove) _stateMachine.SegmentsMove();
    }

    // 退出状态时调用（清理）
    public void ExitState()
    {
        if(_RangedAttack != null)
        {
            _stateMachine.StopCoroutine(_RangedAttack);
            _RangedAttack = null;
        }
        _stateMachine = null;
    }









    // 每帧更新
    public void UpdateState()
    {

    }

    // 处理动画事件
    public void OnAnimationEvent(string eventName)
    {

    }


}
