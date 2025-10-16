using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("PostProcessing/Pixelate")]
public class Pixelate : VolumeComponent, IPostProcessComponent
{
    [Tooltip("是否启用效果")] public BoolParameter enable = new BoolParameter(false);

    
    // [Tooltip("列数")] public ClampedIntParameter columnCount = new ClampedIntParameter(10, 0, 1000);
    // [Tooltip("行数")] public ClampedIntParameter rowCount = new ClampedIntParameter(10, 0, 1000);
    [Tooltip("行数")] public ClampedIntParameter intervalCount = new ClampedIntParameter(10, 0, 1000);
    

    public bool IsActive() => enable.value;
    public bool IsTileCompatible() => false;
}

