using System;
using System.Collections.Generic;
using UnityEngine;

public class TwoParamHandler<T1, T2> : HandlerBase
{
    private List<Action<T1, T2>> listenerLst;

    public TwoParamHandler(string eventName)
    {
        this.eventName = eventName;
        this.listenerLst = new List<Action<T1, T2>>();
    }

    public override void AddListener(object listener)
    {
        if (listener == null || this.listenerLst.Contains((Action<T1, T2>)listener))
        {
            return;
        }
        this.listenerLst.Add((Action<T1, T2>)listener);
    }

    public override void Invoke(object param1, object param2)
    {
        if (param1 != null && !object.Equals(param1.GetType(), typeof(T1)) && !typeof(T1).IsAssignableFrom(param1.GetType()))
        {
            Debug.LogError("事件： " + eventName + "参数类型错误，param1类型为： " + typeof(T1));
            return;
        }
        if (param2 != null && !object.Equals(param2.GetType(), typeof(T2)) && !typeof(T2).IsAssignableFrom(param2.GetType()))
        {
            Debug.LogError("事件： " + eventName + "参数类型错误，param2类型为： " + typeof(T2));
            return;
        }
        if (listenerLst != null && listenerLst.Count > 0) 
        {
            for (int i = 0; i < listenerLst.Count; i++)
            {
                if (listenerLst[i] != null)
                {
                    listenerLst[i]((T1)param1, (T2)param2);
                }
            }
        }
    }

    public override int RemoveListener(object listener)
    {
        return listenerLst?.Remove((Action<T1, T2>)listener) ?? false ? listenerLst.Count : 0;
    }
}
