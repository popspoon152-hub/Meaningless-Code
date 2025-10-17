using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentAndMap : MonoBehaviour
{
    public int CurrentBeansCount;

    [SerializeField] private EnvironmentAndMapStats MapStats;

    private int _currentMapIndex;

    private Map _currentMap;
    private Vector2[] _currentBeansPositions;
    private int _currentBeansInstantiatedTime;

    private int _BeansInstantiateThisTime;

    #region LifeCycle
    private void Start()
    {
        _currentMapIndex = 0;

        for(int i = 0; i < MapStats.Maps.Length; i++)
        {
            MapStats.Maps[i].TileMap.SetActive(false);
        }

        LoadMap(_currentMapIndex);
    }

    private void Update()
    {
        if(CurrentBeansCount == 0)
        {
            CheckBeans();
        }
    }

    #endregion

    #region LoadBeansAndMap
    private void LoadMap(int currentMapIndex)
    {
        _currentMap = MapStats.Maps[currentMapIndex];
        _currentBeansPositions = _currentMap.BeansPositions;
        _currentBeansInstantiatedTime = _currentMap.BeansInstantiateTimes;

        if(_currentMapIndex == 0)
        {
            MapStats.Maps[MapStats.Maps.Length - 1].TileMap.SetActive(false);
        }
        else
        {
            MapStats.Maps[_currentMapIndex - 1].TileMap.SetActive(false);
        }
        _currentMap.TileMap.SetActive(true);

        //实例化豆子
        ChooseInstantiatePoints();
    }

    private void ChooseInstantiatePoints()
    {
        //选择生成几个
        _BeansInstantiateThisTime = UnityEngine.Random.Range(_currentMap.MinBeansInstantiatePerTime, _currentMap.MaxBeansInstantiatePerTime + 1);

        //选择点
        Vector2[] choosePoints = GetRandomSequence3(_currentBeansPositions, _BeansInstantiateThisTime);

        for(int i = 0; i < choosePoints.Length; i++)
        {
            Instantiate(MapStats.BeansPrefabs, /*(Vector2)_currentMap.TileMap.transform.position + */choosePoints[i], Quaternion.identity);
        }

        CurrentBeansCount = _BeansInstantiateThisTime;
    }

    public static Vector2[] GetRandomSequence3(Vector2[] array, int count)
    {
        Vector2[] output = new Vector2[count];

        for (int i = array.Length - 1; i >= 0 && count > 0; i--)
        {
            if (UnityEngine.Random.Range(0, i + 1) < count)//概率是 剩余取数长度/总数组剩余的长度
            {
                output[count - 1] = array[i];//output从最后一位开始往前存
                count--;
            }
        }
        return output;
    }

    #endregion

    #region Check
    
    private void CheckBeans()
    {
        if(_currentBeansInstantiatedTime > 0)
        {
            _currentBeansInstantiatedTime--;
            ChooseInstantiatePoints();
        }
        else
        {
            //进入战斗环节
            StartCoroutine(WaitForBattleEnd());

            //加载下一个地图
            _currentMapIndex++;
            if(_currentMapIndex >= MapStats.Maps.Length - 1)
            {
                _currentMapIndex = 0;
            }
            LoadMap(_currentMapIndex);
        }
    }

    private IEnumerator WaitForBattleEnd()
    {
        yield return new WaitForSeconds(_currentMap.MapStayTime);
    }


    #endregion

}
