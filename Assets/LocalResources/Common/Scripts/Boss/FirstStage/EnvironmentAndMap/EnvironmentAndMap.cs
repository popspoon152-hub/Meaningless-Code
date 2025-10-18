using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentAndMap : MonoBehaviour
{
    public int CurrentBeansCount;

    public EnvironmentAndMapStats MapStats;

    private int _currentMapIndex;

    private Map _currentMap;
    private Vector2[] _currentBeansPositions;
    private int _currentBeansInstantiatedTime;

    private int _BeansInstantiateThisTime;

    private bool _isWaitingForNextMap = false;

    private Coroutine _waitCoroutine;

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
        if(CurrentBeansCount == 0 && !_isWaitingForNextMap)
        {
            CheckBeans();
        }

        //���bossûѪ��
        if(_isWaitingForNextMap /*&& bossûѪ��*/)
        {
            CancelNextMapSwitch();
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

        //ʵ��������
        ChooseInstantiatePoints();
    }

    private void ChooseInstantiatePoints()
    {
        //ѡ�����ɼ���
        _BeansInstantiateThisTime = UnityEngine.Random.Range(_currentMap.MinBeansInstantiatePerTime, _currentMap.MaxBeansInstantiatePerTime + 1);

        //ѡ���
        Vector2[] choosePoints = GetRandomSequence(_currentBeansPositions, _BeansInstantiateThisTime);

        for(int i = 0; i < choosePoints.Length; i++)
        {
            Instantiate(MapStats.BeansPrefabs, /*(Vector2)_currentMap.TileMap.transform.position + */choosePoints[i], Quaternion.identity);
        }

        CurrentBeansCount = _BeansInstantiateThisTime;
    }

    public static Vector2[] GetRandomSequence(Vector2[] array, int count)
    {
        Vector2[] output = new Vector2[count];

        for (int i = array.Length - 1; i >= 0 && count > 0; i--)
        {
            if (UnityEngine.Random.Range(0, i + 1) < count)//������ ʣ��ȡ������/������ʣ��ĳ���
            {
                output[count - 1] = array[i];//output�����һλ��ʼ��ǰ��
                count--;
            }
        }
        return output;
    }

    #endregion

    #region CheckBeans

    private void CheckBeans()
    {
        if(_currentBeansInstantiatedTime > 0)
        {
            _currentBeansInstantiatedTime--;
            ChooseInstantiatePoints();
        }
        else
        {
            _isWaitingForNextMap = true;
            //����ս������
            _waitCoroutine = StartCoroutine(WaitForBattleEnd());
        }
    }

    private IEnumerator WaitForBattleEnd()
    {
        yield return new WaitForSeconds(_currentMap.MapStayTime);

        _waitCoroutine = null;
        _isWaitingForNextMap = false;

        //������һ����ͼ
        _currentMapIndex++;
        if (_currentMapIndex >= MapStats.Maps.Length - 1)
        {
            _currentMapIndex = 0;
        }
        LoadMap(_currentMapIndex);
    }

    private void CancelNextMapSwitch()
    {
        if (!_isWaitingForNextMap) return;

        // ֹͣЭ�̣�����״̬
        if (_waitCoroutine != null)
        {
            StopCoroutine(_waitCoroutine);
            _waitCoroutine = null;
        }
        _isWaitingForNextMap = false;
    }

    #endregion

}
