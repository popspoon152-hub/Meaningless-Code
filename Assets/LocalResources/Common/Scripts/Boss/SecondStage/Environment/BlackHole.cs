using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BlackHole : MonoBehaviour
{
    [Header("黑洞参数（由预制体设定）")]
    public float Duration = 5f;             // 黑洞持续时间
    public float TickInterval = 0.5f;       // 对受影响对象造成伤害的间隔
    public float DamagePerTick = 5f;        // 每次伤害
    public float PullStrength = 5f;         // 吸力强度
    public float Radius = 2f;               // 吸力范围半径

    [Header("影响与排除设置")]
    //public GameObject Owner;                // 生成者（不受影响）
    public LayerMask AffectedLayers;        //  只有这些层的对象才会被吸力影响

    private float _lifeTimer;
    private float _tickTimer;
    private HashSet<Rigidbody2D> _insideBodies = new HashSet<Rigidbody2D>();
    private HashSet<Collider2D> _insideColliders = new HashSet<Collider2D>();
    private Collider2D _collider2d;

    void Awake()
    {
        _collider2d = GetComponent<Collider2D>();
        _collider2d.isTrigger = true;

        // 可同步半径（如果使用 CircleCollider2D）
        CircleCollider2D circle = _collider2d as CircleCollider2D;
        if (circle != null)
        {
            circle.radius = Radius;
        }
    }

    void Start()
    {
        _lifeTimer = 0f;
        _tickTimer = 0f;
    }

    void Update()
    {
        _lifeTimer += Time.deltaTime;
        _tickTimer += Time.deltaTime;

        // 定时对范围内对象造成伤害
        if (_tickTimer >= TickInterval)
        {
            _tickTimer = 0f;
            foreach (var col in _insideColliders)
            {
                if (col == null) continue;
                if (((1 << col.gameObject.layer) & AffectedLayers) == 0) continue; // 只对指定层有效

                if (col.CompareTag("Player"))
                {
                    var health = col.GetComponent<PlayerHealth>();
                    if (health != null)
                    {
                        health.TakeDamageByEnemy(DamagePerTick);
                    }
                }
            }
        }

        // 持续时间到达，销毁黑洞
        if (_lifeTimer >= Duration)
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        // 对范围内刚体施加吸力
        Vector2 center = transform.position;
        foreach (var rb in _insideBodies)
        {
            if (rb == null) continue;
            GameObject obj = rb.gameObject;

            // 只影响指定 Layer
            if (((1 << obj.layer) & AffectedLayers) == 0)
                continue;

            //// 忽略生成者自身
            //if (Owner != null && obj == Owner)
            //    continue;

            Vector2 dir = (center - rb.position);
            float dist = dir.magnitude;
            if (dist <= 0.01f) continue;
            dir.Normalize();

            rb.AddForce(dir * PullStrength, ForceMode2D.Force);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 层过滤：如果不是受影响层，不添加进列表
        if (((1 << other.gameObject.layer) & AffectedLayers) == 0)
            return;

        if (!_insideColliders.Contains(other))
            _insideColliders.Add(other);

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null && !_insideBodies.Contains(rb))
            _insideBodies.Add(rb);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_insideColliders.Contains(other))
            _insideColliders.Remove(other);

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null && _insideBodies.Contains(rb))
            _insideBodies.Remove(rb);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0f, 0.8f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}
