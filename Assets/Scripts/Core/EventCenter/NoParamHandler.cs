using System;
using System.Collections.Generic;

public class NoParamHandler : HandlerBase
{
    private List<Action> listenerLst;

    public NoParamHandler(string eventName)
    {
        this.eventName = eventName;
        this.listenerLst = new List<Action>();
    }

    public override void AddListener(object listener)
    {
        if (listener == null || this.listenerLst.Contains((Action)listener))
        {
            return;
        }
        this.listenerLst.Add((Action)listener);
    }

    public override void Invoke()
    {
        if (listenerLst != null && listenerLst.Count > 0)
        {
            for (int i = 0; i < listenerLst.Count; i++)
            {
                if (listenerLst[i] != null)
                {
                    listenerLst[i]();
                }
            }
        }
    }

    public override int RemoveListener(object listener)
    {
        return listenerLst?.Remove((Action)listener) ?? false ? listenerLst.Count : 0;
    }
}
