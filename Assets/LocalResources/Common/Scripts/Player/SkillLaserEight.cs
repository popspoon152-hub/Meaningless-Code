using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class LaserSkillEightWay : MonoBehaviour
{
    [Header("�������")]
    public float laserLength = 5f;
    public float laserWidth = 0.2f;
    public float laserDuration = 0.4f;

    [Header("ǰ��ҡ���")]
    public float preCastTime = 0.25f;
    public float postCastTime = 0.25f;
    public float backStepForce = 6f; // �� ������С���������ʱ��

    [Header("����")]
    public Transform firePoint;
    public GameObject laserPrefab;
    public MonoBehaviour moveScriptToDisable; // PlayMove �ű�
    public PlayerDirection directionController;

    private Rigidbody2D rb;
    private PlayerInputControls PlayerInputControls;
    private bool isCasting = false;

    // ��¼ԭʼԼ��
    private RigidbodyConstraints2D originalConstraints;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        PlayerInputControls = new PlayerInputControls();
        PlayerInputControls.Player.ShootLaser.started += OnShoot;
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

        // ����Y�ᣨ������
        originalConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        rb.velocity = Vector2.zero;

        // ��ֹ�ƶ�
        if (moveScriptToDisable != null) moveScriptToDisable.enabled = false;

        yield return new WaitForSeconds(preCastTime);

        Vector2 dir = GetShootDirection();
        SpawnLaser(dir);

        yield return new WaitForSeconds(laserDuration);

        // �����󳷣�ʩ�ӷ��������
        Vector2 recoil = -dir.normalized * backStepForce;
        rb.AddForce(recoil, ForceMode2D.Impulse);

        // ����Y��
        rb.constraints = originalConstraints;

        yield return new WaitForSeconds(postCastTime);

        // �ָ��ƶ�
        if (moveScriptToDisable != null) moveScriptToDisable.enabled = true;
        isCasting = false;
    }

    private Vector2 GetShootDirection()
    {
        if (directionController == null)
        {
            Debug.LogWarning("[LaserSkill] δ�� PlayerDirectionController��Ĭ�����ҡ�");
            return Vector2.right;
        }

        // ���з������룬�������뷽�򣬷����õ�ǰ����
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
