using System;
using System.Collections.Generic;
using UnityEngine;

public class OneParamHandler<T> : HandlerBase
{
    private List<Action<T>> listenerLst;

    public OneParamHandler(string eventName)
    {
        this.eventName = eventName;
        this.listenerLst = new List<Action<T>>();
    }

    public override void AddListener(object listener)
    {
        if (listener == null || this.listenerLst.Contains((Action<T>)listener))
        {
            return;
        }
        this.listenerLst.Add((Action<T>)listener);
    }

    public override void Invoke(object param)
    {
        if (param != null && !object.Equals(param.GetType(), typeof(T)) && !typeof(T).IsAssignableFrom(param.GetType()))
        {
            Debug.LogError("事件： " + eventName + "参数类型错误，param类型为： " + typeof(T));
            return;
        }
        if (listenerLst != null && listenerLst.Count > 0)
        {
            for (int i = 0; i < listenerLst.Count; i++)
            {
                if (listenerLst[i] != null)
                {
                    listenerLst[i]((T)param);
                }
            }
        }
    }

    public override int RemoveListener(object listener)
    {
        return listenerLst?.Remove((Action<T>)listener) ?? false ? listenerLst.Count : 0;
    }
}
