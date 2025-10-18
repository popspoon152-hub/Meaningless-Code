using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState
{
    //�ƶ�����״̬
    EatBeans,            //�Զ�

    //��������״̬
    RangedAttack,       //Զ�̹���
    Teleport,           //����
    DashAttack,         //��湥��
    AttackRandomMove,   //�������͵�����ƶ�

    //ͨ������״̬
    Hurt,               //����
    Die,                //����
    Grow,               //����䳤һ��
}

public class BossFirstStateMachine : MonoBehaviour
{
    [Header("״̬����")]
    public BossState startingState;

    [Header("������Ϣ")]
    [SerializeField] private BossState _currentState;
    [SerializeField] private string _currentStateName;

    [Header("��������")]
    [Range(10f, 1000f)] public float MaxHealth = 100f;
    [Range(1f, 20f)] public float EatBeanMoveSpeed = 3f;
    [Range(1f, 20f)] public float AttackMoveSpeed = 5f;
    private float _currentMoveSpeed;

    [Header("�ߵĽ�������")]
    [Range(1f, 20f)] public int StartSnakeSegments = 5;
    [Range(1f, 20f)] public int MaxSnakeSegments = 10;

    public Transform SegmentPrefab;
    [HideInInspector] public List<Transform> _segments = new List<Transform>();
    [HideInInspector] public bool IsMove = true;

    public BossState CurrentState => _currentState;
    public float CurrentMoveSpeed
    {
        get { return _currentMoveSpeed; }
        set {  _currentMoveSpeed = value; }
    }

    // ״̬�ֵ�
    private Dictionary<BossState, IBossStateFirstStage> _states;
    private IBossStateFirstStage _currentStateInstance;

    // ������ã�����״̬����
    public Animator Animator { get; private set; }
    public Transform Player { get; private set; }
    public Rigidbody Rb { get; private set; }

    public float CurrentHealth { get; set; }

    #region LifeCycle
    void Awake()
    {
        // ��ȡ�������
        Animator = GetComponent<Animator>();
        Rb = GetComponent<Rigidbody>();
        Player = GameObject.FindGameObjectWithTag("Player").transform;

        // ��ʼ��״̬�ֵ�
        _states = new Dictionary<BossState, IBossStateFirstStage>
        {
            { BossState.EatBeans, new BossEatBeansState_First() },
            { BossState.RangedAttack, new BossRangedAttackState_First() },
            { BossState.Teleport, new BossTeleportState_First() },
            { BossState.DashAttack, new BossDashAttackState_First() },
            { BossState.AttackRandomMove, new BossAttackRandomMoveState_First() },
            { BossState.Hurt, new BossHurtState() },
            { BossState.Die, new BossDieState_First() },
            { BossState.Grow, new BossGrowState_First() },
        };
    }

    void Start()
    {
        _segments.Add(this.transform); // ���ͷ����Ϊ��һ��

        CurrentHealth = MaxHealth;
        CurrentMoveSpeed = EatBeanMoveSpeed;


        // ��ʼ״̬
        ChangeState(startingState);
    }

    void Update()
    {
        _currentStateInstance?.UpdateState();
    }

    void FixedUpdate()
    {
        _currentStateInstance?.FixedUpdateState();
    }

    #endregion

    #region ChangeState

    // ״̬�л�����
    public void ChangeState(BossState newState)
    {
        // �˳���ǰ״̬
        _currentStateInstance?.ExitState();

        // ��ȡ��״̬ʵ��
        if (_states.TryGetValue(newState, out IBossStateFirstStage nextState))
        {
            _currentStateInstance = nextState;
            _currentState = newState;
            _currentStateName = newState.ToString();

            // ������״̬
            _currentStateInstance.EnterState(this);
        }
        else
        {
            Debug.LogError($"״̬ {newState} δ���ֵ���ע��!");
        }
    }

    #endregion

    #region Boss Move

    public void SegmentsMove()
    {
        if (_segments == null || _segments.Count < 2) return;

        float step = CurrentMoveSpeed * Time.fixedDeltaTime;

        for (int i = _segments.Count - 1; i > 0; i--)
        {
            Transform currentSegment = _segments[i];
            Transform previousSegment = _segments[i - 1];

            Vector3 targetPosition = previousSegment.position;
            targetPosition.y = currentSegment.position.y; // ����y��λ�ò���

            Rigidbody rb = currentSegment.GetComponent<Rigidbody>();
            Vector3 newPos = Vector3.MoveTowards(currentSegment.position, targetPosition, step);
            if (rb != null)
            {
                rb.MovePosition(newPos);
            }
            else
            {
                currentSegment.position = newPos;
            }

            // ����ˮƽ��ת
            Vector3 direction = previousSegment.position - currentSegment.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 1e-6f)
            {
                direction.Normalize();
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                // ʹ�� step ��Ϊ��ֵ���ӣ��ɸ�����Ҫ��Ϊ����ϵ����
                currentSegment.rotation = Quaternion.Slerp(currentSegment.rotation, targetRotation, Mathf.Clamp01(step));
            }
        }
    }

    #endregion

    #region Player

    public bool IsPlayerInRange(float range)
    {
        return Vector3.Distance(transform.position, Player.position) <= range;
    }

    public void LookAtPlayer()
    {
        Vector3 direction = (Player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    // ����ҹ���ʱ����
    public void TakeDamage(float damage)
    {
        if (_currentState == BossState.Die) return;

        CurrentHealth -= damage;

        if (CurrentHealth <= 0)
        {
            ChangeState(BossState.Die);
        }
        else
        {
            ChangeState(BossState.Hurt);
        }
    }

    #endregion

    #region Animation Events

    // �����¼��ص�
    public void OnAnimationEvent(string eventName)
    {
        _currentStateInstance?.OnAnimationEvent(eventName);
    }

    #endregion
}
