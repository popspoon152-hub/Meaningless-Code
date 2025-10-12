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
        if (moveInput == Vector2.zero) 
        {
            Vector2 targetVelocity = Vector2.zero;
            if (InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
            }
        }
    } 

    #endregion
}
