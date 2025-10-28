using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Shockwave : MonoBehaviour
{
    private Vector2 _moveDir;
    private float _moveSpeed;
    private float _damage;
    private float _lifeTime;
    private float _damageInterval = 0.5f;

    private BossThirdStateMachine _boss;
    private Collider2D _collider;
    private bool _isActive = false;
    private bool _isHurtable = true;

    private LayerMask _playerLayer;
    private Coroutine _damageRoutine;

    public void Initialize(Vector2 dir)
    {
        _moveDir = dir.normalized;
        _isActive = true;

        // 可调整初始化动画或粒子特效
        // e.g. Animator.SetTrigger("Activate");
    }

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        _collider.isTrigger = true;

        // 自动查找Boss引用
        _boss = FindObjectOfType<BossThirdStateMachine>();
        if (_boss == null)
        {
            Debug.LogError("[Shockwave] Could not find BossThirdStateMachine in scene!");
            Destroy(gameObject);
            return;
        }

        // 从状态机读取参数
        _moveSpeed = _boss.SmashShockwaveSpeed;
        _damage = _boss.SmashShockwaveDamage;
        _lifeTime = _boss.SmashShockwaveLifetime;
        _playerLayer = _boss.PlayerLayerMask;

        // 启动生命周期计时
        Destroy(gameObject, _lifeTime);
    }

    private void Update()
    {
        if (!_isActive) return;
        transform.Translate(_moveDir * _moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _playerLayer) != 0 || _isHurtable == true)
        {
            MakeDamage(collision.gameObject);
            //// 玩家受伤逻辑
            //var player = collision.GetComponent<PlayerHealth>();
            //if (player != null)
            //{
            //    player.TakeDamageByEnemy(_damage);
            //}
            _isHurtable = false;
        }
    }

    private void MakeDamage(GameObject player)
    {
        //Debug.Log("玩家受到伤害");
        // 假设玩家身上有 PlayerHealth 组件
        var health = player.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            Debug.Log("玩家受到冲击波伤害");
            health.TakeDamageByEnemy(_damage);
        }
    }

}
