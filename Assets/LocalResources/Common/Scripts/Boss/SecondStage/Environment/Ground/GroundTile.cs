using System.Collections;
using UnityEngine;

public class GroundTile : MonoBehaviour
{
    public bool IsDevoured = false;    // 是否被吞噬
    public float RespawnTime = 5f;     // 重新生成时间

    private Renderer _renderer;
    private Collider2D _collider;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider2D>();
    }

    public void Devour()
    {
        if (!IsDevoured)
        {
            IsDevoured = true;
            _renderer.enabled = false;   // 隐藏地面
            _collider.enabled = false;   // 禁用碰撞器

            // 启动重新生成
            StartCoroutine(Respawn());
        }
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(RespawnTime);

        IsDevoured = false;
        _renderer.enabled = true;      // 恢复显示
        _collider.enabled = true;      // 恢复碰撞
    }
}
