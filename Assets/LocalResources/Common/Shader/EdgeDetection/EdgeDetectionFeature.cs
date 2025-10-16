using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class EdgeDetecteionRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public Shader shader;
    }
    public Settings settings = new Settings();
    //EdgePass edgePass;

    /// <inheritdoc/>
    public override void Create()
    {
        this.name = "EdgeDetecteionRenderFeature";
        //EdgePass = new EdgePass(settings.renderPassEvent, settings.shader);
        //定义渲染的位置
    }

    // 这里你可以在渲染器中注入一个或多个渲染通道。
    //每一帧都执行
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //把实例化的CustomRenderPass插入渲染管线
       // renderer.EnqueuePass(m_ScriptablePass);
    }
}
