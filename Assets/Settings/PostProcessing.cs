using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
    }

    public Settings settings = new Settings();
    EdgeDetecteionPass EdgeDetecteion;
    PixelatePass Pixelate;

    public override void Create()
    {
        this.name = "PostProcessingRendererFeature";
        EdgeDetecteion = new EdgeDetecteionPass(settings.renderPassEvent, settings.shader);
        Pixelate = new PixelatePass(settings.renderPassEvent, settings.shader);

    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(EdgeDetecteion);
        renderer.EnqueuePass(Pixelate);
    }
}