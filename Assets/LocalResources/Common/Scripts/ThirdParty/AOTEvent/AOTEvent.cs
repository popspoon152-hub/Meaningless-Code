using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CutyRoom.Core
{
    public class AOTEvent
    {
        public static StringBuilder ErrMsg = null;
        /// <summary>
        /// 取消所有事件的监听
        /// </summary>
        public static void OffAllEvent()
        {
            AOTEvent.ErrMsg ??= new();
            AOTEvent.Emit((int)AOTEventEnum.OffAllEvent);
            // #if UNITY_EDITOR
            if (actionMap != null)
                foreach (var item in actionMap)
                {
                    var list = item.Value.GetInvocationList();
                    foreach (var _Delegate in list)
                    {
                        AOTEvent.ErrMsg.AppendLine($"target:{_Delegate.Target} func:{_Delegate.Method} ");
                    }
                }
            if (actionMap != null)
                foreach (var item in actionMapOnce)
                {
                    var list = item.Value.GetInvocationList();
                    foreach (var _Delegate in list)
                    {
                        AOTEvent.ErrMsg.AppendLine($"target:{_Delegate.Target} func:{_Delegate.Method} ");
                    }
                }
            if (AOTEvent.ErrMsg.Length > 0)
            {
                Debug.LogError($"没有取消监听的事件\n{AOTEvent.ErrMsg}");
            }

            // #endif
            actionMap?.Clear();
            actionMapOnce?.Clear();
            AOTEvent.ErrMsg = null;

        }
        public static Dictionary<int, Action> actionMap =
          new Dictionary<int, Action>();


        public static Dictionary<int, Action> actionMapOnce =
          new Dictionary<int, Action>();
        /// <summary>
        /// 一次性监听
        /// </summary>
        /// <param name="e"></param>
        /// <param name="func"></param>
        public static void Once(int e, Action func)
        {
            if (actionMapOnce.ContainsKey(e))
            {
                actionMapOnce[e] += func;
            }
            else
            {
                actionMapOnce[e] = func;
            }
        }
        public static void On(int e, Action func)
        {
            if (actionMap.ContainsKey(e))
            {
#if UNITY_EDITOR
                var list = actionMap[e].GetInvocationList();
                foreach (var action in list)
                {
                    if (action.Equals(func))
                    {
                        throw new Exception($"重复监听 event:{e} \nTarget:{func.Target} \nFunc:{func.Method} ");
                    }
                }
#endif
                actionMap[e] += func;
            }
            else
            {
                actionMap[e] = func;
            }
        }

        public static void Off(int e, Action func)
        {
            if (actionMap.ContainsKey(e))
            {
                actionMap[e] -= func;
                if (actionMap[e] == null || actionMap[e].GetInvocationList().Length == 0)
                {
                    actionMap.Remove(e);
                }
            }
        }
        public static void Off(int e)
        {
            if (actionMap.ContainsKey(e))
            {
                actionMap.Remove(e);
            }
        }
        public static void Emit(int e)
        {
            if (actionMap.TryGetValue(e, out var func))
            {
                try
                {
                    func.Invoke();
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception);
                }
            }
            if (actionMapOnce.TryGetValue(e, out var funcOnce))
            {
                try
                {
                    funcOnce.Invoke();
                    actionMapOnce.Remove(e);
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception);
                }
            }
        }
    }

}

