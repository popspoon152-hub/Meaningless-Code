using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentAndMap : MonoBehaviour
{
    [Header("地图")]
    public GameObject TileMapStairs1;                                                                      //地图数组
    public GameObject TileMapWall1;                                                                      //地图数组
    public Transform[] BeansPositions1;                                                                //豆子生成点数组
    public GameObject TileMapStairs2;                                                                      //地图数组
    public GameObject TileMapWall2;                                                                      //地图数组
    public Transform[] BeansPositions2;                                                                //豆子生成点数组

    [Header("豆子生成逻辑")]
    [Range(1f, 10f)] public int MinBeansInstantiatePerTime1 = 2;                                         //地图每次生成最少的豆子数量
    [Range(1f, 10f)] public int MaxBeansInstantiatePerTime1 = 5;                                         //地图每次生成最多的豆子数量
    [Range(1f, 5f)] public int BeansInstantiateTimes1 = 2;                                               //每个地图生成豆子的次数
    [Range(1f, 10f)] public int MinBeansInstantiatePerTime2 = 2;                                         //地图每次生成最少的豆子数量
    [Range(1f, 10f)] public int MaxBeansInstantiatePerTime2 = 5;                                         //地图每次生成最多的豆子数量
    [Range(1f, 5f)] public int BeansInstantiateTimes2 = 2;                                               //每个地图生成豆子的次数

    [Header("地图停留时间")]
    [Range(5f, 120f)] public float MapStayTime1 = 20f;                                                     //地图停留时间(然后切换下一个地图)
    [Range(5f, 120f)] public float MapStayTime2 = 20f;                                                     //地图停留时间(然后切换下一个地图)

    public GameObject BeansPrefabs;

    public int CurrentBeansCount { get; private set; }

    private int _currentMapIndex;

    private int _currentBeansInstantiatedTime;

    private int _BeansInstantiateThisTime;

    private bool _isWaitingForNextMap = false;

    private Coroutine _waitCoroutine;

    private int _beansInstantiateThisTime;

    public static EnvironmentAndMap Ins { get; private set; }

    #region LifeCycle

    private void Awake()
    {
        if(Ins == null)
        {
            Ins = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if (!ValidateReferences())
        {
            return;
        }
        _currentMapIndex = 1;

        LoadMap(_currentMapIndex);
    }

    private void Update()
    {
        if(CurrentBeansCount == 0 && !_isWaitingForNextMap)
        {
            CheckBeans();
        }
    }

    private void OnDestroy()
    {
        if(_waitCoroutine != null)
        {
            StopCoroutine(_waitCoroutine);
        }
    }

    #endregion

    #region LoadBeansAndMap
    private void LoadMap(int currentMapIndex)
    {
        if (!ValidateReferences()) return;

        TileMapStairs1.SetActive(false);
        TileMapWall1.SetActive(false);
        TileMapStairs2.SetActive(false);
        TileMapWall2.SetActive(false);

        if (currentMapIndex == 1)
        {
            TileMapStairs1.SetActive(true);
            TileMapWall1.SetActive(true);
            _currentBeansInstantiatedTime = BeansInstantiateTimes1;
        }
        else if (currentMapIndex == 2)
        {
            TileMapStairs2.SetActive(true);
            TileMapWall2.SetActive(true);
            _currentBeansInstantiatedTime = BeansInstantiateTimes2;
        }

        _isWaitingForNextMap = false;

        //创建网格
        if (GridManager.Ins != null)
        {
            GridManager.Ins.CreateGrid();
        }
        else
        {
            Debug.LogWarning("GridManager.Instance 为空！");
        }

        //实例化豆子
        ChooseInstantiatePoints();
    }

    private void ChooseInstantiatePoints()
    {
        if (!ValidateReferences()) return;

        Transform[] currentPositions;

        if (_currentMapIndex == 1)
        {
            currentPositions = BeansPositions1;
            _beansInstantiateThisTime = Mathf.Clamp(UnityEngine.Random.Range(MinBeansInstantiatePerTime1, MaxBeansInstantiatePerTime1 + 1), 1, currentPositions.Length);
        }
        else
        {
            currentPositions = BeansPositions2;
            _beansInstantiateThisTime = Mathf.Clamp(UnityEngine.Random.Range(MinBeansInstantiatePerTime2, MaxBeansInstantiatePerTime2 + 1), 1, currentPositions.Length);
        }

        // 选择生成点
        Transform[] choosePoints = GetRandomSequence(currentPositions, _beansInstantiateThisTime);

        if (choosePoints == null || choosePoints.Length == 0)
        {
            //Debug.LogError("没有有效的豆子生成点！");
            return;
        }

        // 实例化豆子
        int successfulInstantiates = 0;
        for (int i = 0; i < choosePoints.Length; i++)
        {
            if (choosePoints[i] != null)
            {
                Instantiate(BeansPrefabs, choosePoints[i].position, Quaternion.identity);
                successfulInstantiates++;
            }
        }

        CurrentBeansCount = successfulInstantiates;
        //Debug.Log($"成功生成了 {successfulInstantiates} 个豆子");
    }

    public static Transform[] GetRandomSequence(Transform[] array, int count)
    {
        if (array == null || array.Length == 0 || count <= 0)
        {
            //Debug.LogError("GetRandomSequence: 输入参数无效");
            return new Transform[0];
        }

        // 确保count不超过数组长度
        count = Mathf.Min(count, array.Length);
        Transform[] output = new Transform[count];

        // 复制数组以避免修改原数组
        Transform[] tempArray = new Transform[array.Length];
        array.CopyTo(tempArray, 0);

        // Fisher-Yates洗牌算法
        for (int i = tempArray.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Transform temp = tempArray[i];
            tempArray[i] = tempArray[j];
            tempArray[j] = temp;
        }

        // 取前count个元素
        for (int i = 0; i < count; i++)
        {
            output[i] = tempArray[i];
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
            _waitCoroutine = StartCoroutine(WaitForBattleEnd());
        }
    }

    private IEnumerator WaitForBattleEnd()
    {
        float waitTime = _currentMapIndex == 1 ? MapStayTime1 : MapStayTime2;
        if(_currentMapIndex == 1)
        {
            TileMapStairs1.SetActive(false);
            TileMapWall1.SetActive(true);
            TileMapStairs2.SetActive(false);
            TileMapWall2.SetActive(false);
        }
        else
        {
            TileMapStairs1.SetActive(false);
            TileMapWall1.SetActive(false);
            TileMapStairs2.SetActive(false);
            TileMapWall2.SetActive(true);
        }

        yield return new WaitForSeconds(waitTime);

        // 切换地图前检查对象是否仍然存在
        if (this == null) yield break;

        // 切换地图
        _currentMapIndex = _currentMapIndex == 1 ? 2 : 1;
        LoadMap(_currentMapIndex);
        _isWaitingForNextMap = false; // 重置等待状态
    }

    public void OnBeanEaten()
    {
        if (CurrentBeansCount > 0)
        {
            CurrentBeansCount--;
        }
    }
    #endregion

    #region Validation
    private bool ValidateReferences()
    {
        bool isValid = true;

        if (TileMapStairs1 == null)
        {
            Debug.LogError("TileMapStairs1 未赋值！");
            isValid = false;
        }

        if(TileMapWall1 == null)
        {
            Debug.LogError("TileMapWall1 未赋值");
            isValid = false;
        }

        if (TileMapWall2 == null)
        {
            Debug.LogError("TileMapWall2 未赋值");
            isValid = false;
        }

        if (TileMapStairs2 == null)
        {
            Debug.LogError("TileMapStairs2 未赋值！");
            isValid = false;
        }

        if (BeansPrefabs == null)
        {
            Debug.LogError("BeansPrefabs 未赋值！");
            isValid = false;
        }

        if (BeansPositions1 == null || BeansPositions1.Length == 0)
        {
            Debug.LogError("BeansPositions1 数组为空或未赋值！");
            isValid = false;
        }
        else
        {
            // 检查数组中是否有空元素
            for (int i = 0; i < BeansPositions1.Length; i++)
            {
                if (BeansPositions1[i] == null)
                {
                    Debug.LogError($"BeansPositions1 索引 {i} 为空！");
                    isValid = false;
                }
            }
        }

        if (BeansPositions2 == null || BeansPositions2.Length == 0)
        {
            Debug.LogError("BeansPositions2 数组为空或未赋值！");
            isValid = false;
        }
        else
        {
            for (int i = 0; i < BeansPositions2.Length; i++)
            {
                if (BeansPositions2[i] == null)
                {
                    Debug.LogError($"BeansPositions2 索引 {i} 为空！");
                    isValid = false;
                }
            }
        }

        return isValid;
    }
    #endregion
}
