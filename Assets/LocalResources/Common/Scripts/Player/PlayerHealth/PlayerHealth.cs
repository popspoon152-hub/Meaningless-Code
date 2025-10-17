using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{    
    [Header("Health Num")]
    [Range(50f, 150f)] public readonly float PlayerMaxHealth = 100f;                                     //玩家最大生命值

    [Header("Health Decline")]
    [Range(0.1f, 1f)] public readonly float ExtraHealthDeclineRateByPlayer = 0.8f;                       //被玩家自己打中虚血条占扣血的比例
    [Range(0.1f, 1f)] public readonly float ExtraHealthDeclineRateByEnemy = 0.5f;                        //被敌人打中虚血条占扣血的比例
    [Range(0f, 10f)] public readonly float ExtraHealthDeclineNumDeltaTime = 5f;                          //虚血条每秒下降数值


    private float _currentHealth;                                                                        //当前生命值
    private float _currentExtraHealth;                                                                   //当前虚血值
    private bool _isHealthDeclining;                                                                     //是否正在扣血
    
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

            //向player传_isDead
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