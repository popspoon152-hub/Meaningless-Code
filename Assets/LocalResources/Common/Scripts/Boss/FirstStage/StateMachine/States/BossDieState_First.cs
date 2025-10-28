using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossDieState_First : IBossStateFirstStage
{
    private BossFirstStateMachine _stateMachine;

    private Coroutine _bossDie;

    // 进入状态时调用（初始化）
    public void EnterState(BossFirstStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        //if (_stateMachine.Animator != null)
        //{
        //    _stateMachine.Animator.SetTrigger("Die");
        //}

        _bossDie = _stateMachine.StartCoroutine(BossDie());
    }

    private IEnumerator BossDie()
    {
        _stateMachine.transform.position = _stateMachine.BossDieTransform.position;

        Rigidbody2D rb = _stateMachine.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        for (int i = 1; i < _stateMachine.Segments.Count; i++)
        {
            // 使用相对位置
            Vector3 segmentOffset = new Vector3(i * 4.5f, 0, 0);

            _stateMachine.Segments[i].position = _stateMachine.transform.position + segmentOffset;

            Rigidbody2D r = _stateMachine.Segments[i].GetComponent<Rigidbody2D>();
            if (r != null)
            {
                r.velocity = Vector2.zero;
                r.angularVelocity = 0f;
            }
        }

        GameObject reward = UnityEngine.Object.Instantiate(_stateMachine.RewardPrefab, _stateMachine.RewardTransform.position,Quaternion.identity);

        yield return new WaitForSeconds(_stateMachine.DieInvulnerableTime);
    }

    // 退出状态时调用（清理）
    public void ExitState()
    {
        if (_bossDie != null)
        {
            _stateMachine.StopCoroutine(_bossDie);
            _bossDie = null;
        }
        _stateMachine = null;
    }











    // 每帧更新
    public void UpdateState()
    {

    }

    // 固定时间步长更新（物理相关）
    public void FixedUpdateState()
    {

    }

    // 处理动画事件
    public void OnAnimationEvent(string eventName)
    {

    }
}
