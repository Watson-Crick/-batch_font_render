using System;
using System.Collections.Generic;
using UnityEngine;

public class ThreeParamHandler<T1, T2, T3> : HandlerBase
{
    private List<Action<T1, T2, T3>> listenerLst;

    public ThreeParamHandler(string eventName)
    {
        this.eventName = eventName;
        this.listenerLst = new List<Action<T1, T2, T3>>();
    }

    public override void AddListener(object listener)
    {
        if (listener == null || this.listenerLst.Contains((Action<T1, T2, T3>)listener))
        {
            return;
        }
        this.listenerLst.Add((Action<T1, T2, T3>)listener);
    }

    public override void Invoke(object param1, object param2, object param3)
    {
        if (!object.Equals(param1.GetType(), typeof(T1)) && !typeof(T1).IsAssignableFrom(param1.GetType()))
        {
            Debug.LogError("事件： " + eventName + "参数类型错误，param1类型为： " + typeof(T1));
            return;
        }
        if (!object.Equals(param2.GetType(), typeof(T2)) && !typeof(T2).IsAssignableFrom(param2.GetType()))
        {
            Debug.LogError("事件： " + eventName + "参数类型错误，param2类型为： " + typeof(T2));
            return;
        }
        if (!object.Equals(param3.GetType(), typeof(T3)) && !typeof(T3).IsAssignableFrom(param3.GetType()))
        {
            Debug.LogError("事件： " + eventName + "参数类型错误，param3类型为： " + typeof(T3));
            return;
        }
        if (listenerLst != null && listenerLst.Count > 0)
        {
            for (int i = 0; i < listenerLst.Count; i++)
            {
                if (listenerLst[i] != null)
                {
                    listenerLst[i]((T1)param1, (T2)param2, (T3)param3);
                }
            }
        }
    }

    public override int RemoveListener(object listener)
    {
        return listenerLst?.Remove((Action<T1, T2, T3>)listener) ?? false ? listenerLst.Count : 0;
    }
}
