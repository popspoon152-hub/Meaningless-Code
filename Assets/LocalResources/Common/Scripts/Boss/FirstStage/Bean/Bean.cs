using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bean : MonoBehaviour
{
    [Range(1, 8)] public int BeanHealth = 5;            //豆子生命值


    private void Update()
    {
        //播动画

        if (BeanHealth <= 0)
        {
            //播动画
            Destroy(this.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        BeanHealth -= damage;
    }

    //被吃了的还没写
}
