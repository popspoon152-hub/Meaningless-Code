using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;//跟踪目标
    public float smooth=3.0f;  //平滑时间
    public Vector2 targetOffset;//跟踪点偏移值
    public Vector3 speed = Vector3.zero;//初始速度
    public float PosZ = -10;
    public Camera myCamera ;

    [Header("相机移动限制")]
    public Vector2 lrRange = new Vector2(-10, 10);
    public Vector2 udRange = new Vector2(-5, 5);


    void Start()
    {
        myCamera = GetComponent<Camera>();
        PosZ = transform.position.z;
    }
    void LateUpdate()
    {
        if (transform.position != target.position)
        {
            Vector3 targetPos = SetTargetPos();
            targetPos.z = PosZ;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref speed, smooth);  //起点，终点，速度，时间
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(SetTargetPos(), 0.1f);

        // 绘制左右范围线
        Gizmos.DrawLine(new Vector2(lrRange.x, 1), new Vector2(lrRange.x, -1));
        Gizmos.DrawLine(new Vector2(lrRange.y, 1), new Vector2(lrRange.y, -1));
        // 绘制上下范围线
        Gizmos.DrawLine(new Vector2(1, udRange.x), new Vector2(-1, udRange.x));
        Gizmos.DrawLine(new Vector2(1, udRange.y), new Vector2(-1, udRange.y));
    }

    private Vector3 SetTargetPos()//设置跟踪点（主要是懒得写）
    {
        float higntDistance = myCamera.orthographicSize;//摄像机中心到上边界的距离
        float weightDistance = higntDistance * myCamera.aspect;// 中心到左边界的距离（Size × 宽高比）
        Vector3 targetPos = target.position + (Vector3)targetOffset;
        targetPos.x = Mathf.Clamp(targetPos.x, lrRange.x + weightDistance, lrRange.y - weightDistance);
        targetPos.y = Mathf.Clamp(targetPos.y, udRange.x + higntDistance, udRange.y - higntDistance);
        return targetPos;
    }
}
