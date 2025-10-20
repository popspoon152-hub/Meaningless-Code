using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("PostProcessing/EdgeDetecteion")]
public class EdgeDetecteion : VolumeComponent, IPostProcessComponent
{
    [Tooltip("是否启用效果")] public BoolParameter enable = new BoolParameter(false);
    [Tooltip("描边颜色")] public ColorParameter EdgeColor = new ColorParameter(Color.white);

    [Tooltip("描边宽度")] public FloatParameter EdgeWidth = new FloatParameter(0f);

    [Tooltip("背景色混合强度")] public FloatParameter BackgroundFade = new FloatParameter(0f);

    [Tooltip("背景色")] public ColorParameter BackgroundColor = new ColorParameter(Color.white);

    public bool IsActive() => enable.value;

    public bool IsTileCompatible() => false;
}

[Serializable, VolumeComponentMenu("PostProcessing/Pixelate")]
public class Pixelate : VolumeComponent, IPostProcessComponent
{
    [Tooltip("是否启用效果")] public BoolParameter 开关 = new BoolParameter(false);

    
    // [Tooltip("列数")] public ClampedIntParameter columnCount = new ClampedIntParameter(10, 0, 1000);
    // [Tooltip("行数")] public ClampedIntParameter rowCount = new ClampedIntParameter(10, 0, 1000);
    [Tooltip("像素格数")] public ClampedIntParameter 像素格数 = new ClampedIntParameter(10, 0, 1000);
    

    public bool IsActive() => 开关.value;
    public bool IsTileCompatible() => false;
}
