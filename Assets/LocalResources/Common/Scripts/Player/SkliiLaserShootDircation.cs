using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerDirection : MonoBehaviour
{
    [Header("当前方向状态")]
    public Vector2 directionState;   // 当前方向输入
    public bool hasInput;            // 是否正在按方向键
    public Vector2 facingDirection;  // 当前朝向（供其他脚本使用）

    private SpriteRenderer sr;
    private PlayerInputControls PlayerInputControls;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        PlayerInputControls = new PlayerInputControls();
        PlayerInputControls.Player.Move.performed += OnMove;
        PlayerInputControls.Player.Move.canceled += OnMoveCanceled;
    }

    private void OnEnable()
    {
        PlayerInputControls.Enable();
    }

    private void OnDisable()
    {
        PlayerInputControls.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        directionState = ctx.ReadValue<Vector2>();
        hasInput = directionState != Vector2.zero;

        //更新当前朝向（最后一个有效方向）
        if (hasInput)
        {
            facingDirection = directionState.normalized;
            //if (facingDirection.x > 0)
            //    sr.flipX = false;
            //else if (facingDirection.x < 0)
            //    sr.flipX = true;
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        hasInput = false;
        directionState = Vector2.zero;
        // 不重置 facingDirection，保持上一次朝向
    }
}
