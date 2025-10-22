using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState_Third
{
    // 各种攻击类型状态
    HoleAttack,          // 黑洞攻击
    DevourGround,        // 吞噬地面
    Teleport,            // 传送
    DashAttack,          // 冲锋攻击
    SmashAttack,         // 下砸攻击

    // 攻击待机状态
    AttackIdle,          // 攻击待机
    AttackRandomMove,    // 攻击类型的随机移动

    // 通用状态
    Hurt,                // 受伤
    Die,                 // 死亡
}

public class BossThirdStateMachine : MonoBehaviour
{
    #region State Machine Config
    [Header("状态配置")]
    public BossState_Third startingState = BossState_Third.AttackRandomMove;

    [Header("调试信息")]
    [SerializeField] private BossState_Third _currentState_Third;
    [SerializeField] private string _currentStateName_Third;

    #endregion

    #region Boss属性
    [Header("属性配置")]
    [Range(10f, 1000f)] public float MaxHealth = 100f;
    //[Range(1f, 20f)] public float AttackMoveSpeed = 5f;
    [SerializeField] private float _currentMoveSpeed_Third;
    [HideInInspector] public bool IsMove = true;

    [Header("受击配置")]
    [Range(0f, 1f)] public float HurtInvulnerableTime = 0.1f;       //受击后的短暂无敌时间(避免重复判定)
    #endregion

    #region Boss移动与碰撞配置
    [Header("Boss移动速度配置")]
    [Range(1f, 20f)] public float MoveSpeed_Normal = 4f;        // 普通移动速度
    [Range(1f, 20f)] public float MoveSpeed_Attack = 7f;        // 攻击移动速度（用于AttackRandomMove）
    [Range(1f, 30f)] public float MoveSpeed_Dash = 12f;         // 冲锋攻击速度
    [Range(1f, 30f)] public float MoveSpeed_ToIdle = 12f;         // 前往待机点速度
    //[Range(1f, 30f)] public float MoveSpeed_Teleport = 10f;     // 闪现滑行速度（可选）

    [Header("碰撞伤害配置")]
    [Range(1f, 100f)] public float CollisionDamage = 10f;        // 碰撞对玩家造成的伤害
    //[Range(0.1f, 5f)] public float CollisionCooldown = 1f;       // 连续碰撞伤害冷却时间
    private bool _canDealCollisionDamage = true;
    #endregion

    #region BossDashAttackState_Third
    [Header("BossDashAttackState_Third 配置")]
    [Range(5f, 50f)] public float DashSpeed = 20f;               // 冲刺速度
    [Range(0.1f, 3f)] public float DashChargeTime = 1f;           // 冲刺蓄力时间
    [Range(1f, 50f)] public float DashMaxDistance = 15f;          // 冲刺最大距离限制
    [Range(0.1f, 1f)] public float DashTrailSpawnInterval = 0.2f; // 路径标记生成间隔

    [Header("BossDashAttackState_Third 伤害配置")]
    [Range(1f, 50f)] public float DashTrailDamagePerTick = 5f;    // 持续伤害区域的伤害
    [Range(1f, 100f)] public float DashImpactDamage = 20f;        // 冲刺直接撞击玩家伤害

    [Header("BossDashAttackState_Third 特效与检测")]
    public GameObject DashTrailPrefab;                            // 路径标记预制体
    public LayerMask WallLayerMask;                               // 冲刺检测墙体的层
    [HideInInspector] public bool IsCharging = false;              // 用于动画触发判断
    #endregion


    #region 吞噬地面配置
    [Header("Boss吞噬地面配置")]
    public GroundTileManager GroundTileManager;          // 地面管理器
    [Range(0f, 5f)] public float DevourPreWarnTime = 1f; // 吞噬前摇时间（预警时间）
    [Range(1f, 30f)] public float DevourSpeed = 10f;     // 吞噬时的横向移动速度
                                                         //[Range(1f, 10f)] public float DevourDistance = 5f;   // 吞噬的最大范围（从平台位置横向穿越的距离）(未使用，距离直接算的地砖长度)
                                                         //[Range(0f, 5f)] public float DevourTileRespawnTime = 5f; // 吞噬后的地砖重生时间
    #endregion

