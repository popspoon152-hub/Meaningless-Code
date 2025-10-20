using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstStageBullet : MonoBehaviour
{
    public float Damage = 10f;                                  //被子弹打的伤害 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth.Ins.TakeDamageByEnemy(Damage);
        }

        Destroy(gameObject);
    }
}
