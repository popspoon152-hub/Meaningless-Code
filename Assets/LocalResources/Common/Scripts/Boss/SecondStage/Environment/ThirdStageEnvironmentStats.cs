using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ThirdStage/EnvironmentStats")]
public class ThirdStageEnvironmentStats : ScriptableObject
{
    [Header("引用")]
    public GameObject StonesPrefabs;                                                                    //石头预制体

    [Header("数值设定")]
    [Range(1f, 10f)] public float StoneMinSize = 1f;                                                           //生成的石头的最小尺寸
    [Range(1f, 10f)] public float StoneMaxSize = 3f;                                                           //生成的石头的最大尺寸

    public Vector2 InstantiateLeftPosition;                                                             //石头生成的左边界位置
    public Vector2 InstantiateRightPosition;                                                            //石头生成的右边界位置

    [Range(1f, 5f)] public int StoneMinInstantiateNum = 1;                                                  //每次生成石头的最小数量   
    [Range(1f, 5f)] public int StoneMaxInstantiateNum = 3;                                                  //每次生成石头的最大数量

    [Header("Time")]
    [Range(0f, 15f)] public float StoneMinInstantiateInterval = 7f;                                           //每波石头最小生成间隔时间
    [Range(0f, 15f)] public float StoneMaxInstantiateInterval = 10f;                                           //每波石头最大生成间隔时间

    [Range(0f,1f)] public float EachStoneMinInstantiateDelay = 0.3f;                                            //每个石头生成的最小间隔时间
    [Range(0f,1f)] public float EachStoneMaxInstantiateDelay = 0.8f;                                            //每个石头生成的最大间隔时间
}
