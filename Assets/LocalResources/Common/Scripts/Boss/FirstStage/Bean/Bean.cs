using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bean : MonoBehaviour
{
    [Range(1, 8)] public int BeanHealth = 5;            //豆子生命值
    [SerializeField] private int _currentHealth;
    private Animator anim;

    private void Start()
    {
        _currentHealth = BeanHealth;
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        //播动画

        if (_currentHealth <= 0)
        {
            StartCoroutine(DestroyBean());

        }
    }

    private IEnumerator DestroyBean()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.enabled = false;

        //播动画
        anim.SetTrigger("Dead");
        yield return new WaitForSeconds(0.8f);

        Destroy(this.gameObject);
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

    public void BeEat()
    {
        Destroy(this.gameObject);
    }
}
