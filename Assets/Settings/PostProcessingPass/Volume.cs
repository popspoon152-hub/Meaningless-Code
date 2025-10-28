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

[Serializable, VolumeComponentMenu("PostProcessing/BlockGlitch")]
public class BlockGlitch : VolumeComponent, IPostProcessComponent
{
    [Header("是否启用效果")] public BoolParameter enable = new BoolParameter(false);
    [Header("区块大小")] public FloatParameter blockSize = new FloatParameter(0f);
    [Header("抖动速度")] public FloatParameter speed = new FloatParameter(0f);
    [Header("X偏离")] public FloatParameter MaxRGBSplitX = new FloatParameter(0f);
    [Header("Y偏离")] public FloatParameter MaxRGBSplitY = new FloatParameter(0f);
    public bool IsActive() => enable.value;
    public bool IsTileCompatible() => false;
}

[Serializable, VolumeComponentMenu("PostProcessing/Vignette02")]
public class Vignette02 : VolumeComponent, IPostProcessComponent
{
    public BoolParameter 开关 = new BoolParameter(false);
    public ColorParameter 颜色 = new ColorParameter(Color.black);
    public ClampedFloatParameter 强度 = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
    public Vector2Parameter 中心坐标 = new Vector2Parameter(new Vector2(0.5f, 0.5f));
    public ClampedFloatParameter 中心半径 = new ClampedFloatParameter(0.1f, 0.0f, 0.5f);
    public ClampedFloatParameter 平滑半径 = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
    public ClampedFloatParameter 亮度阈值 = new ClampedFloatParameter(1.0f, 0.0f, 1.0f);
    public ClampedFloatParameter 亮度阈值平滑值 = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
    public BoolParameter 是否将中心还原为圆形 = new BoolParameter(false);

    public bool IsActive() => active;
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
