using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bean : MonoBehaviour
{
    [Range(1, 8)] public int BeanHealth = 5;            //��������ֵ


    private void Update()
    {
        //������

        if (BeanHealth <= 0)
        {
            //������
            Destroy(this.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        BeanHealth -= damage;
    }
}
