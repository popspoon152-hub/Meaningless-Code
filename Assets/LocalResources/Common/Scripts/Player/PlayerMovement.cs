using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _bodyColl;

    private Rigidbody2D _rb;

    //移动相关
    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    //碰撞体检测相关
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    private void Awake()
    {
        _isFacingRight = true;
        
        _rb = GetComponent<Rigidbody2D>();
    }

    #region Movement

    private void Move(float accleration, float deceleration, Vector2 moveInput)
    {
        try
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

                _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, accleration * Time.deltaTime);
                _rb.velocity = new Vector2(_moveVelocity.x, _moveVelocity.y);
            }

            else if (moveInput == Vector2.zero)
            {
                _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.deltaTime);
                _rb.velocity = new Vector2(_moveVelocity.x, _moveVelocity.y);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"An error occurred:{ex.Message}");
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if(moveInput == null)
        {
            Debug.Log("TurnCheck方法中没传进来值");
        }
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred:{ex.Message}");
        }
    }

    private void Turn(bool turnRight)
    {
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred:{ex.Message}");
        }
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
    }

    private void CollisionChecks()
    {
        IsGrounded();
    }

    #endregion
}
