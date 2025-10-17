using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{    
    [Header("Health Num")]
    [Range(50f, 150f)] public readonly float PlayerMaxHealth = 100f;                                     //����������ֵ

    [Header("Health Decline")]
    [Range(0.1f, 1f)] public readonly float ExtraHealthDeclineRateByPlayer = 0.8f;                       //������Լ�������Ѫ��ռ��Ѫ�ı���
    [Range(0.1f, 1f)] public readonly float ExtraHealthDeclineRateByEnemy = 0.5f;                        //�����˴�����Ѫ��ռ��Ѫ�ı���
    [Range(0f, 10f)] public readonly float ExtraHealthDeclineNumDeltaTime = 5f;                          //��Ѫ��ÿ���½���ֵ


    private float _currentHealth;                                                                        //��ǰ����ֵ
    private float _currentExtraHealth;                                                                   //��ǰ��Ѫֵ
    private bool _isHealthDeclining;                                                                     //�Ƿ����ڿ�Ѫ
    
    public float CurrentHealth
    {
        get {  return _currentHealth; }
        set 
        {
            if (value <= PlayerMaxHealth)
            {
                _currentHealth = value;
            }
            else
            {
                _currentHealth = PlayerMaxHealth;
            }
        }
    }

    public float CurrentExtraHealth
    {
        get { return _currentExtraHealth; }
        set 
        {
            if (value <= CurrentHealth)
            {
                _currentExtraHealth = value;
            }
            else
            {
                _currentExtraHealth = CurrentHealth;
            }
        }
    }


    private static PlayerHealth Ins;

    #region Lifecycle

    private void Awake()
    {
        if (Ins == null)
        {
            Ins = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _isHealthDeclining = false; 
    }


    private void Update()
    {
        if(_isHealthDeclining)
        {
            CurrentExtraHealth -= ExtraHealthDeclineNumDeltaTime * Time.deltaTime;
            if (CurrentExtraHealth <= CurrentHealth)
            {
                CurrentExtraHealth = CurrentHealth;
                _isHealthDeclining = false;
            }
        }
    }
    #endregion


    public void TakeDamageByEnemy(float damage)
    {
        if (damage <= 0) return;

        CurrentHealth -= damage;

        if (CurrentHealth < 0)
        {
            CurrentHealth = 0;

            //��player��_isDead
        }

        if (!_isHealthDeclining)
        {
            CurrentExtraHealth -= damage * ExtraHealthDeclineRateByEnemy;
        }


        _isHealthDeclining = true;
    }

    public void TakeDamageByPlayer(float damage)
    {
        if (damage <= 0) return;

        if(damage >= CurrentHealth)
        {
            damage = CurrentHealth - 1;
            CurrentHealth -= damage;
        }
        else
        {
            CurrentHealth -= damage;

            if (!_isHealthDeclining)
            {
                CurrentExtraHealth -= damage * ExtraHealthDeclineRateByEnemy;
            }
            _isHealthDeclining = true;
        }
    }
}