using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Map
{
    //注意: BeansPositions数组中的位置是相对于MapsPrefabs数组中对应地图预制体的位置偏移
    public GameObject TileMap;                                                                      //地图数组
    public Vector2[] BeansPositions;                                                                //豆子生成点数组

    [Header("豆子生成逻辑")]
    [Range(1f, 10f)] public int MinBeansInstantiatePerTime;                                         //地图每次生成最少的豆子数量
    [Range(1f, 10f)] public int MaxBeansInstantiatePerTime;                                         //地图每次生成最多的豆子数量
    [Range(1f, 5f)] public int BeansInstantiateTimes;                                               //每个地图生成豆子的次数

    [Header("地图停留时间")]
    [Range(5f, 120f)] public float MapStayTime;                                                     //地图停留时间(然后切换下一个地图)
}  

[CreateAssetMenu(menuName = "FirstStage/EnvironmentAndMapStats")]
public class EnvironmentAndMapStats : ScriptableObject
{
    [Header("引用")]
    public GameObject BeansPrefabs;                                                                 //豆子预制体

    [Header("地图与豆子生成点")]
    public Map[] Maps;                                                                              //地图
}
