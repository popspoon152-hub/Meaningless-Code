using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class LaserSkillEightWay : MonoBehaviour
{
    [Header("激光参数")]
    public float laserLength = 5f;
    public float laserWidth = 0.2f;
    public float laserDuration = 0.4f;

    [Header("前后摇与后撤")]
    public float preCastTime = 0.25f;
    public float postCastTime = 0.25f;
    public float backStepForce = 6f; // ← 用力大小代替距离与时间

    [Header("引用")]
    public Transform firePoint;
    public GameObject laserPrefab;
    public MonoBehaviour moveScriptToDisable; // PlayMove 脚本
    public PlayerDirection directionController;

    private Rigidbody2D rb;
    private PlayerInputControls PlayerInputControls;
    private bool isCasting = false;

    // 记录原始约束
    private RigidbodyConstraints2D originalConstraints;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        PlayerInputControls = new PlayerInputControls();
        PlayerInputControls.Player.Shoot.started += OnShoot;
    }

    private void OnEnable() => PlayerInputControls.Enable();
    private void OnDisable() => PlayerInputControls.Disable();

    private void OnShoot(InputAction.CallbackContext ctx)
    {
        if (!isCasting)
        {
            StartCoroutine(LaserSequenceCoroutine());
        }
    }

    private IEnumerator LaserSequenceCoroutine()
    {
        isCasting = true;

        // 锁定Y轴（悬浮）
        originalConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        rb.velocity = Vector2.zero;

        // 禁止移动
        if (moveScriptToDisable != null) moveScriptToDisable.enabled = false;

        yield return new WaitForSeconds(preCastTime);

        Vector2 dir = GetShootDirection();
        SpawnLaser(dir);

        yield return new WaitForSeconds(laserDuration);

        // 立即后撤：施加反方向的力
        Vector2 recoil = -dir.normalized * backStepForce;
        rb.AddForce(recoil, ForceMode2D.Impulse);

        // 解锁Y轴
        rb.constraints = originalConstraints;

        yield return new WaitForSeconds(postCastTime);

        // 恢复移动
        if (moveScriptToDisable != null) moveScriptToDisable.enabled = true;
        isCasting = false;
    }

    private Vector2 GetShootDirection()
    {
        if (directionController == null)
        {
            Debug.LogWarning("[LaserSkill] 未绑定 PlayerDirectionController。默认向右。");
            return Vector2.right;
        }

        // 若有方向输入，则用输入方向，否则用当前朝向
        if (directionController.hasInput)
            return directionController.directionState.normalized;
        else
            return directionController.facingDirection == Vector2.zero ? Vector2.right : directionController.facingDirection;
    }

    private void SpawnLaser(Vector2 direction)
    {
        if (laserPrefab == null || firePoint == null) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject laser = Instantiate(laserPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));

        Vector3 scale = laser.transform.localScale;
        scale.x = laserLength;
        scale.y = laserWidth;
        laser.transform.localScale = scale;

        Destroy(laser, laserDuration);
    }
}
