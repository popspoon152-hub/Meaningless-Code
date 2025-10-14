using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement")] 
public class PlayerMovementStats : ScriptableObject
{
    /// <summary>
    /// 参数仅供参考
    /// </summary> 

    [Header("Walk")]
    [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;                        //最大速度
    [Range(0.25f, 50f)] public float GroundAccleration = 5f;                    //地面加速度
    [Range(0.25f, 50f)] public float GroundDeceleration = 20f;                  //地面减速度               
    [Range(0.25f, 50f)] public float AirAccleration = 5f;                      //空气加速度
    [Range(0.25f, 50f)] public float AirDeceleration = 5f;                     //空气减速度

    [Header("Run")]
    [Range(1f, 100f)] public float MaxRunSpeed = 20f;                           //最大速度

    [Header("地面检测")]
    public LayerMask GroundLayer;                                               //地面层
    public float GroundDetectionRayLength = 0.02f;                              //地面检测射线长度
    public float HeadDetectionRayLength = 0.02f;                                //头部检测射线长度
    [Range(0f, 1f)] public float HeadWidth = 0.75f;                             //头宽

    [Header("Jump")]
    public float JumpHeight = 6.5f;                                             //跳跃高度
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;       //跳跃的修正系数(玩家在进行短按跳跃时的最小高度)
    public float TimeTillJumpApex = 0.35f;                                      //到达最高点所需的时间
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;            //松开按键时的重力乘数(当玩家在空中松开跳跃键时，施加在角色身上的重力会乘以这个系数，为了让你跳得更高或更低)
    public float MaxFallSpeed = 26f;                                            //最大下落速度
    [Range(1, 5)] public int NumberOfJumpsAllowed = 2;                          //允许最大跳跃次数

    [Header("JumpCut")]
    [Range(0.02f, 0.3f)] public float TimeForUpwardCancel = 0.027f;             //允许取消向上速度的时间(起跳初期通过松开按键来直接取消上升速度)

    [Header("JumpApex")]
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;                       //最高点阈值
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;                      //最高点悬停时间

    [Header("Jump Buffer")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.125f;                         //按下跳跃键,但角色此时并不满足起跳条件时,会将这个跳跃指令暂时储存起来.如果在这个时间窗口内角色变得可以起跳,那么系统会自动执行这个跳跃

    [Header("Jump Coyote Time")]
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.1f;                         //当角色从平台边缘掉落后的一段时间内,系统仍然认为角色处于地面上,并允许跳跃

    [Header("Debug")]
    public bool DebugShowIsGroundedBox;
    public bool DebugShowHeadBumpBox;

    [Header("JumpVisualization Tool")]
    public bool ShowWalkJumpArc = false;
    public bool ShowRunJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)] public int ArcSolution = 20;
    [Range(0, 500)] public int VisualizationSteps = 90;

    public float AdjustedJumpHeight { get; private set; }
    public float Gravity { get; private set; }
    public float IntialJumpVelocity { get; private set; }

    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnable()
    {
        CalculateValues();
    }

    public void CalculateValues()
    {
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
        IntialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
    }

    [Header("冲刺相关")]
}
