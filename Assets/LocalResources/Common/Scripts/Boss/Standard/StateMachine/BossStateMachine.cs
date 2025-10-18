using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 状态枚举
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
    [Header("状态配置")]
    public BossState startingState = BossState.Idle;

    [Header("调试信息")]
    [SerializeField] private BossState _currentState;
    [SerializeField] private string _currentStateName;

    // 状态字典
    private Dictionary<BossState, IBossState> _states;
    private IBossState _currentStateInstance;

    // 组件引用（所有状态共享）
    public Animator Animator { get; private set; }
    public Transform Player { get; private set; }
    public Rigidbody Rb { get; private set; }

    // Boss属性
    public float CurrentHealth { get; set; }
    public float MaxHealth { get; private set; } = 100f;

    void Awake()
    {
        // 获取组件引用
        Animator = GetComponent<Animator>();
        Rb = GetComponent<Rigidbody>();
        Player = GameObject.FindGameObjectWithTag("Player").transform;

        // 初始化状态字典
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
        // 初始状态
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

    // 状态切换方法
    public void ChangeState(BossState newState)
    {
        // 退出当前状态
        _currentStateInstance?.ExitState();

        // 获取新状态实例
        if (_states.TryGetValue(newState, out IBossState nextState))
        {
            _currentStateInstance = nextState;
            _currentState = newState;
            _currentStateName = newState.ToString();

            // 进入新状态
            _currentStateInstance.EnterState(this);
        }
        else
        {
            Debug.LogError($"状态 {newState} 未在字典中注册!");
        }
    }

    // 公共方法供状态调用
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

    // 被玩家攻击时调用
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

    // 动画事件回调
    public void OnAnimationEvent(string eventName)
    {
        _currentStateInstance?.OnAnimationEvent(eventName);
    }

    // 获取当前状态（只读）
    public BossState CurrentState => _currentState;
}
