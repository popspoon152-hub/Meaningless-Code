using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public abstract class PostProcessingUniversalRenderPass<T> : ScriptableRenderPass where T : VolumeComponent, IPostProcessComponent
{
    protected abstract string RenderTag { get; }
    protected T volumeComponent;
    protected Material material;
    protected Shader shader;
    
    // 缓冲区
    protected static readonly int TempBufferId1 = Shader.PropertyToID("_TempBuffer1");
    protected static readonly int TempBufferId2 = Shader.PropertyToID("_TempBuffer2");

    protected virtual bool IsActive() => volumeComponent.IsActive();

    //初始化后处理渲染通道的实例
    public PostProcessingUniversalRenderPass(RenderPassEvent renderPassEvent, Shader shader)
    {
        this.renderPassEvent = renderPassEvent;
        this.shader = shader;

        if (shader != null)
        {
            material = CoreUtils.CreateEngineMaterial(shader);
        }
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        //检查相机有效性
        if (!renderingData.cameraData.postProcessEnabled)
        {
            //Debug.LogWarning("ScriptableRenderPass:调用" + RenderTag + "的摄像机未开启后处理");
            return;
        }
        //获取 Volume 组件并检查是否启用
        var stack = VolumeManager.instance.stack;
        volumeComponent = stack.GetComponent<T>();
        if (volumeComponent == null || !volumeComponent.IsActive())
        {
            //if (volumeComponent == null) Debug.LogError("ScriptableRenderPass:" + RenderTag + "未获取到Volume组件");
            //else Debug.LogWarning("ScriptableRenderPass:" + RenderTag + "的Volume组件未激活");
            return;
        }

        if (material == null)
        {
            //Debug.LogWarning("ScriptableRenderPass:" + RenderTag + "的材质初始化失败");
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

    public EdgeDetecteionPass(RenderPassEvent renderPassEvent, Shader shader) 
    : base(renderPassEvent, shader) { }

    protected override void RenderPostProcessingEffect(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;
        var camera = cameraData.camera;

        var src = cameraData.renderer.cameraColorTargetHandle;
        int dest = TempBufferId1;

        //填写你的变量
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

public class NosiePass : PostProcessingUniversalRenderPass<Nosie>
{
    protected override string RenderTag => "NosiePass";

    public NosiePass(RenderPassEvent renderPassEvent, Shader shader) 
    : base(renderPassEvent, shader) { }

    protected override void RenderPostProcessingEffect(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;
        var camera = cameraData.camera;

        var src = cameraData.renderer.cameraColorTargetHandle;
        int dest = TempBufferId1;

        material.SetFloat("_Speed", volumeComponent.speed.value);
        material.SetFloat("_Strength", volumeComponent.strength.value);

        cmd.GetTemporaryRT(dest, camera.scaledPixelWidth, camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(src, (RenderTargetIdentifier)dest);

        if (volumeComponent.enable == false) cmd.Blit((RenderTargetIdentifier)dest, src);
        else cmd.Blit((RenderTargetIdentifier)dest, src, material, 1);
    }

}
public class LineBlockPass : PostProcessingUniversalRenderPass<LineBlock>
{
    protected override string RenderTag => "LineBlockPass";

    public LineBlockPass(RenderPassEvent renderPassEvent, Shader shader)
     : base(renderPassEvent, shader) { }

    protected override void RenderPostProcessingEffect(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;
        var camera = cameraData.camera;

        var src = cameraData.renderer.cameraColorTargetHandle;
        int dest = TempBufferId1;

        material.SetFloat("_Frequency", volumeComponent.frequency.value);
        material.SetFloat("_TimeX", volumeComponent.timeX.value);
        material.SetFloat("_LinesWidth", volumeComponent.lineswidth.value);
        material.SetFloat("_Amount", volumeComponent.amount.value);
        material.SetFloat("_Offest", volumeComponent.offset.value);
        material.SetFloat("_Alpha", volumeComponent.alpha.value);

        cmd.GetTemporaryRT(dest, camera.scaledPixelWidth, camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(src, (RenderTargetIdentifier)dest);

        if (volumeComponent.enable == false) cmd.Blit((RenderTargetIdentifier)dest, src);
        else cmd.Blit((RenderTargetIdentifier)dest, src, material, 2);
    }

}

public class BlockGlitchPass : PostProcessingUniversalRenderPass<BlockGlitch>
{
    protected override string RenderTag => "BlockGlitchPass";

    public BlockGlitchPass(RenderPassEvent renderPassEvent, Shader shader)
     : base(renderPassEvent, shader) { }

    protected override void RenderPostProcessingEffect(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;
        var camera = cameraData.camera;

        var src = cameraData.renderer.cameraColorTargetHandle;
        int dest = TempBufferId1;

        material.SetFloat("_BlockSize", volumeComponent.blockSize.value);
        material.SetFloat("_Speed", volumeComponent.speed.value);
        material.SetFloat("_MaxRGBSplitX", volumeComponent.MaxRGBSplitX.value);
        material.SetFloat("_MaxRGBSplitY", volumeComponent.MaxRGBSplitY.value);

        cmd.GetTemporaryRT(dest, camera.scaledPixelWidth, camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(src, (RenderTargetIdentifier)dest);

        if (volumeComponent.enable == false) cmd.Blit((RenderTargetIdentifier)dest, src);
        else cmd.Blit((RenderTargetIdentifier)dest, src, material, 3);
    }

}

public class Vignette02Pass : PostProcessingUniversalRenderPass<Vignette02>
{
    protected override string RenderTag => "Vignette02Pass";
    
    public Vignette02Pass(RenderPassEvent renderPassEvent, Shader shader) : base(renderPassEvent, shader) {}

    protected override void RenderPostProcessingEffect(CommandBuffer cmd, ref RenderingData renderingData)
    {
        
        ref var cameraData = ref renderingData.cameraData;
        var camera = cameraData.camera;

        var src = cameraData.renderer.cameraColorTargetHandle;
        int dest = TempBufferId1;

        material.SetVector("_Color", volumeComponent.颜色.value);
        material.SetFloat("_Intensity", volumeComponent.强度.value);
        material.SetVector("_Center", volumeComponent.中心坐标.value);
        material.SetFloat("_CenterRadius", volumeComponent.中心半径.value);
        material.SetFloat("_SmoothnessRadius", volumeComponent.平滑半径.value);
        material.SetFloat("_BrightnessThreshold", volumeComponent.亮度阈值.value);
        material.SetFloat("_SmoothnessThreshold", volumeComponent.亮度阈值平滑值.value);
        material.SetInt("_Rounded", volumeComponent.是否将中心还原为圆形.value ? 1 : 0);
        
        cmd.GetTemporaryRT(dest, camera.scaledPixelWidth, camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(src, (RenderTargetIdentifier)dest);
        
        if (volumeComponent.开关 == false) cmd.Blit((RenderTargetIdentifier)dest, src);
        else cmd.Blit((RenderTargetIdentifier)dest, src, material, 4);
    }
}

public class PixelatePass : PostProcessingUniversalRenderPass<Pixelate>
{
    protected override string RenderTag => "PixelatePass";

    public PixelatePass(RenderPassEvent renderPassEvent, Shader shader)
    : base(renderPassEvent, shader) { }

    protected override void RenderPostProcessingEffect(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;
        var camera = cameraData.camera;

        var src = cameraData.renderer.cameraColorTargetHandle;
        int dest = TempBufferId1;

        material.SetFloat("_Interval", volumeComponent.像素格数.value);

        cmd.GetTemporaryRT(dest, camera.scaledPixelWidth, camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(src, (RenderTargetIdentifier)dest);

        if (volumeComponent.开关 == false) cmd.Blit((RenderTargetIdentifier)dest, src);
        else cmd.Blit((RenderTargetIdentifier)dest, src, material, 5);
    }

}
