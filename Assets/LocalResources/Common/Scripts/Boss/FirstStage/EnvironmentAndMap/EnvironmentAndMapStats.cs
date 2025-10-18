using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Map
{
    [Header("ע��: BeansPositions�����е�λ���������MapsPrefabs�����ж�Ӧ��ͼԤ�����λ��ƫ��")]
    public GameObject TileMap;                                                                      //��ͼ����
    public Vector2[] BeansPositions;                                                                //�������ɵ�����

    [Header("���������߼�")]
    [Range(1f, 10f)] public int MinBeansInstantiatePerTime;                                         //��ͼÿ���������ٵĶ�������
    [Range(1f, 10f)] public int MaxBeansInstantiatePerTime;                                         //��ͼÿ���������Ķ�������
    [Range(1f, 5f)] public int BeansInstantiateTimes;                                               //ÿ����ͼ���ɶ��ӵĴ���

    [Header("��ͼͣ��ʱ��")]
    [Range(5f, 120f)] public float MapStayTime;                                                     //��ͼͣ��ʱ��(Ȼ���л���һ����ͼ)
}  

[CreateAssetMenu(menuName = "FirstStage/EnvironmentAndMapStats")]
public class EnvironmentAndMapStats : ScriptableObject
{
    [Header("����")]
    public GameObject BeansPrefabs;                                                                 //����Ԥ����

    [Header("��ͼ�붹�����ɵ�")]
    public Map[] Maps;                                                                              //��ͼ
}
