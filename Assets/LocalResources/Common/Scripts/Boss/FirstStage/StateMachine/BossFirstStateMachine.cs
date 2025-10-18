using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState
{
    //吃豆环节状态
    EatBeans,            //吃豆
    EatBeansRangedAttack, //吃豆环节远程攻击

    //攻击类型状态
    RangedAttack,       //远程攻击
    Teleport,           //传送
    DashAttack,         //冲锋攻击
    AttackRandomMove,   //攻击类型的移动

    //通用类型状态
    Hurt,               //受伤
    Die,                //死亡
    Grow,               //蛇身变长一节
}

public class BossFirstStateMachine : MonoBehaviour
{
    [Header("状态配置")]
    public BossState startingState;

    [Header("调试信息")]
    [SerializeField] private BossState _currentState;
    [SerializeField] private string _currentStateName;

    [Header("属性配置")]
    [Range(10f, 1000f)] public float MaxHealth = 100f;
    [Range(1f, 20f)] public float EatBeanMoveSpeed = 3f;
    [Range(1f, 20f)] public float AttackMoveSpeed = 5f;
    private float _currentMoveSpeed;

    [Header("蛇的节数配置")]
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

    // 状态字典
    private Dictionary<BossState, IBossStateFirstStage> _states;
    private IBossStateFirstStage _currentStateInstance;

    // 组件引用（所有状态共享）
    public Animator Animator { get; private set; }
    public Transform Player { get; private set; }
    public Rigidbody Rb { get; private set; }

    public float CurrentHealth { get; set; }

    #region LifeCycle
    void Awake()
    {
        // 获取组件引用
        Animator = GetComponent<Animator>();
        Rb = GetComponent<Rigidbody>();
        Player = GameObject.FindGameObjectWithTag("Player").transform;

        // 初始化状态字典
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
        _segments.Add(this.transform); // 添加头部作为第一节

        CurrentHealth = MaxHealth;
        CurrentMoveSpeed = EatBeanMoveSpeed;


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

    #endregion

    #region ChangeState

    // 状态切换方法
    public void ChangeState(BossState newState)
    {
        // 退出当前状态
        _currentStateInstance?.ExitState();

        // 获取新状态实例
        if (_states.TryGetValue(newState, out IBossStateFirstStage nextState))
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
            targetPosition.y = currentSegment.position.y; // 保持y轴位置不变

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

            // 保持水平旋转
            Vector3 direction = previousSegment.position - currentSegment.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 1e-6f)
            {
                direction.Normalize();
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                // 使用 step 作为插值因子（可根据需要改为其他系数）
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

    #endregion

    #region Animation Events

    // 动画事件回调
    public void OnAnimationEvent(string eventName)
    {
        _currentStateInstance?.OnAnimationEvent(eventName);
    }

    #endregion
}
