using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditorInternal;
using UnityEngine;

public class BossAttackRandomMoveState_First : IBossStateFirstStage
{
    private BossFirstStateMachine _stateMachine;

    private Coroutine _attackRandomMove;
    private int targetIndex;
    private Vector2 _chooseJumpPos;
    private Transform _chooseTrans;

    private bool _isLeft;

    // 进入状态时调用（初始化）
    public void EnterState(BossFirstStateMachine stateMachine)
    {
        this._stateMachine = stateMachine;
        //if (_stateMachine.Animator != null)
        //{
        //    _stateMachine.Animator.SetTrigger("AttackRandomMove");
        //}

        for (int i = 0; i < _stateMachine.Segments.Count; i++)
        {
            Rigidbody2D rb = _stateMachine.Segments[i].GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
        }
        RandomMoveAttack();
    }

    private void RandomMoveAttack()
    {
        _stateMachine.IsMove = false;
        _stateMachine.CurrentMoveSpeed = _stateMachine.AttackRandomMoveUpSpeed;

        // 检查必要的Transform引用
        if (_stateMachine.AttackRandomMoveJumpLeftPostion == null ||
            _stateMachine.AttackRandomMoveJumpRightPostion == null)
        {
            Debug.LogError("AttackRandomMoveJump positions are not assigned!");
        }

        _isLeft = _stateMachine.transform.position.x <= _stateMachine.AttackRandomMoveJumpLeftPostion.position.x;

        SelectPoint();
        if (TelePort(_chooseTrans.position))
        {
            _stateMachine.IsMove = true;
            _attackRandomMove = _stateMachine.StartCoroutine(AttackRandomMove());
        }
    }


    #region AttackRandomMove
    private void SelectPoint()
    {
        float randomX = UnityEngine.Random.Range(_stateMachine.AttackRandomMoveJumpLeftPostion.position.x, _stateMachine.AttackRandomMoveJumpRightPostion.position.x);
        _chooseJumpPos = new Vector2(randomX, _stateMachine.AttackRandomMoveJumpLeftPostion.position.y);

        if (_isLeft)
        {
            _chooseTrans = _stateMachine.AttackRandomMoveStartPostionLeftPoint;
        }
        else
        {
            _chooseTrans = _stateMachine.AttackRandomMoveStartPostionRightPoint;
        }
    }

    private bool TelePort(Vector2 position)
    {
        _stateMachine.transform.position = position;

        Rigidbody2D rb = _stateMachine.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        return SetupSegmentsPosition();
    }

    private bool SetupSegmentsPosition()
    {
        for (int i = 1; i < _stateMachine.Segments.Count; i++)
        {
            // 使用相对位置
            Vector3 segmentOffset = _isLeft ?
                new Vector3(-i * 4.5f, 0, 0) :
                new Vector3(i * 4.5f, 0, 0);

            _stateMachine.Segments[i].position = _stateMachine.transform.position + segmentOffset;
        }

        return true;
    }

    private IEnumerator AttackRandomMove()
    {
        while (true)
        {
            _stateMachine.transform.position = Vector3.MoveTowards(_stateMachine.transform.position, _chooseJumpPos, _stateMachine.CurrentMoveSpeed * Time.deltaTime);

            yield return null;

            float distToTarget = Vector2.Distance(_stateMachine.transform.position, _chooseJumpPos);
            if (distToTarget <= _stateMachine.DashTargetCheckLength)
            {
                break;
            }
        }

        yield return new WaitForSeconds(_stateMachine.UpStayTime);

        Vector2 groundTarget = new Vector2(_stateMachine.transform.position.x, _stateMachine.GroundPoint.position.y);

        _stateMachine.CurrentMoveSpeed = _stateMachine.AttackRandomMoveDownSpeed;

        while (true)
        {
            _stateMachine.transform.position = Vector3.MoveTowards(_stateMachine.transform.position, groundTarget, _stateMachine.CurrentMoveSpeed * Time.deltaTime);

            yield return null;

            float distToGround = Vector2.Distance(_stateMachine.transform.position, groundTarget);
            if (distToGround <= _stateMachine.DashTargetCheckLength)
            {
                Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(_stateMachine.transform.position, _stateMachine.DownHurtRange, _stateMachine.PlayerLayer);

                if (hitPlayer != null && hitPlayer.Length > 0)
                {
                    // 确保PlayerHealth实例存在
                    if (PlayerHealth.Ins != null)
                    {
                        PlayerHealth.Ins.TakeDamageByEnemy(_stateMachine.DownDamage);
                    }
                    else
                    {
                        Debug.LogWarning("PlayerHealth instance is null!");
                    }
                }

                yield return new WaitForSeconds(0.1f);

                if (UnityEngine.Object.FindObjectsOfType<Bean>() != null)
                {
                    _stateMachine.ChangeState(BossState.EatBeans);
                }
                else
                {
                    //分为站立,冲锋攻击，先上在下的随机移动两种
                    int randonNum = UnityEngine.Random.Range(0, 3);
                    if (randonNum == 0)
                    {
                        _stateMachine.ChangeState(BossState.DashAttack);
                    }
                    else if (randonNum == 1)
                    {
                        _stateMachine.ChangeState(BossState.AttackRandomMove);
                    }
                    else
                    {
                        _stateMachine.ChangeState(BossState.AttackIdle);
                    }
                }
                yield break;
            }
        }
        //_stateMachine.transform.position = Vector3.down * _stateMachine.CurrentMoveSpeed * Time.deltaTime;
    }

    #endregion

    // 每帧更新
    public void UpdateState()
    {
        
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
        if (_attackRandomMove != null)
        {
            _stateMachine.StopCoroutine(_attackRandomMove);
            _attackRandomMove = null;
        }
        _stateMachine = null;
    }














    // 处理动画事件
    public void OnAnimationEvent(string eventName)
    {

    }
}
