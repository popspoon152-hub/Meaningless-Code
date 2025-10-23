using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于调试 Pathfinding 结果的可视化脚本，不修改 Pathfinding 本身
/// </summary>
[ExecuteAlways]
public class PathfindingDebugger : MonoBehaviour
{
    [Header("调试参数")]
    public Pathfinding pathfinding;     // 引用要调试的 Pathfinding 组件
    public Transform startPoint;        // 起点（通常是Boss或Seeker）
    public Transform endPoint;          // 终点（通常是Player或Target）
    public bool autoUpdate = true;      // 是否实时更新路径
    public float updateInterval = 0.5f; // 自动更新间隔（秒）

    [Header("可视化设置")]
    public Color lineColor = Color.green;
    public Color nodeColor = Color.cyan;
    public Color startColor = Color.yellow;
    public Color endColor = Color.red;
    public float nodeRadius = 0.15f;

    private List<Vector3> _currentPath;
    private float _updateTimer;

    private void Update()
    {
        if (pathfinding == null || startPoint == null || endPoint == null)
            return;

        if (!autoUpdate) return;

        _updateTimer += Time.deltaTime;
        if (_updateTimer >= updateInterval)
        {
            _updateTimer = 0f;
            UpdatePath();
        }
    }

    [ContextMenu("Manual Update Path")]
    public void UpdatePath()
    {
        if (pathfinding == null)
        {
            Debug.LogWarning("[PathfindingDebugger] Missing Pathfinding reference.");
            return;
        }

        if (startPoint == null || endPoint == null)
        {
            Debug.LogWarning("[PathfindingDebugger] Missing startPoint or endPoint reference.");
            return;
        }

        _currentPath = pathfinding.FindPath(startPoint.position, endPoint.position);

        //if (_currentPath == null || _currentPath.Count == 0)
        //{
        //    Debug.LogWarning("[PathfindingDebugger] Pathfinding returned no valid path.");
        //}
        //else
        //{
        //    Debug.Log($"[PathfindingDebugger] Path updated. Node count = {_currentPath.Count}");
        //}
    }

    private void OnDrawGizmos()
    {
        if (_currentPath == null || _currentPath.Count == 0)
            return;

        Gizmos.color = lineColor;
        for (int i = 0; i < _currentPath.Count - 1; i++)
        {
            Gizmos.DrawLine(_currentPath[i], _currentPath[i + 1]);
        }

        // 绘制节点
        foreach (var node in _currentPath)
        {
            Gizmos.color = nodeColor;
            Gizmos.DrawSphere(node, nodeRadius);
        }

        // 绘制起点与终点
        if (startPoint != null)
        {
            Gizmos.color = startColor;
            Gizmos.DrawSphere(startPoint.position, nodeRadius * 1.2f);
        }

        if (endPoint != null)
        {
            Gizmos.color = endColor;
            Gizmos.DrawSphere(endPoint.position, nodeRadius * 1.2f);
        }
    }
}
