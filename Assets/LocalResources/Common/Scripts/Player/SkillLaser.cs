using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class ShootLaser : MonoBehaviour
{
    [Header("激光参数")]
    public float laserLength = 5f;//其实是缩放倍率
    public float laserWidth = 0.2f;
    public float laserDuration = 0.4f;

    [Header("时序参数")]
    public float preCastTime = 0.25f;//射前
    //射
    public float postCastTime = 0.25f;//射后
    public float backStepDistance = 0.5f;//后撤距离
    public float backStepTime = 0.1f;//后撤时间

    [Header("引用")]
    public Transform firePoint;
    public GameObject laserPrefab;

    private Rigidbody2D rb;
    private PlayerInputControls PlayerInputControls;
    private MonoBehaviour moveScript; // 引用移动脚本
    private bool isCasting = false;

    // 后撤控制
    private Vector2 backStartPos;
    private Vector2 backTargetPos;
    private float backStartTime;

    // 记录原始约束
    private RigidbodyConstraints2D originalConstraints;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        PlayerInputControls = new PlayerInputControls();
        PlayerInputControls.Player.ShootLaser.started += OnShootLaser;
        moveScript = GetComponent<PlayerMovement>(); // 自动检测你的移动脚本
    }

    private void OnEnable() => PlayerInputControls.Enable();
    private void OnDisable() => PlayerInputControls.Disable();

    private void OnShootLaser(InputAction.CallbackContext ctx)
    {
        if (!isCasting)
            StartCoroutine(CastLaserRoutine());
    }

    private System.Collections.IEnumerator CastLaserRoutine()
    {
        isCasting = true;
        DisableMovement(); // === 禁止移动 ===

        //  锁定Y轴（悬浮）
        originalConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        rb.velocity = Vector2.zero;

        // 前摇
        yield return new WaitForSeconds(preCastTime);

        // 发射激光（朝向与角色相同）
        SpawnLaser();

        // 激光持续期间
        yield return new WaitForSeconds(laserDuration);

        // 平滑后撤
        yield return StartCoroutine(BackStepSmooth());

        // 解除锁定
        rb.constraints = originalConstraints;

        // 后摇
        yield return new WaitForSeconds(postCastTime);

        EnableMovement(); // === 解除移动锁定 ===
        isCasting = false;
    }

    private void SpawnLaser()
    {
        if (!laserPrefab || !firePoint) return;

        float facing = Mathf.Sign(transform.localScale.x != 0 ? transform.localScale.x : 1f);
        GameObject laser = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);
        laser.transform.right = transform.right * facing; // 激光方向与角色朝向一致
        laser.transform.localScale = new Vector3(laserLength, laserWidth, 1f);
        Destroy(laser, laserDuration);
    }

    private System.Collections.IEnumerator BackStepSmooth()
    {
        float facing = Mathf.Sign(transform.localScale.x != 0 ? transform.localScale.x : 1f);
        backStartPos = rb.position;
        backTargetPos = backStartPos - new Vector2(facing * backStepDistance, 0f);
        backStartTime = Time.time;

        while (Time.time - backStartTime < backStepTime)
        {
            float t = (Time.time - backStartTime) / backStepTime;
            Vector2 newPos = Vector2.Lerp(backStartPos, backTargetPos, t);
            rb.MovePosition(newPos);
            yield return null;
        }
        rb.MovePosition(backTargetPos);
    }

    private void DisableMovement()
    {
        if (moveScript != null)
            moveScript.enabled = false;
        rb.velocity = Vector2.zero;
    }

    private void EnableMovement()
    {
        if (moveScript != null)
            moveScript.enabled = true;
    }
}
