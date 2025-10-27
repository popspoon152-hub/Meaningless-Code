
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Transform playerPos;
    [Header("是否是固定位置")]
    public bool isStable=false;
    public float followDistance;
    private Transform mainCameraTrans; // 主摄像机的Transform组件
    private Vector3 lastCameraPosition; // 上一帧摄像机的位置
    private float textureUnitSizeX; // 背景图单位尺寸
    private Vector3 offset;
    public bool followCamera;

    public Vector2 followWeight; // 跟随摄像机的权重
                                 // 距离越远的物体权重越高，如 天空、云、太阳等 设置0.8-1范围效果尚可
                                 // 距离越近的物体权重越低，如 身边的树木、花草、房子等等

    void Start()
    {
        if (isStable)
        {
            playerPos = GameObject.FindWithTag("Player").GetComponent<Transform>();
            followCamera = false;
        }
        
            mainCameraTrans = Camera.main.transform; // 获取主摄像机的Transform组件
            lastCameraPosition = mainCameraTrans.position; // 初始化上一帧摄像机的位置为当前摄像机的位置

            Sprite sprite = GetComponent<SpriteRenderer>().sprite;
            Texture2D texture = sprite.texture; // 获取Sprite的纹理
            textureUnitSizeX = texture.width / sprite.pixelsPerUnit; // 计算背景图在游戏场景里的单位尺寸
            offset = transform.position - mainCameraTrans.position;
        
    }
    void Update()
    {

    }
    private void LateUpdate()
    {
        if (isStable)
        {
            float dis = Mathf.Abs(transform.position.x - playerPos.position.x);
            if (dis > followDistance)
            {
                lastCameraPosition = mainCameraTrans.position;
                return;
            }
        }
        ImageFollowCamera();
        if(followCamera)
        {
            ResetImageX();
        }

        lastCameraPosition = mainCameraTrans.position; // 更新上一帧摄像机的位置
    }

    private void ResetImageX()
    {
        // 检查是否需要移动背景
        if (Mathf.Abs(mainCameraTrans.position.x - transform.position.x) >= textureUnitSizeX)
        {
            // 重置背景位置
            transform.position = new Vector3(mainCameraTrans.position.x, transform.position.y, transform.position.z);
        }
    }

    private void ImageFollowCamera()
    {
        // 计算摄像机位置的偏移量
        Vector3 offsetPosition = mainCameraTrans.position - lastCameraPosition;

        // 根据权重调整背景图片的位置
        transform.position += new Vector3(offsetPosition.x * followWeight.x, offsetPosition.y * followWeight.y, 0);
    }
}