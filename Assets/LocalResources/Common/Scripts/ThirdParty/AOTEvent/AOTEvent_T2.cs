using CutyRoom.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOTEvent<T, T1>
{
    static AOTEvent()
    {
        AOTEvent.Once((int)AOTEventEnum.OffAllEvent, () =>
        {
            // #if UNITY_EDITOR
            AOTEvent.ErrMsg ??= new();
            if (action_map != null)
                foreach (var item in action_map)
                {
                    var list = item.Value.GetInvocationList();
                    foreach (var _Delegate in list)
                    {
                        AOTEvent.ErrMsg.AppendLine($"target: {_Delegate.Target} func: {_Delegate.Method} ");
                    }
                }
            if (func_map != null)
                foreach (var item in func_map)
                {
                    var list = item.Value.GetInvocationList();
                    foreach (var _Delegate in list)
                    {
                        AOTEvent.ErrMsg.AppendLine($"target: {_Delegate.Target} func: {_Delegate.Method} ");
                    }
                }
            // #endif
            func_map?.Clear();
            action_map?.Clear();
        });
    }

    public static Dictionary<int, Func<T, T1>> func_map = new Dictionary<int, Func<T, T1>>();
    public static Dictionary<int, Action<T, T1>> action_map = new Dictionary<int, Action<T, T1>>();
    public static void On(int e, Func<T, T1> func)
    {
        if (func_map.ContainsKey(e))
        {
#if UNITY_EDITOR
            var list = func_map[e].GetInvocationList();
            foreach (var action in list)
            {
                if (action.Equals(func))
                {
                    throw new Exception($"ÖØ¸´¼àÌý event:{e} \nTarget:{func.Target} \nFunc:{func.Method} ");
                }
            }
#endif
            func_map[e] += func;
        }
        else
        {
            func_map[e] = func;
        }
    }
    public static T1 EmitFunc(int e, T t)
    {
        if (func_map.TryGetValue(e, out var func))
        {
            try
            {
                return func.Invoke(t);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }
        return default;
    }
    public static void Off(int e, Func<T, T1> func)
    {
        if (func_map.ContainsKey(e))
        {
            func_map[e] -= func;
            if (func_map[e] == null || func_map[e].GetInvocationList().Length == 0)
            {
                func_map.Remove(e);
            }
        }
    }
    public static void On(int e, Action<T, T1> func)
    {
        if (action_map.ContainsKey(e))
        {
#if UNITY_EDITOR
            var list = action_map[e].GetInvocationList();
            foreach (var action in list)
            {
                if (action.Equals(func))
                {
                    throw new Exception($"ÖØ¸´¼àÌý event:{e} \nTarget:{func.Target} \nFunc:{func.Method} ");
                }
            }
#endif
            action_map[e] += func;
        }
        else
        {
            action_map[e] = func;
        }
    }

    public static void Off(int e, Action<T, T1> func)
    {
        if (action_map.ContainsKey(e))
        {
            action_map[e] -= func;
            if (action_map[e] == null || action_map[e].GetInvocationList().Length == 0)
            {
                action_map.Remove(e);
            }
        }
    }
    public static void Emit(int e, T val, T1 val2)
    {
        if (action_map.TryGetValue(e, out var func))
        {
            try
            {
                func.Invoke(val, val2);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }
    }

}

