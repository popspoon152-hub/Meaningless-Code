using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Attack")]
public class PlayerAttackStats : ScriptableObject
{
    [Header("Attack Action")]
    public float AttackNumberCount = 3;                                                 //攻击段数
    [Range(1f, 100f)] public float[] ComboDamage = { 10f, 10f, 20f };                          //每段攻击的伤害

    [Header("Attack Time")]
    [Range(0.1f, 1f)] public float AttackComboWindow = 0.4f;                            //连击时间窗口
    [Range(0.1f, 1f)] public float[] AttackDuration = { 0.3f, 0.4f, 0.5f };             //每段攻击的持续时间
    [Range(0.1f, 1f)] public float AttackBuffer = 0.1f;                                 //攻击缓冲时间

    [Header("Attack Postion")]
    [Range(0.1f, 30f)] public float[] AttackRange = { 3f, 4f, 5f };                       //攻击范围
    [Range(0f, 3f)] public float[] AttackLittleDash = { 0.1f, 0.1f, 0.1f };             //每段攻击的小冲刺距离

    [Header("Attack Layer")]
    public LayerMask EnemyLayer;                                                        //敌人层
    public LayerMask BeanLayer;                                                     //障碍层
    public LayerMask GroundLayer;                                                   //地面层
    public LayerMask RewardLayer;                                                   //奖励层

    [Header("AttackVisualization Tool")]
    public bool ShowAttackRangeArc = false;

    [Header("攻击烧血数值")]
    [Range(1f, 50f)] public float AttackHurtPlayerNum = 10f;                            //自己烧血 
    public float PlayerAttackUpPerHealth;
}
