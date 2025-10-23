using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstStageBullet : MonoBehaviour
{
    public float Damage = 10f;                                  //被子弹打的伤害 

    public float AliveTime = 15f;                                //子弹存活时间

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth.Ins.TakeDamageByEnemy(Damage);
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Boss"))
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        AliveTime -= Time.deltaTime;
        if (AliveTime < 0)
        {
            Destroy(gameObject);
        }
    }
}
