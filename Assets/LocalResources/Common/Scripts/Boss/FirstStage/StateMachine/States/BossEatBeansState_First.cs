using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BossEatBeansState_First : IBossStateFirstStage
{
    [Range(0.5f, 2f)] public float RepathInterval = 0.5f;                  // ��������·������ֹ�ϰ�/�����ƶ�����·��ʧЧ
    [Range(0.1f, 3f)] public float EatDistance = 0.8f;                     // ����˾�����Ϊ���Ե�������

    private BossFirstStateMachine _stateMachine;
    private Bean _targetBean;
    private NavMeshPath _path;
    private int _currentCornerIndex;
    private float _repathTimer;
    private float NoBeansWaitTime = 0.2f;                             // û�ж���ʱ�ĵȴ�ʱ��

    private readonly float _reachCornerThreshold = 0.2f;
    private float _maxVerticalSpeed;                                // �����ֱ�ٶ�

    private Coroutine _noBeansCoroutine;

    // ����״̬ʱ���ã���ʼ����
    public void EnterState(BossFirstStateMachine stateMachine)
    {
        _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        _path = new NavMeshPath();
        _currentCornerIndex = 0;
        _repathTimer = 0f;
        _maxVerticalSpeed = _stateMachine.EatBeanMoveSpeed;

        SelectNextTarget();
    }

    // ÿ֡����
    public void UpdateState()
    {
        //���û��Ŀ�궹�ӣ�����ѡ����һ��
        if(_targetBean == null)
        {
            SelectNextTarget();
            return;
        }

        //�������һ����������ƻ�,������ѡ��Ŀ��
        if(_targetBean == null || _targetBean.gameObject == null || !_targetBean.gameObject.activeInHierarchy)
        {
            _targetBean = null;
            SelectNextTarget();
            return;
        }

        //��������·��
        _repathTimer += Time.deltaTime;
        if (_repathTimer >= RepathInterval)
        {
            _repathTimer = 0f;
            TryRepathToTarget();
        }
    }

    // �̶�ʱ�䲽�����£�������أ�
    public void FixedUpdateState()
    {
        if (_stateMachine == null) return;

        //�����ƶ�
        _stateMachine.SegmentsMove();

        if(_targetBean == null) return;

        //�ƶ���Ŀ�궹��
        Vector3 bossPos = _stateMachine.Rb.position;
        Vector3 targetPos;

        if (_path != null && _path.status == NavMeshPathStatus.PathComplete && _path.corners != null && _currentCornerIndex < _path.corners.Length)
        {
            targetPos = _path.corners[_currentCornerIndex];
        }
        else
        {
            // û�п���·��ʱֱ������ bean ��λ�ã����ܱ��ϰ��赲��
            targetPos = _targetBean.transform.position;
        }

        float desiredY = Mathf.MoveTowards(bossPos.y, targetPos.y, _maxVerticalSpeed * Time.fixedDeltaTime);
        targetPos.y = desiredY;

        Vector3 dir = targetPos - bossPos;
        float sqrDist = dir.sqrMagnitude;
        if (sqrDist <= _reachCornerThreshold * _reachCornerThreshold)
        {
            // ���ﵱǰ corner���ƽ�����һ�� corner
            if (_path != null && _path.corners != null && _currentCornerIndex < _path.corners.Length - 1)
            {
                _currentCornerIndex++;
            }
        }

        //move(�������ٶ�)
        if (dir.sqrMagnitude > 1e-6f)
        {
            Vector3 moveDir = dir.normalized;
            float speed = _stateMachine.EatBeanMoveSpeed;
            _stateMachine.CurrentMoveSpeed = speed;
            Vector3 newPos = bossPos + moveDir * speed * Time.fixedDeltaTime;
            _stateMachine.Rb.MovePosition(newPos);

            //����
            Vector3 look = moveDir;
            
            if(look != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(look);

                // ƽ����ת������˲�丩��
                _stateMachine.transform.rotation = Quaternion.Slerp(_stateMachine.transform.rotation, targetRot, Mathf.Clamp01(8f * Time.fixedDeltaTime));
            }
        }

        //���Զ�����
        float distToBean = Vector3.Distance(bossPos, _targetBean.transform.position);
        if (distToBean <= EatDistance)
        {
            //û�Ӷ���

            UnityEngine.Object.Destroy(_targetBean.gameObject);
            _targetBean = null;
            SelectNextTarget();
        }
        
    }

    // �˳�״̬ʱ���ã�����
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

    // �������¼�
    public void OnAnimationEvent(string eventName)
    {

    }

    #region Found Path

    private void SelectNextTarget()
    {
        Bean[] beans = UnityEngine.Object.FindObjectsOfType<Bean>();
        if(beans == null || beans.Length == 0)
        {
            // ����δ���еȴ�Э�̣��������ȴ����Ѿ��ڵȴ�����ֱ�ӷ���
            if (_noBeansCoroutine == null)
            {
                _noBeansCoroutine = _stateMachine.StartCoroutine(NoBeansWaitCoroutine());
            }
            return;
        }

        // ���ڵȴ��׶γ����˶��ӣ�ȡ���ȴ�Э��
        if (_noBeansCoroutine != null)
        {
            _stateMachine.StopCoroutine(_noBeansCoroutine);
            _noBeansCoroutine = null;
        }

        var ordered = beans.OrderByDescending(b => Vector3.Distance(_stateMachine.transform.position, b.transform.position)).ToArray();

        foreach (var candidate in ordered)
        {
            if (candidate == null || candidate.gameObject == null || !candidate.gameObject.activeInHierarchy) continue;

            // ����ͨ�� NavMesh ����·��
            if (TryComputePath(candidate.transform.position, out NavMeshPath candidatePath) && candidatePath.status == NavMeshPathStatus.PathComplete)
            {
                _targetBean = candidate;
                _path = candidatePath;
                _currentCornerIndex = 0;
                _repathTimer = 0f;
                return;
            }
        }

        // ���û���κ�Զ�Ŀɴ� bean���ٳ���ѡ����������ģ���������
        var fallback = ordered.OrderBy(b => Vector3.Distance(_stateMachine.transform.position, b.transform.position)).FirstOrDefault();
        if (fallback != null)
        {
            // ��Ȼ����ֱ��·������Ȼ���ϰ���
            _targetBean = fallback;
            TryComputePath(_targetBean.transform.position, out _path); // �����ǲ�������·��
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

        // �ȴ��������ٴμ�鳡���Ƿ��ж���
        Bean[] beansAfter = UnityEngine.Object.FindObjectsOfType<Bean>();
        _noBeansCoroutine = null;

        if (beansAfter == null || beansAfter.Length == 0)
        {
            _stateMachine.ChangeState(BossState.AttackRandomMove);
        }
        else
        {
            // �ж���������ѡĿ��
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
            // ʹ�� NavMesh ������ȫ·��
            bool ok = NavMesh.CalculatePath(source, destination, NavMesh.AllAreas, outPath);
            return ok;
        }
        catch (Exception)
        {
            // ��� NavMesh �����û����쳣������ʧ�ܣ��ϲ���������ƶ���
            outPath = null;
            return false;
        }
    }

    #endregion
}