    #region BossHoleAttackState_Third 配置
    [Header("BossHoleAttackState_Third 配置")]
    [Range(0f, 5f)] public float HolePreWarnTime = 1.0f;       // 前摇 / 预警时间
    [Range(0f, 2f)] public float HoleSpawnEndDelay = 0.3f;     // 生成完成后延迟
    [Range(0f, 20f)] public float HoleSpawnRangeX = 5f;        // X方向随机生成范围
    [Range(0f, 10f)] public float HoleSpawnRangeY = 3f;        // Y方向随机生成范围
    public GameObject HolePrefab;                               // 黑洞预制体（自身包含吸力与伤害参数）
    #endregion

    #region BossSmashAttackState_Third 配置
    [Header("BossSmashAttackState_Third 配置")]
    [Range(0f, 2f)] public float SmashPreWarnTime = 1.0f;       // 下砸前摇时间（预警）
    [Range(1f, 10f)] public float SmashFallSpeed = 5f;           // 下砸的速度
    [Range(1f, 10f)] public float SmashPostDelay = 0.3f;        // 下砸后的停滞时间
    [Range(1f, 10f)] public float SmashAreaRadius = 3f;         // 下砸范围伤害的半径
    [Range(1f, 50f)] public float SmashImpactDamage = 20f;      // 下砸造成的直接伤害（影响范围内的玩家）
    public LayerMask HoleGroundLayerMask;                       // 用于检测地面是否存在的LayerMask
    #endregion





    #region Move
    [Header("Pathfinding")]
    public Pathfinding pathfinding;
    #endregion


    #region BossAttackIdleState_Third
    [Header("BossAttackIdleState_Third中Boss的位置设定")]
    public Transform IdleLeftTransfrom;
    public Transform IdleRightTransfrom;

    [Header("BossAttackIdleState_Third中Boss的僵直时间")]
    [Range(1f, 5f)] public float IdleTime = 2f;
    #endregion

    //#region BossRangedAttackState_First
    //[Header("BossRangedAttackState_First的子弹")]
    //public GameObject BulletPrefab_Attack;
    //public Transform FirePoint_Attack;
    //[Range(1f, 14f)] public float BulletSpeed_Attack = 5f;

    //[Header("BossRangedAttackState_First的Boss出招僵直时间")]
    //[Range(0f, 2f)] public float RangedAttackInvulnerableTime;
    //#endregion



    #region BossDieState_Third
    [Header("BossDieState_Third的Boss出招僵直时间")]
    [Range(0f, 2f)] public float DieInvulnerableTime;
    #endregion

    #region private fields
    // 运行时字段
    private bool _isInvulnerable = false;
    private Coroutine _hurtCoroutine;

    public BossState_Third CurrentState => _currentState_Third;
    public float CurrentMoveSpeed
    {
        get { return _currentMoveSpeed_Third; }
        set { _currentMoveSpeed_Third = value; }
    }

    // 状态字典
    private Dictionary<BossState_Third,IBossStateThirdStage> _states_Third;
    private IBossStateThirdStage _currentStateInstance;

    // 组件引用（所有状态共享）
    public Animator Animator_Third { get; private set; }
    public Transform Player_Third { get; private set; }
    public Rigidbody2D Rb_Third { get; private set; }

    public float CurrentHealth { get; set; }
    #endregion

    #region LifeCycle
    void Awake()
    {
        // 获取组件引用
        Animator_Third = GetComponent<Animator>();
        Rb_Third = GetComponent<Rigidbody2D>();
        Player_Third = GameObject.FindGameObjectWithTag("Player").transform;

        // 初始化状态字典
        _states_Third = new Dictionary<BossState_Third, IBossStateThirdStage>
        {
            { BossState_Third.DevourGround, new BossDevourGroundState_Third() },
            { BossState_Third.HoleAttack, new BossHoleAttackState_Third() },
            { BossState_Third.Teleport, new BossTeleportState_Third() },
            { BossState_Third.DashAttack, new BossDashAttackState_Third() },
            { BossState_Third.SmashAttack, new BossSmashAttackState_Third() },

            { BossState_Third.AttackIdle, new BossAttackIdleState_Third() },
            { BossState_Third.AttackRandomMove, new BossAttackRandomMoveState_Third() },

            { BossState_Third.Hurt, new BossHurtState_Third() },
            { BossState_Third.Die, new BossDieState_Third() },
        };
    }

