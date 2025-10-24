using UnityEngine;

public class DashTrailDamageZone : MonoBehaviour
{
    public float DamagePerTick = 5f;
    public float TickInterval = 0.5f;
    public float LifeTime = 2.5f;

    private float _tickTimer;

    void Start()
    {
        Destroy(gameObject, LifeTime);
    }

    private void Update()
    {
        _tickTimer += Time.deltaTime;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_tickTimer >= TickInterval)
        {
            _tickTimer = 0f;

            if (other.CompareTag("Player"))
            {
                var health = other.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamageByEnemy(DamagePerTick);
                }
            }
        }
    }
}
