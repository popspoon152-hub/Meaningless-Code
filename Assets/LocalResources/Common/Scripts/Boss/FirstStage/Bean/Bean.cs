using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bean : MonoBehaviour
{
    [Range(1, 8)] public int BeanHealth = 5;            //豆子生命值
    [SerializeField] private int _currentHealth;

    private void Start()
    {
        _currentHealth = BeanHealth;
    }

    private void Update()
    {
        //播动画

        if (_currentHealth <= 0)
        {
            //播动画
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        EnvironmentAndMap environment = FindObjectOfType<EnvironmentAndMap>();
        if (environment != null)
        {
            environment.OnBeanEaten();
        }
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
    }

    //被吃了的还没写
}
