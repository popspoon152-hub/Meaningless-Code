using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ContinuousDamage : MonoBehaviour
{
    [Header("自动引用")]
    public BossThirdStateMachine bossStateMachine; // 自动或手动绑定

    private float _timer;
    private float _damageTimer;
    private bool _isActive;

    private Collider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _collider.isTrigger = true;
    }

    private void Start()
    {
        if (bossStateMachine == null)
            bossStateMachine = FindObjectOfType<BossThirdStateMachine>();

        _isActive = true;
        StartCoroutine(DamageZoneRoutine());
    }

    private IEnumerator DamageZoneRoutine()
    {
        _timer = 0f;

        while (_timer < bossStateMachine.ZoneDuration)
        {
            _timer += Time.deltaTime;
            yield return null;
        }

        // 持续时间到期后销毁
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!_isActive) return;

        // 检查目标是否为玩家
        if (other.CompareTag("Player"))
        {
            _damageTimer += Time.deltaTime;

            if (_damageTimer >= bossStateMachine.ZoneDamageInterval)
            {
                _damageTimer = 0f;
                MakeDamage(other.gameObject);
               
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _damageTimer = 0f; // 离开范围后重置计时
        }
    }

    private void MakeDamage(GameObject player)
    {
        //Debug.Log("玩家受到伤害");
        // 假设玩家身上有 PlayerHealth 组件
        var health = player.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            //Debug.Log("玩家受到灼烧伤害");
            health.TakeDamageByEnemy(bossStateMachine.ZoneDamage);
        }
    }
}
