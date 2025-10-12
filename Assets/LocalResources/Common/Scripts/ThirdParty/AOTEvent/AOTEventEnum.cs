using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CutyRoom.Core
{
    /// <summary>
    /// 事件 枚举
    public enum AOTEventEnum
    {
        /// <summary>
        /// 枚举开始
        /// </summary>
        BUEnum_Start = 1,
        /// <summary>
        /// 取消所有事件
        /// </summary>
        OffAllEvent,
        /// <summary>
        /// unity update 调用
        /// </summary>
        OnUpdate,

        /// <summary>
        /// unity fixedUpdate 调用
        /// </summary>
        OnFixedUpdate,

        /// <summary>
        /// 逻辑帧调用前事件
        /// </summary>
        OnLogicFrameBefore,
        /// <summary>
        /// 逻辑帧调用结束后事件
        /// </summary>
        OnLogicFrameAfter,
        /// <summary>
        /// 在渲染帧调用前事件
        /// </summary>
        OnRenderFrameBefore,
        /// <summary>
        /// 在渲染帧调用结束后事件
        /// </summary>
        OnRenderFrameAfter,

        /// <summary>
        /// 打开UI
        /// </summary>
        OPEN_UI,

        /// <summary>
        /// 关闭UI
        /// </summary>
        CLOSE_UI,


        /// <summary>
        /// 禁用碰撞
        /// </summary>
        OnColliderDisable,
        /// <summary>
        /// 标记为不销毁的 GameObject Awake
        /// </summary>
        OnAwakeStaticGameObject,

        /// <summary>
        /// 枚举结束
        /// </summary>
        BUEnum_End,
    }
}