using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class PixelateFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Setting
    {
        public Material myMaterial;
        public RenderPassEvent injectPoint = RenderPassEvent.AfterRenderingTransparents;
    }

    class PixelatePass : ScriptableRenderPass
    {
        private Material _material;
        private RTHandle _source;
        private RTHandle _tempTexture;

        public PixelatePass(Material mat)
        {
            _material = mat;
        }
        
        //在类里定义静态 readonly ID缓存
        // static readonly int ColumnsID      = Shader.PropertyToID("_Columns");
        // static readonly int RowsID         = Shader.PropertyToID("_Rows");
        static readonly int IntervalID         = Shader.PropertyToID("_Interval");
        

        //此方法由 URP 在渲染相机之前调用，用于配置渲染目标和其他特定于相机的状态
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //获取相机当前的颜色目标 RTHandle
            _source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(ref _tempTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp,
                name: "_TempTex");

            //告诉 ScriptableRenderPass 你的渲染目标
            ConfigureTarget(_source);
            //指定在 Pass 开始时，是否清除渲染目标（颜色/深度），以及用什么颜色清
            //ClearFlag.None：不清空（保留原有内容）
            ConfigureClear(ClearFlag.None, Color.black);

            //调试日志，以确认该方法正在被调用
            // Debug.Log("PixelateFeature OnCameraSetup");
        }

        //此方法由 URP 调用以执行渲染 pass，是发出实际渲染命令的地方
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_material == null || _source == null || _tempTexture == null) return;

            var stack = VolumeManager.instance.stack;
            var customEffect = stack.GetComponent<Pixelate>();

            if (customEffect == null || !customEffect.IsActive()) return;

            //将自定义的 volume component 里的属性值传递给 shader
            
            // _material.SetFloat(ColumnsID, customEffect.columnCount.value);
            // _material.SetFloat(RowsID, customEffect.rowCount.value);
            _material.SetFloat(IntervalID, customEffect.intervalCount.value);
            

            //从对象池获取一个 CommandBuffer，CommandBuffer 用于对渲染命令进行排队
            var cmd = CommandBufferPool.Get("Pixelate");

            using (new ProfilingScope(cmd, new ProfilingSampler("Pixelate")))
            {
                // Blit 到临时纹理
                Blitter.BlitCameraTexture(cmd, _source, _tempTexture);
                // 再用雾材质 Blit 回去
                Blitter.BlitCameraTexture(cmd, _tempTexture, _source, _material, 0);
            }

            //将 CommandBuffer 执行，清除，释放回对象池
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);

            // 调试日志，以确认此方法正在被调用
            // Debug.Log("Pixelate OnExecute");
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // 对于由系统或 Dispose() 管理的 RTHandle，不需要在这里清理。
        }

        public void Dispose()
        {
            // 在此特定代码中，_source 被赋值为 `renderingData.cameraData.renderer.cameraColorTargetHandle`。
            // 这意味着 _source 是一个系统管理的 RTHandle (相机的实际渲染目标)。
            // 我们不应该在这里释放它，因为它不由此通道拥有。
            // 释放它可能会破坏渲染或导致错误。
            _tempTexture?.Release();
            _tempTexture = null;
        }
    }

    public Setting setting = new Setting();

    private PixelatePass m_pass;

    public override void Create()
    {
        m_pass = new PixelatePass(setting.myMaterial)
        {
            renderPassEvent = setting.injectPoint
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // if (renderingData.cameraData.camera.cameraType != CameraType.Game) return;//只在game视图里出现后处理效果
        if (setting.myMaterial == null) return;

        // 确保当前相机生成深度贴图
        // renderingData.cameraData.requiresDepthTexture = true;
        renderer.EnqueuePass(m_pass);
    }

    protected override void Dispose(bool disposing)
    {
        //调用渲染通道的自定义 Dispose 方法以释放其 RTHandle
        m_pass?.Dispose();
        //处置后将通道实例设置为空
        m_pass = null;
    }
}