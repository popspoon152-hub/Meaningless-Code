using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public abstract class PostProcessingUniversalRenderPass<T> : ScriptableRenderPass where T : VolumeComponent, IPostProcessComponent
{
    protected abstract string RenderTag { get; }//提供通道名称
    protected T volumeComponent;
    protected Shader shader;
    protected Material material;

    //缓冲区
    protected static readonly int TempBufferId1 = Shader.PropertyToID("_TempBuffer1");
    protected static readonly int TempBufferId2 = Shader.PropertyToID("_TempBuffer2");
    protected virtual bool IsActive() => volumeComponent.IsActive();

    //初始化后处理渲染通道的实例
    //处理时机+后处理片段shader
    public PostProcessingUniversalRenderPass(RenderPassEvent renderPassEvent, Shader shader)
    {
        this.renderPassEvent = renderPassEvent;

        if (shader == null)
        {
            Debug.LogError("ScriptableRenderPass:" + RenderTag + "的Shader为空");
            return;
        }

        this.shader = shader;
        material = CoreUtils.CreateEngineMaterial(this.shader);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        //检查相机有效性
        if (!renderingData.cameraData.postProcessEnabled)
        {
            Debug.LogWarning("ScriptableRenderPass:调用" + RenderTag + "的摄像机未开启后处理");
            return;
        }
        //获取 Volume 组件并检查是否启用
        var stack = VolumeManager.instance.stack;
        volumeComponent = stack.GetComponent<T>();
        if (volumeComponent == null || !volumeComponent.IsActive())
        {
            if (volumeComponent == null) Debug.LogError("ScriptableRenderPass:" + RenderTag + "未获取到Volume组件");
            else Debug.LogError("ScriptableRenderPass:" + RenderTag + "的Volume组件未激活");
            return;
        }

        if (material == null)
        {
            Debug.LogError("ScriptableRenderPass:" + RenderTag + "的材质初始化失败");
            return;
        }
        //设置渲染命令缓冲区
        CommandBuffer cmd = CommandBufferPool.Get(RenderTag);

        //执行具体的后处理渲染逻辑
        RenderPostProcessingEffect(cmd, ref renderingData);

        //执行命令并释放缓冲区
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
    protected abstract void RenderPostProcessingEffect(CommandBuffer cmd, ref RenderingData renderingData);
    public void Dispose()
    {
        CoreUtils.Destroy(material);
    }
}

public class EdgeDetecteionPass : PostProcessingUniversalRenderPass<EdgeDetecteion>
{
    protected override string RenderTag => "EdgeDetecteionPass";

    public EdgeDetecteionPass(RenderPassEvent renderPassEvent, Shader shader) : base(renderPassEvent, shader) { }

    protected override void RenderPostProcessingEffect(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;
        var camera = cameraData.camera;

        var src = cameraData.renderer.cameraColorTargetHandle;
        int dest = TempBufferId1;
        material.SetVector("_EdgeColor", volumeComponent.EdgeColor.value);
        material.SetFloat("_EdgeWidth", volumeComponent.EdgeWidth.value);
        material.SetFloat("_BackgroundFade", volumeComponent.BackgroundFade.value);
        material.SetVector("_BackgroundColor", volumeComponent.BackgroundColor.value);

        cmd.GetTemporaryRT(dest, camera.scaledPixelWidth, camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(src, (RenderTargetIdentifier)dest);

        if (volumeComponent.enable == false) cmd.Blit((RenderTargetIdentifier)dest, src);
        else cmd.Blit((RenderTargetIdentifier)dest, src, material, 0);
    } 
}

public class PixelatePass : PostProcessingUniversalRenderPass<Pixelate>
{
    protected override string RenderTag => "PixelatePass";

    public PixelatePass(RenderPassEvent renderPassEvent, Shader shader) : base(renderPassEvent, shader) { }

    protected override void RenderPostProcessingEffect(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;
        var camera = cameraData.camera;
        var src = cameraData.renderer.cameraColorTargetHandle;
        int dest = TempBufferId1;
        
        //shader自定义接口
        material.SetFloat("_Interval", volumeComponent.像素格数.value);

        cmd.GetTemporaryRT(dest, camera.scaledPixelWidth, camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(src, (RenderTargetIdentifier)dest);

        if (volumeComponent.开关 == false) cmd.Blit((RenderTargetIdentifier)dest, src);
        else cmd.Blit((RenderTargetIdentifier)dest, src, material, 1);
    }

}
