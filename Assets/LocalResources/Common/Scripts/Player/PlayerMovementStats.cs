using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement")] 
public class PlayerMovementStats : ScriptableObject
{
    /// <summary>
    /// 参数仅供参考
    /// </summary> 

    [Header("Walk")]
    [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;                        //最大速度
    [Range(0.25f, 50f)] public float GroundAccleration = 5f;                    //地面加速度
    [Range(0.25f, 50f)] public float GroundDeceleration = 20f;                  //地面减速度               
    [Range(0.25f, 50f)] public float AirAccleration = 20f;                      //空气加速度
    [Range(0.25f, 50f)] public float AirDeceleration = 20f;                     //空气减速度

    [Header("Run")]
    [Range(1f, 100f)] public float MaxRunSpeed = 20f;                           //最大速度

    [Header("地面检测")]
    public LayerMask GroundLayer;                                               //地面层
    public float GroundDetectionRayLength = 0.02f;                              //地面检测射线长度
    public float HeadDetectionRayLength = 0.02f;                                //头部检测射线长度
    [Range(0f, 1f)] public float HeadWidth = 0.75f;                             //头宽
}
