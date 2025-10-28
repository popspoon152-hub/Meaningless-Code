using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class PostProcessRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
    }
    
    public Settings settings = new Settings();
    
    private EdgeDetecteionPass edgeDetectionPass;
    private NosiePass noisePass;
    private LineBlockPass lineBlockPass;
    private BlockGlitchPass blockGlitchPass;
    private Vignette02Pass vignette02Pass;
    private PixelatePass pixelatePass;
    
    public override void Create()
    {
        edgeDetectionPass = new EdgeDetecteionPass(
            settings.renderPassEvent, 
            settings.shader
        );

        noisePass = new NosiePass(
            settings.renderPassEvent,
            settings.shader
        );

        lineBlockPass = new LineBlockPass(
            settings.renderPassEvent,
            settings.shader
        );

        blockGlitchPass = new BlockGlitchPass(
            settings.renderPassEvent,
            settings.shader
        );

        vignette02Pass =new Vignette02Pass(
            settings.renderPassEvent,
            settings.shader
        );

        pixelatePass = new PixelatePass(
            settings.renderPassEvent, 
            settings.shader
        );
        
        // 其他pass的初始化...
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(edgeDetectionPass);
        renderer.EnqueuePass(noisePass);
        renderer.EnqueuePass(lineBlockPass);
        renderer.EnqueuePass(blockGlitchPass);
        renderer.EnqueuePass(vignette02Pass);
        renderer.EnqueuePass(pixelatePass);
        // 添加其他pass...
    }
}