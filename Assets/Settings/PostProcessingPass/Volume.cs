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
    public IntParameter _StencilRef = new IntParameter(1);
    public IntParameter _StencilComp = new IntParameter(3);

    public bool IsActive() => enable.value;

    public bool IsTileCompatible() => false;
}

[Serializable, VolumeComponentMenu("PostProcessing/Nosie")]
public class Nosie : VolumeComponent, IPostProcessComponent
{
    [Tooltip("是否启用效果")] public BoolParameter enable = new BoolParameter(false);
    
    [Header("抖动速度")] public FloatParameter speed = new FloatParameter(0f);
    [Header("强度")] public FloatParameter strength = new FloatParameter(0f);


    public bool IsActive() => enable.value;

    public bool IsTileCompatible() => false;
}

[Serializable, VolumeComponentMenu("PostProcessing/LineBlock")]
public class LineBlock : VolumeComponent, IPostProcessComponent
{
    [Header("是否启用效果")] public BoolParameter enable = new BoolParameter(false);

    [Header("频率")] public FloatParameter frequency = new FloatParameter(0f);
    [Header("TimeX")] public FloatParameter timeX = new FloatParameter(0f);
    [Header("线条宽度")] public FloatParameter lineswidth = new FloatParameter(0f);
    [Header("线条数")] public FloatParameter amount = new FloatParameter(0f);
    [Header("偏移量")] public FloatParameter offset = new FloatParameter(0f);
    [Header("Alpha")] public FloatParameter alpha = new FloatParameter(0f);
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
