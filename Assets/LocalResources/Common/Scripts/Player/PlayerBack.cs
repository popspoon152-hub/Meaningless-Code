using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerBack : MonoBehaviour
{
    [Header("角色回溯")]
    [Range(1f, 5f)] public float BackTime = 2f;                     //回溯到几秒前的位置
    public PlayerHealth Health;

    [System.Serializable]
    public class TimedPostion
    {
        public Vector2 Position;
        public float Time;

        public TimedPostion(Vector2 pos, float t)
        {
            Position = pos;
            Time = t;
        }
    }
    private Queue<TimedPostion> _positionQueue = new Queue<TimedPostion>();

    private void Update()
    {
        RecordCurrentPostion();

        RemoveOldPostions();

        BackCheck();
    }

    #region Quene
    private void RecordCurrentPostion()
    {
        _positionQueue.Enqueue(new TimedPostion(transform.position, Time.time));
    }

    private void RemoveOldPostions()
    {
        float currentTime = Time.time;

        while (_positionQueue.Count > 0 && currentTime - _positionQueue.Peek().Time > BackTime)
        {
            _positionQueue.Dequeue();
        }
    }

    #endregion
    
    private void BackCheck()
    {
        if (InputManager.BackWasPressed)
        {
            Vector2 backPos;

            if (_positionQueue.Count > 0)
            {
                // 队列中最旧的位置就是两秒前的位置
                backPos = _positionQueue.Peek().Position;
                Health.HealthUntilExtraHealth();
            }
            else
            {
                // 如果没有足够的历史数据，返回当前位置
                Debug.LogWarning("Not enough position history, returning current position");
                backPos = transform.position;
            }

            transform.position = backPos;
        }

    }
}
