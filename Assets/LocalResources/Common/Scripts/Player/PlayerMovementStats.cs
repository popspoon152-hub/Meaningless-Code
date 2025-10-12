using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement")] 
public class PlayerMovementStats : ScriptableObject
{
    /// <summary>
    /// ���������ο�
    /// </summary> 

    [Header("Walk")]
    [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;                        //����ٶ�
    [Range(0.25f, 50f)] public float GroundAccleration = 5f;                    //������ٶ�
    [Range(0.25f, 50f)] public float GroundDeceleration = 20f;                  //������ٶ�               
    [Range(0.25f, 50f)] public float AirAccleration = 20f;                      //�������ٶ�
    [Range(0.25f, 50f)] public float AirDeceleration = 20f;                     //�������ٶ�

    [Header("Run")]
    [Range(1f, 100f)] public float MaxRunSpeed = 20f;                           //����ٶ�

    [Header("������")]
    public LayerMask GroundLayer;                                               //�����
    public float GroundDetectionRayLength = 0.02f;                              //���������߳���
    public float HeadDetectionRayLength = 0.02f;                                //ͷ��������߳���
    [Range(0f, 1f)] public float HeadWidth = 0.75f;                             //ͷ��
}
