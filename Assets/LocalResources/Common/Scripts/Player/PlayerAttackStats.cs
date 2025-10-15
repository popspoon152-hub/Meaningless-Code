using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Attack")]
public class PlayerAttackStats : ScriptableObject
{
    [Header("Attack Action")]
    public float AttackNumberCount = 3;                                                 //¹¥»÷¶ÎÊý
    [Range(1f, 100f)] float[] ComboDamage = { 10f, 10f, 20f };                          //Ã¿¶Î¹¥»÷µÄÉËº¦

    [Header("Attack Time")]
    [Range(0.1f, 1f)] public float AttackComboWindow = 0.4f;                            //Á¬»÷Ê±¼ä´°¿Ú
    [Range(0.1f, 1f)] public float[] AttackDuration = { 0.3f, 0.4f, 0.5f };             //Ã¿¶Î¹¥»÷µÄ³ÖÐøÊ±¼ä
    [Range(0.1f, 1f)] public float AttackBuffer = 0.1f;                                 //¹¥»÷»º³åÊ±¼ä

    [Header("Attack Postion")]
    public Transform AttackPoint;                                                       //¹¥»÷µã
    [Range(0.1f, 10f)] public float[] AttackRange = { 3f, 4f, 5f };                       //¹¥»÷·¶Î§
    [Range(0f, 3f)] public float[] AttackLittleDash = { 0.1f, 0.1f, 0.1f };             //Ã¿¶Î¹¥»÷µÄÐ¡³å´Ì¾àÀë

    [Header("Attack Layer")]
    public LayerMask EnemyLayer;                                                        //µÐÈË²ã

    [Header("AttackVisualization Tool")]
    public bool ShowAttackRangeArc = false;
}
