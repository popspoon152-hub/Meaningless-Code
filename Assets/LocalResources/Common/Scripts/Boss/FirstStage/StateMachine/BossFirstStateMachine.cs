using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Enum
public enum BossState
{
    //吃豆环节状态
    EatBeans,            //吃豆
    EatBeansRangedAttack, //吃豆环节远程攻击

    //攻击类型状态
    AttackIdle,          //攻击待机
    RangedAttack,       //远程攻击
    DashAttack,         //冲锋攻击
    AttackRandomMove,   //攻击类型的移动

    //通用类型状态
    Hurt,               //受伤
    Die,                //死亡
    Grow,               //蛇身变长一节
}

#endregion

public class BossFirstStateMachine : MonoBehaviour
{
    #region State Machine Config
    [Header("状态配置")]
    public BossState startingState = BossState.EatBeans;
    public Pathfinding pathfinding;

    [Header("调试信息")]
    [SerializeField] private BossState _currentState;
    [SerializeField] private string _currentStateName;
    #endregion

    #region Boss属性
    [Header("属性配置")]
    [Range(10f, 1000f)] public float MaxHealth = 100f;
    [Range(1f, 20f)] public float EatBeanMoveSpeed = 3f;
    [Range(1f, 20f)] public float AttackMoveSpeed = 5f;
    private float _currentMoveSpeed;

    [Header("蛇的节数配置")]
    [Range(1f, 20f)] public int StartSnakeSegments = 5;
    [Range(1f, 20f)] public int MaxSnakeSegments = 10;
    public Transform SegmentPrefab;
    [HideInInspector] public List<Transform> Segments = new List<Transform>();
    [HideInInspector] public bool IsMove = true;
    [HideInInspector] public int CurrentSegmentNum;

    [Header("受击配置")]
    [Range(0f, 1f)] public float HurtInvulnerableTime = 0.1f;       //受击后的短暂无敌时间(避免重复判定)

    [Header("碰撞伤害")]
    [Range(1f, 50f)] public float HitDamage = 15f;
    private float _playerInvulnerableTime = 0.2f;
    private bool _canHurt = true;

    [HideInInspector] public bool _isAtLeft = false; 
    #endregion



    #region BossEatBeansRangedAttackState_First
    [Header("BossEatBeansRangedAttackState_First的子弹")]
    public GameObject BulletPrefab;
    public Transform FirePoint;
    [Range(1f, 14f)] public float BulletSpeed = 5f;

    [Header("BossEatBeansRangedAttackState_First的Boss出招僵直时间")]
    [Range(0f, 2f)] public float EatBeansRangedAttackInvulnerableTime;
    #endregion

    #region BossGrowState_First
    [Header("BossGrowState_First")]
    [Range(0f, 2f)] public float StateInvulnerableTime = 1f;
    #endregion

    #region BossEatBeansState_First
    [Header("BossEatBeansState_First")]
    [Range(0.5f, 2f)] public float RepathInterval = 0.5f;                  // 定期重算路径，防止障碍/豆子移动导致路径失效
    [Range(0.1f, 3f)] public float EatDistance = 0.8f;                     // 到达此距离视为“吃掉”豆子
    #endregion



    #region BossAttackIdleState_First
    [Header("BossAttackIdleState_First中Boss的位置设定")]
    public Transform IdleLeftTransfrom;
    public Transform IdleRightTransfrom;

    [Header("BossAttackIdleState_First中Boss的僵直时间")]
    [Range(1f, 5f)] public float IdleTime = 2f;
    #endregion

    #region BossRangedAttackState_First
    [Header("BossRangedAttackState_First的子弹")]
    public GameObject BulletPrefab_Attack;
    public GameObject LittleBulletPrefab_Attack;
    public Transform FirePoint_Attack;
    [Range(1f, 20f)] public float BulletSpeed_Attack = 5f;
    [Range(1f, 20f)] public float LittleBulletSpeed_Attack = 7f;

    [Header("BossRangedAttackState_First的Boss出招僵直时间")]
    [Range(0f, 2f)] public float RangedAttackInvulnerableTime;
    #endregion

    #region BossDashAttackState_First
    [Header("冲撞攻击相关")]
    public Transform TelePortLeftPoint;
    public Transform TelePortRightPoint;

    [Header("冲刺的单位长度")]
    public int DashLength = 10;
    public float DashTargetCheckLength = 1f;
    [Range(1f, 20f)] public float DashSpeed = 8f;
    #endregion

