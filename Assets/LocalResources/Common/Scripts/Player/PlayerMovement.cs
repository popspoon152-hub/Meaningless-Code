using Sirenix.OdinInspector.Editor.Examples;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static UnityEngine.Rendering.DebugUI;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MoveStats;
    public PlayerAttackStats AttackStats;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _bodyColl;
    [SerializeField] private Collider2D _attackColl;
    [SerializeField] private Animator Anim;
    private Rigidbody2D _rb;

    //移动相关
    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    //碰撞体检测相关
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    //跳跃相关
    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    //跳跃顶点相关
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    //jump buffer
    private float _jumpBufferTimer;
    private bool _jumpReleaseDuringBuffer;

    //coyote time
    private float _coyoteTimer;

    //冲刺相关
    private bool _isDashing = false;
    private float _dashTime;
    private Vector2 _dashDirection;
    private float _dashSpeed;
    private float _dashCooldownTimer;

    //攻击相关
    private float _currentCombo;
    private float _lastAttackTime;
    private bool _isAttacking = false;
    private Coroutine _attackCoroutine;


    #region LifeCycle
    private void Awake()
    {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
        MoveStats.CalculateValues();
        _attackColl.enabled = false;
    }

    private void Update()
    {
        CountTimers();
        JumpChecks();
        DashChecks();
        AttackCheck();
        CheckComboReset();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        if (_isDashing)
        {
            Dash();
        }
        else
        {
            if (_isGrounded)
            {
                Move(MoveStats.GroundAccleration, MoveStats.GroundDeceleration, InputManager.Movement);
            }
            else
            {
                Move(MoveStats.AirAccleration, MoveStats.AirDeceleration, InputManager.Movement);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (MoveStats != null && Application.isPlaying)
        {
            if (MoveStats.ShowWalkJumpArc)
            {
                DrawJumpArc(MoveStats.MaxWalkSpeed, Color.white);
            }
            if (MoveStats.ShowRunJumpArc)
            {
                DrawJumpArc(MoveStats.MaxRunSpeed, Color.red);
            }
            if(MoveStats.ShowDashJumpArc)
            {
                DrawDashArc();
            }
            if (AttackStats.ShowAttackRangeArc)
            {
                DrawAttackArc();
            }
        }
    }
    #endregion

    #region Movement

    private void Move(float accleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            if (InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
            }
            else { targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed; }

            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, accleration * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
        }

        else if (moveInput == Vector2.zero)
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if (!_isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    #endregion

    #region Jump

    private void JumpChecks()
    {
        //按空格
        if (InputManager.JumpWasPressed)
        {
            _jumpBufferTimer = MoveStats.JumpBufferTime;
            _jumpReleaseDuringBuffer = false;
        }

        //松空格
        if (InputManager.JumpWasReleased)
        {
            if(_jumpBufferTimer > 0f)
            {
                _jumpReleaseDuringBuffer = true; 
            }

            if(_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MoveStats.TimeForUpwardCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        //Initiate Jump Coyote Time
        if(_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (_jumpReleaseDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }

        //Double Jump
        else if(_jumpBufferTimer > 0f && _isJumping && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }

        //Air Jump After Coyote Time Lapsed
        else if(_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1)
        {
            InitiateJump(1);
            _isFastFalling = false;
        }

        //Landed
        if((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int numberOfJumpsUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = MoveStats.IntialJumpVelocity;
    }

    private void Jump()
    {
        //启用重力
        if (_isJumping)
        {
            //检测头部碰撞
            if (_bumpedHead)
            {
                _isFastFalling = true;
            }

            //上升重力
            if (VerticalVelocity >= 0f)
            {
                //顶点控制
                _apexPoint = Mathf.InverseLerp(MoveStats.IntialJumpVelocity, 0f, VerticalVelocity);

                if (_apexPoint > MoveStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < MoveStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }

                //上升重力 并且 不满足上面的顶点控制
                else
                {
                    VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }

            //下降重力
            else if (!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }

            else if (VerticalVelocity < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        //Jump Cut
        if (_isFastFalling)
        {
            if (_fastFallTime >= MoveStats.TimeForUpwardCancel)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < MoveStats.TimeForUpwardCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / MoveStats.TimeForUpwardCancel));
            }

            _fastFallTime += Time.fixedDeltaTime;
        }

        //正常下落时的重力
        if (!_isGrounded && !_isJumping)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }

            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }

        //快速下落速度
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);

        _rb.velocity = new Vector2(_rb.velocity.x, VerticalVelocity);
    }

    #endregion

    #region Collision Check

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, MoveStats.GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);
        if(_groundHit.collider != null)
        {
            _isGrounded = true;
        }
        else { _isGrounded= false; }

        #region Debug Visualization

        if (MoveStats.DebugShowIsGroundedBox)
        {
            Color rayColor;
            if( _isGrounded)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - MoveStats.GroundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
        }

        #endregion
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_bodyColl.bounds.center.x, _bodyColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_bodyColl.bounds.size.x, MoveStats.HeadDetectionRayLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);
        if (_headHit.collider != null)
        {
            _bumpedHead = true;
        }
        else { _bumpedHead = false; }

        #region Debug Visualization

        if (MoveStats.DebugShowHeadBumpBox)
        {
            float headWidth = MoveStats.HeadWidth;

            Color rayColor;
            if (_bumpedHead)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + MoveStats.HeadDetectionRayLength), Vector2.right * boxCastSize.x * headWidth, rayColor);
        }

        #endregion
    }

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        //jump
        _jumpBufferTimer -= Time.deltaTime;

        if (!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else { _coyoteTimer = MoveStats.JumpCoyoteTime; }

    }

    #endregion

    #region Dash

    private void DashChecks()
    {
        if (_dashCooldownTimer > 0f)
            _dashCooldownTimer -= Time.deltaTime;

        if (InputManager.DashWasPressed && !_isDashing && _dashCooldownTimer <= 0f)
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        _isDashing = true;
        _dashTime = 0f;
        _dashDirection = _isFacingRight ? Vector2.right : Vector2.left;
        _dashSpeed = MoveStats.MaxDashLength / MoveStats.DashDuration; // 速度=距离/时间
        _rb.velocity = new Vector2(_dashDirection.x * _dashSpeed, 0f);
    }

    private void Dash()
    {
        _dashTime += Time.fixedDeltaTime;
        _rb.velocity = new Vector2(_dashDirection.x * _dashSpeed, 0f);

        if (_dashTime >= MoveStats.DashDuration)
        {
            _isDashing = false;
            _rb.velocity = Vector2.zero;
        }
    }

    #endregion

    #region Attack

    private void AttackCheck()
    {
        //按攻击键
        if (InputManager.AttackWasPressed && !_isAttacking)
        {
            StartAttack();
            Debug.Log("Attack");
        }
        else if(_isAttacking && InputManager.AttackWasPressed && CanCombo())
        {
            ContinueCombo();
            Debug.Log("Combo");
        }
    }

    private void StartAttack()
    {
        _isAttacking = true;
        _currentCombo = 1;
        _lastAttackTime = Time.time;

        //设置动画trigger



        if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
        _attackCoroutine = StartCoroutine(AttackCoroutine());
    }


    private void ContinueCombo()
    {
        _currentCombo++;
        _lastAttackTime = Time.time;

        //停止携程
        if(_attackCoroutine != null) StopCoroutine(_attackCoroutine);

        //更新动画



        //开始新携程
        _attackCoroutine = StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        float attackRange;
        if (_currentCombo == 0)
        {
            attackRange = AttackStats.AttackRange[0];
        }
        else
        {
            attackRange = AttackStats.AttackRange[(int)_currentCombo - 1];
        }
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(AttackStats.AttackPoint.position, attackRange, AttackStats.EnemyLayer);

        yield return new WaitForSeconds(AttackStats.AttackDuration[(int)_currentCombo - 1]);

        foreach (var enemy in hitEnemies)
        {
            //触发扣血
        }
    }

    private bool CanCombo()
    {
        return _currentCombo < AttackStats.AttackNumberCount && Time.time - _lastAttackTime <= AttackStats.AttackComboWindow;
    }

    private void CheckComboReset()
    {
        if (_isAttacking && Time.time - _lastAttackTime > AttackStats.AttackComboWindow)
        {
            ResetAttack();
        }
    }

    private void ResetAttack()
    {
        _isAttacking = false;
        _currentCombo = 0;
        //animator.SetInteger(comboIndex, 0);
    }

    #endregion

    #region Gizmos
    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        Vector2 startPostion = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 previousPostion = startPostion;
        float speed = 0f;

        if (MoveStats.DrawRight)
        {
            speed = moveSpeed;
        }
        else { speed = -moveSpeed; }

        Vector2 velocity = new Vector2(speed, MoveStats.IntialJumpVelocity);

        Gizmos.color = gizmoColor;

        float timeStep = 2 * MoveStats.TimeTillJumpApex / MoveStats.ArcSolution;

        for (int i = 0; i < MoveStats.VisualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacement;
            Vector2 drawPoint;

            if (simulationTime < MoveStats.TimeTillJumpApex)
            {
                displacement = velocity * simulationTime + 0.5f * new Vector2(0f, MoveStats.Gravity) * Mathf.Pow(simulationTime, 2);
            }
            else if (simulationTime < MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime)
            {
                float apexTime = simulationTime - MoveStats.TimeTillJumpApex;
                displacement = velocity * MoveStats.TimeTillJumpApex + 0.5f * new Vector2(0f, MoveStats.Gravity) * Mathf.Pow(MoveStats.TimeTillJumpApex, 2)
                    + new Vector2(velocity.x * apexTime, 0f);
            }
            else
            {
                float descendTime = simulationTime - (MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime);
                displacement = velocity * MoveStats.TimeTillJumpApex + 0.5f * new Vector2(0f, MoveStats.Gravity) * Mathf.Pow(MoveStats.TimeTillJumpApex, 2)
                    + new Vector2(speed * MoveStats.ApexHangTime, 0f)
                    + new Vector2(speed * descendTime, 0f) + 0.5f * new Vector2(0f, MoveStats.Gravity) * Mathf.Pow(descendTime, 2);
            }

            drawPoint = startPostion + displacement;

            if (MoveStats.StopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPostion, drawPoint - previousPostion, Vector2.Distance(previousPostion, drawPoint), MoveStats.GroundLayer);
                if (hit.collider != null)
                {
                    Gizmos.DrawLine(previousPostion, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previousPostion, drawPoint);
            previousPostion = drawPoint;
        }
    }

    private void DrawDashArc()
    {
        if (_isFacingRight)
        {
            Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + MoveStats.MaxDashLength, transform.position.y));
        }
        else
        {
            Gizmos.DrawLine(new Vector2(transform.position.x - MoveStats.MaxDashLength, transform.position.y), transform.position);
        }
    }

    private void DrawAttackArc()
    {
        float attackRange;
        if (_currentCombo == 0)
        {
            attackRange = AttackStats.AttackRange[0];
        }
        else
        {
            attackRange = AttackStats.AttackRange[(int)_currentCombo - 1];
        }

        Gizmos.DrawWireSphere(AttackStats.AttackPoint.position, attackRange);
    }
    #endregion
}