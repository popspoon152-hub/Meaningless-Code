using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public PlayerMovement PlayerMovement;
    public PlayerSlider Slider;
 
    [Header("Health Num")]
    [Range(50f, 150f)] public float PlayerMaxHealth = 100f;                                     //玩家最大生命值

    [Header("Health Decline")]
    [Range(0f, 10f)] public float ExtraHealthDeclineNumDeltaTime = 5f;                          //虚血条每秒下降数值


    public float CurrentHealth;                                                                        //当前生命值
    public float CurrentExtraHealth;                                                                   //当前虚血值
    private bool _isHealthDeclining;                                                                     //是否正在扣血

    private bool _playerIsDead = false;


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
        _playerIsDead = false;
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

        if (_playerIsDead)
        {
            PlayerMovement.PlayerIsDead = true;
        }

        Slider.UpdateHealth(CurrentHealth, CurrentExtraHealth, PlayerMaxHealth);
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
            _playerIsDead = true;
        }

        _isHealthDeclining = true;

        EventCenter.Ins.Dispatch(EPlayerHurt.on);
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

        _isHealthDeclining = true;

        EventCenter.Ins.Dispatch(EPlayerHurt.on);
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