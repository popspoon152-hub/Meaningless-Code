using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEatBeansRangedAttackState_First : IBossStateFirstStage
{
    private BossFirstStateMachine _stateMachine;

    [Header("子弹")]
    public GameObject BulletPrefab;
    public Transform FirePoint;
    [Range(1f, 14f)] public float BulletSpeed = 5f;

    [Header("Boss出招僵直时间")]
    [Range(0f, 2f)] public float StateInvulnerableTime;
    private Coroutine _RangedAttack;

    [Header("调试")]
    [SerializeField] private Transform _playerPos;

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
    }

    private IEnumerator RangedAttack()
    {
        if(BulletPrefab == null || FirePoint == null || _playerPos == null)
        {
            Debug.LogWarning("Bullet Prefab, Fire Point, or Player Position is not assigned.");

            _stateMachine.ChangeState(BossState.EatBeans);
        }
        else if(BulletPrefab != null && FirePoint != null && _playerPos != null)
        {
            _stateMachine.IsMove = false;

            GameObject bullet = UnityEngine.Object.Instantiate(BulletPrefab, FirePoint.position, FirePoint.rotation);

            Vector2 direction = (_playerPos.position - FirePoint.position).normalized;

            if(bullet.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.velocity = direction * BulletSpeed;
            }

            _stateMachine.IsMove = true;
        }

        yield return new WaitForSeconds(StateInvulnerableTime);

        _stateMachine.ChangeState(BossState.EatBeans);
    }

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
        _stateMachine = null;
        if(_RangedAttack != null)
        {
            _stateMachine.StopCoroutine(_RangedAttack);
            _RangedAttack = null;
        }
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
