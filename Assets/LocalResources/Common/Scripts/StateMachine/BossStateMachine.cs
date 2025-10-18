using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ״̬ö��
public enum BossState
{
    Idle,
    Chase,
    Attack1,
    Attack2,
    Hurt,
    Die
}
public class BossStateMachine : MonoBehaviour
{
    [Header("״̬����")]
    public BossState startingState = BossState.Idle;

    [Header("������Ϣ")]
    [SerializeField] private BossState _currentState;
    [SerializeField] private string _currentStateName;

    // ״̬�ֵ�
    private Dictionary<BossState, IBossState> _states;
    private IBossState _currentStateInstance;

    // ������ã�����״̬����
    public Animator Animator { get; private set; }
    public Transform Player { get; private set; }
    public Rigidbody Rb { get; private set; }

    // Boss����
    public float CurrentHealth { get; set; }
    public float MaxHealth { get; private set; } = 100f;

    void Awake()
    {
        // ��ȡ�������
        Animator = GetComponent<Animator>();
        Rb = GetComponent<Rigidbody>();
        Player = GameObject.FindGameObjectWithTag("Player").transform;

        // ��ʼ��״̬�ֵ�
        _states = new Dictionary<BossState, IBossState>
        {
          /*  { BossState.Idle, new BossIdleState() },
            { BossState.Chase, new BossChaseState() },
            { BossState.Attack1, new BossAttack1State() },
            { BossState.Attack2, new BossAttack2State() },
            { BossState.Hurt, new BossHurtState() },
            { BossState.Die, new BossDieState() }
*/        };

        CurrentHealth = MaxHealth;
    }

    void Start()
    {
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

    // ״̬�л�����
    public void ChangeState(BossState newState)
    {
        // �˳���ǰ״̬
        _currentStateInstance?.ExitState();

        // ��ȡ��״̬ʵ��
        if (_states.TryGetValue(newState, out IBossState nextState))
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

    // ����������״̬����
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

    // �����¼��ص�
    public void OnAnimationEvent(string eventName)
    {
        _currentStateInstance?.OnAnimationEvent(eventName);
    }

    // ��ȡ��ǰ״̬��ֻ����
    public BossState CurrentState => _currentState;
}