    #region BossAttackRandomMoveState_First
    [Header("BossAttackRandomMoveState_First攻击相关")]
    public Transform AttackRandomMoveJumpPostion;      //跳到的位置
    
    public Transform AttackRandomMoveEndPostionLeftPoint;
    public Transform AttackRandomMoveEndPostionRightPoint;

    [Range(1f, 20f)] public float AttackRandomMoveSpeed = 10f;
    #endregion



    #region BossDieState_First
    [Header("BossDieState_First的Boss出招僵直时间")]
    [Range(0f, 2f)] public float DieInvulnerableTime;
    #endregion

    #region private fields
    // 运行时字段
    private bool _isInvulnerable = false;
    private Coroutine _hurtCoroutine;

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
    public Rigidbody2D Rb { get; private set; }

    public float CurrentHealth { get; set; }
    #endregion


    #region LifeCycle
    void Awake()
    {
        // 获取组件引用
        Animator = GetComponent<Animator>();
        Rb = GetComponent<Rigidbody2D>();
        Player = GameObject.FindGameObjectWithTag("Player").transform;

        // 初始化状态字典
        _states = new Dictionary<BossState, IBossStateFirstStage>
        {
            { BossState.EatBeans, new BossEatBeansState_First() },
            { BossState.EatBeansRangedAttack, new BossEatBeansRangedAttackState_First() },
            { BossState.Grow, new BossGrowState_First() },


            { BossState.AttackIdle, new BossAttackIdleState_First() },
            { BossState.RangedAttack, new BossRangedAttackState_First() },
            { BossState.DashAttack, new BossDashAttackState_First() },
            { BossState.AttackRandomMove, new BossAttackRandomMoveState_First() },
            { BossState.Hurt, new BossHurtState() },
            { BossState.Die, new BossDieState_First() },
        };
    }

    void Start()
    {
        Segments.Add(this.transform); // 添加头部作为第一节

        CurrentHealth = MaxHealth;
        CurrentMoveSpeed = EatBeanMoveSpeed;
        CurrentSegmentNum = Segments.Count;

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && _canHurt)
        {
            PlayerHealth.Ins.TakeDamageByEnemy(HitDamage);
            StartCoroutine(HitPlayer());
        }
    }

    private IEnumerator HitPlayer()
    {
        _canHurt = false;
        yield return new WaitForSeconds(_playerInvulnerableTime);
        _canHurt = true;
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

    #region Boss Segments Move

    public void SegmentsMove()
    {
        if (Segments == null || Segments.Count < 2) return;

        float step = CurrentMoveSpeed * Time.fixedDeltaTime;

        for (int i = Segments.Count - 1; i > 0; i--)
        {
            Transform currentSegment = Segments[i];
            Transform previousSegment = Segments[i - 1];

            Vector2 targetPosition = previousSegment.position;
            targetPosition.y = currentSegment.position.y; // 保持y轴位置不变

            Rigidbody2D rb = currentSegment.GetComponent<Rigidbody2D>();
            Vector2 newPos = Vector3.MoveTowards(currentSegment.position, targetPosition, step);
            if (rb != null)
            {
                rb.MovePosition(newPos);
            }
            else
            {
                currentSegment.position = newPos;
            }

            // 保持水平旋转
            Vector2 direction = previousSegment.position - currentSegment.position;
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

    #region Boss -> Player

    public bool IsPlayerInRange(float range)
    {
        return Vector2.Distance(transform.position, Player.position) <= range;
    }

    public void LookAtPlayer()
    {
        Vector2 direction = (Player.position - transform.position).normalized;
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
        if (_currentState == BossState.Die) return;
        if(_isInvulnerable) return;

        CurrentHealth -= damage;

        //if (Animator != null)
        //{
        //    Animator.SetTrigger("Hurt");
        //}

        StartHurtRoutine();

        if (CurrentHealth <= 0)
        {
            ChangeState(BossState.Die);
        }
    }

    private void StartHurtRoutine()
    {
        if(_hurtCoroutine != null)
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

    #region Attack State Choose
    public void AttackStateChoose()
    {
        //分为远程攻击，冲锋攻击，攻击类型的随机移动三种
        int randonNum = UnityEngine.Random.Range(0, 4);
        if (randonNum == 0)
        {
            ChangeState(BossState.RangedAttack);
        }
        else if (randonNum == 1)
        {
            ChangeState(BossState.DashAttack);
        }
        else
        {
            ChangeState(BossState.AttackRandomMove);
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
