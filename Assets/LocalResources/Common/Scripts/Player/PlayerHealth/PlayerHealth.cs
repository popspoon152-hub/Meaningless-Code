using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Num")]
    [Range(50f, 150f)] public float PlayerMaxHealth = 100f;                                     //玩家最大生命值

    [Header("Health Decline")]
    [Range(0.1f, 1f)] public float ExtraHealthDeclineRateByPlayer = 0.8f;                       //被玩家自己打中虚血条占扣血的比例
    [Range(0.1f, 1f)] public float ExtraHealthDeclineRateByEnemy = 0.5f;                        //被敌人打中虚血条占扣血的比例
    [Range(0f, 10f)] public float ExtraHealthDeclineNumDeltaTime = 5f;                          //虚血条每秒下降数值


    [SerializeField] private float _currentHealth;                                                                        //当前生命值
    [SerializeField] private float _currentExtraHealth;                                                                   //当前虚血值
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


    public static PlayerHealth Ins;

    #region Lifecycle

    private void Awake()
    {
        Ins = this;
    }

    private void Start()
    {
        _isHealthDeclining = false;
        CurrentHealth = PlayerMaxHealth;
        CurrentExtraHealth = PlayerMaxHealth;
    }


    private void Update()
    {
        if (_isHealthDeclining)
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

    #region Take Damage
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
            CurrentHealth = 1;
        }
        else
        {
            CurrentHealth -= damage;
        }

        if (!_isHealthDeclining)
        {
            CurrentExtraHealth -= damage * ExtraHealthDeclineRateByEnemy;
        }
        _isHealthDeclining = true;
    }

    #endregion

    #region Health
    public void HealthUntilExtraHealth()
    {
        CurrentHealth = CurrentExtraHealth;
        CurrentExtraHealth = CurrentHealth;
        _isHealthDeclining = false;
    }

    public void Health(float num)
    {
        CurrentHealth += num;
        if (CurrentHealth > CurrentExtraHealth)
        {
            CurrentExtraHealth = CurrentHealth;
            _isHealthDeclining = false;
        }
    }

    #endregion
}