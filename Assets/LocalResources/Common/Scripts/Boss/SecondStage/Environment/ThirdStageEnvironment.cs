using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdStageEnvironment : MonoBehaviour
{
    [Header("References")]
    public ThirdStageEnvironmentStats EnvironmentStats;

    private Coroutine _stonesInstantiateCoroutine;


    #region LifeCycle

    private void Start()
    {
        _stonesInstantiateCoroutine = StartCoroutine(StonesInstantiateCoroutine());
    }

    private void OnDisable()
    {
        if(_stonesInstantiateCoroutine != null)
        {
            StopCoroutine(_stonesInstantiateCoroutine);
            _stonesInstantiateCoroutine = null;
        }
    }

    #endregion

    //循环生成一波石头的协程
    private IEnumerator StonesInstantiateCoroutine()
    {
        while (enabled)
        {
            float nextInterval = ChooseNextInstantiateTime();
            yield return new WaitForSeconds(nextInterval);

            int num = ChooseStoneInstantiateNum();
            for(int i = 0; i <num; i++)
            {
                float size = ChoooseStoneSize();
                Vector2 position = ChooseStoneInstantiatePosition();
                Vector3 finalPos = new Vector3(position.x, position.y, 0f);

                GameObject stone = Instantiate(EnvironmentStats.StonesPrefabs, finalPos, Quaternion.identity);
                stone.transform.localScale = new Vector3(size, size, 1f);

                float delay = ChooseEachStoneInstantiateDelay();
                yield return new WaitForSeconds(delay);
            }
        }
    }

    //每波时间间隔
    private float ChooseNextInstantiateTime()
    {
        return UnityEngine.Random.Range(EnvironmentStats.StoneMinInstantiateInterval, EnvironmentStats.StoneMaxInstantiateInterval);
    }

    //数量
    private int ChooseStoneInstantiateNum()
    {
        return UnityEngine.Random.Range(EnvironmentStats.StoneMinInstantiateNum, EnvironmentStats.StoneMaxInstantiateNum + 1);
    }

    //尺寸
    private float ChoooseStoneSize()
    {
        return UnityEngine.Random.Range(EnvironmentStats.StoneMinSize, EnvironmentStats.StoneMaxSize);
    }

    //位置
    private Vector2 ChooseStoneInstantiatePosition()
    {
        float xPosition = UnityEngine.Random.Range(EnvironmentStats.InstantiateLeftPosition.x, EnvironmentStats.InstantiateRightPosition.x);
        float yPosition = EnvironmentStats.InstantiateLeftPosition.y;

        return new Vector2(xPosition, yPosition);
    }

    //每个的时间间隔
    private float ChooseEachStoneInstantiateDelay()
    {
        return UnityEngine.Random.Range(EnvironmentStats.EachStoneMinInstantiateDelay, EnvironmentStats.EachStoneMaxInstantiateDelay);
    }
}
