using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EdgeDetectionEditor : VolumeComponent, IPostProcessComponent
{
    [Tooltip("是否启用效果")] public BoolParameter enable = new BoolParameter(false);
    [Tooltip("描边颜色")]public ColorParameter EdgeColor = new ColorParameter(Color.white) ;

    [Tooltip("描边宽度")]public FloatParameter EdgeWidth = new FloatParameter(0f);

    [Tooltip("背景色混合强度")] public FloatParameter BackgroundFade = new FloatParameter(0f);

    [Tooltip("背景色")] public ColorParameter BackgroundColor = new ColorParameter(Color.white);

    public bool IsActive() => enable.value;

    public bool IsTileCompatible() => false;
}