    void Start()
    {

        CurrentHealth = MaxHealth;
        CurrentMoveSpeed = MoveSpeed_Normal;


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
    public void ChangeState(BossState_Third newState)
    {
        // 退出当前状态
        _currentStateInstance?.ExitState();

        // 获取新状态实例
        if (_states_Third.TryGetValue(newState, out IBossStateThirdStage nextState))
        {
            _currentStateInstance = nextState;
            _currentState_Third = newState;
            _currentStateName_Third = newState.ToString();

            // 进入新状态
            _currentStateInstance.EnterState(this);
        }
        else
        {
            Debug.LogError($"状态 {newState} 未在字典中注册!");
        }
    }

    #endregion

    #region Attack State Choose
    public void AttackStateChoose()
    {
        //// 根据随机选择攻击状态
        //int randomNum = UnityEngine.Random.Range(0, 5);
        //if (randomNum == 0)
        //{
        //    ChangeState(BossState_Third.HoleAttack);
        //}
        //else if (randomNum == 1)
        //{
        //    ChangeState(BossState_Third.DevourGround);
        //}
        ////else if (randomNum == 2)
        ////{
        ////    ChangeState(BossState_Third.Teleport);
        ////}
        //else if (randomNum == 3)
        //{
        //    ChangeState(BossState_Third.DashAttack);
        //}
        //else
        //{
        //    ChangeState(BossState_Third.SmashAttack);
        //}
        ChangeState(BossState_Third.AttackRandomMove);
    }
    #endregion


    #region 碰撞伤害处理
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryDealCollisionDamage(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TryDealCollisionDamage(collision.gameObject);
        }
    }

    private void TryDealCollisionDamage(GameObject player)
    {
        if (!_canDealCollisionDamage) return;

        // 假设玩家身上有 PlayerHealth 组件
        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamageByEnemy(CollisionDamage);
        }

        if (_currentState_Third == BossState_Third.AttackRandomMove &&_currentStateInstance is BossAttackRandomMoveState_Third moveState)
        {
            moveState.OnBossHitPlayer();
        }

        // 开启碰撞冷却，防止连续触发
        //StartCoroutine(CollisionCooldownRoutine());
    }

    //private IEnumerator CollisionCooldownRoutine()
    //{
    //    _canDealCollisionDamage = false;
    //    yield return new WaitForSeconds(CollisionCooldown);
    //    _canDealCollisionDamage = true;
    //}

    #endregion



    #region Boss -> Player

    public bool IsPlayerInRange(float range)
    {
        return Vector2.Distance(transform.position, Player_Third.position) <= range;
    }

    public void LookAtPlayer()
    {
        Vector2 direction = (Player_Third.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector2.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    #endregion

    #region Player Take Damage
    // 被玩家攻击时调用
    public void TakeDamage(float damage)
    {
        if (_currentState_Third == BossState_Third.Die) return;
        if (_isInvulnerable) return;

        CurrentHealth -= damage;

        //if (Animator != null)
        //{
        //    Animator.SetTrigger("Hurt");
        //}

        StartHurtRoutine();

        if (CurrentHealth <= 0)
        {
            ChangeState(BossState_Third.Die);
        }
    }

    private void StartHurtRoutine()
    {
        if (_hurtCoroutine != null)
        {
            StopCoroutine(_hurtCoroutine);
            _hurtCoroutine = null;
        }

        _hurtCoroutine = StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        _isInvulnerable = true;
        bool prevMove = IsMove;
        IsMove = false; // 受击时停止移动（各状态应注意 IsMove）

        yield return new WaitForSeconds(HurtInvulnerableTime);

        IsMove = prevMove;
        _isInvulnerable = false;
        _hurtCoroutine = null;
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
