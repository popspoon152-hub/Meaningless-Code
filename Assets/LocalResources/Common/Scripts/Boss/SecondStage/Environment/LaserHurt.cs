using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserHurt : MonoBehaviour
{
    [Header("LaserHurt")]
    [Range(0f, 20f)] public float damage;

    //private Transform owner;

    //private void Awake()
    //{
    //    // 找到生成时的父对象（即玩家）
    //    owner = transform.parent;
    //    // 如果你生成后会立即脱离父物体，可在 Start 中解除忽略关系
    //    IgnoreOwnerCollisions();
    //}

    //private void IgnoreOwnerCollisions()
    //{
    //    if (owner == null) return;

    //    Collider2D myCol = GetComponent<Collider2D>();
    //    if (myCol == null) return;

    //    // 忽略与父物体及其所有子物体的碰撞
    //    foreach (var c in owner.GetComponentsInChildren<Collider2D>())
    //    {
    //        Physics2D.IgnoreCollision(myCol, c);
    //    }
    //}
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Boss"))
        {
            if (other.gameObject.GetComponent<BossFirstStateMachine>() != null)
            {
                other.gameObject.GetComponent<BossFirstStateMachine>().TakeDamage(damage);
            }
            if(other.gameObject.GetComponent<BossThirdStateMachine>() != null)
            {
                other.gameObject.GetComponent<BossThirdStateMachine>().TakeDamage(damage);
            }
            
        }
    }

}
