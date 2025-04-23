using System;
using System.Collections.Generic;
using System.Linq;

public abstract class HandlerBase
{
    public string eventName;
    public virtual void Invoke() { }
    public virtual void Invoke(object param1) { }
    public virtual void Invoke(object param1, object param2) { }
    public virtual void Invoke(object param1, object param2, object param3) { }
    public virtual void AddListener(object listener) { }
    public virtual int RemoveListener(object listener) => throw new NotImplementedException();
}

public class EventManager
{
    private static Dictionary<string, HandlerBase> handlerDic { get; } = new();
    #region 添加监听事件
    public static void AddListener(string eventName, Action listener)
    {
        if (handlerDic.TryGetValue(eventName, out var handler) == false)
            handlerDic.Add(eventName, handler = new NoParamHandler(eventName));
        handler.AddListener(listener);
    }
    /// <summary>
    /// For lua receive a param
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="listener"></param>
    public static void AddListenerWithAParam(string eventName, Action<object> listener)
    {
        AddListener(eventName, listener);
    }
    /// <summary>
    /// For lua receive a param
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="listener"></param>
    public static void AddListenerWith2Param(string eventName, Action<object, object> listener)
    {
        AddListener(eventName, listener);
    }
    /// <summary>
    /// For lua receive a param
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="listener"></param>
    public static void AddListenerWith3Param(string eventName, Action<object, object, object> listener)
    {
        AddListener(eventName, listener);
    }
    public static void AddListener(string eventName, Action<object> listener)
    {
        AddListener<object>(eventName, listener);
    }
    public static void AddListener(string eventName, Action<object, object> listener)
    {
        AddListener<object, object>(eventName, listener);
    }
    public static void AddListener(string eventName, Action<object, object, object> listener)
    {
        AddListener<object, object, object>(eventName, listener);
    }
    public static void AddListener<T>(string eventName, Action<T> listener)
    {
        eventName = $"{eventName}_1";
        if (handlerDic.TryGetValue(eventName, out var handler) == false)
            handlerDic.Add(eventName, handler = new OneParamHandler<T>(eventName));
        handler.AddListener(listener);
    }
    public static void AddListener<T1, T2>(string eventName, Action<T1, T2> listener)
    {
        eventName = $"{eventName}_2";
        if (handlerDic.TryGetValue(eventName, out var handler) == false)
            handlerDic.Add(eventName, handler = new TwoParamHandler<T1, T2>(eventName));
        handler.AddListener(listener);
    }
    public static void AddListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener)
    {
        eventName = $"{eventName}_3";
        if (handlerDic.TryGetValue(eventName, out var handler) == false)
            handlerDic.Add(eventName, handler = new ThreeParamHandler<T1, T2, T3>(eventName));
        handler.AddListener(listener);
    }
    #endregion
    #region 发送事件
    public static void Invoke(string eventName)
    {
        handlerDic.GetValueOrDefault(eventName)?.Invoke();
    }
    public static void Invoke(string eventName, object param1)
    {
        handlerDic.GetValueOrDefault($"{eventName}_1")?.Invoke(param1);
    }
    public static void Invoke(string eventName, object param1, object param2)
    {
        handlerDic.GetValueOrDefault($"{eventName}_2")?.Invoke(param1, param2);
    }
    public static void Invoke(string eventName, object param1, object param2, object param3)
    {
        handlerDic.GetValueOrDefault($"{eventName}_3")?.Invoke(param1, param2, param3);
    }
    public static void Invoke<T>(string eventName, T param1)
    {
        Invoke(eventName, (object)param1);
    }
    public static void Invoke<T1, T2>(string eventName, T1 param1, T2 param2)
    {
        Invoke(eventName, (object)param1, (object)param2);
    }
    public static void Invoke<T1, T2, T3>(string eventName, T1 param1, T2 param2, T3 param3)
    {
        Invoke(eventName, (object)param1, (object)param2, (object)param3);
    }
    #endregion
    #region 移除监听事件
    private static void RemoveListenerInternal(string eventName, object listener)
    {
        if (handlerDic.TryGetValue(eventName, out var handler) && handler.RemoveListener(listener) == 0)
            handlerDic.Remove(eventName);
    }
    public static void RemoveListener(string eventName, Action listener)
    {
        RemoveListenerInternal(eventName, listener);
    }
    /// <summary>
    /// For lua receive a param
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="listener"></param>
    public static void RemoveListenerWithAParam(string eventName, Action<object> listener)
    {
        RemoveListener(eventName, listener);
    }
    /// <summary>
    /// For lua receive a param
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="listener"></param>
    public static void RemoveListenerWith2Param(string eventName, Action<object, object> listener)
    {
        RemoveListener(eventName, listener);
    }
    /// <summary>
    /// For lua receive a param
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="listener"></param>
    public static void RemoveListenerWith3Param(string eventName, Action<object, object, object> listener)
    {
        RemoveListener(eventName, listener);
    }
    public static void RemoveListener<T1>(string eventName, Action<T1> listener)
    {
        RemoveListenerInternal($"{eventName}_1", listener);
    }
    public static void RemoveListener<T1, T2>(string eventName, Action<T1, T2> listener)
    {
        RemoveListenerInternal($"{eventName}_2", listener);
    }
    public static void RemoveListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener)
    {
        RemoveListenerInternal($"{eventName}_3", listener);
    }
    public static void RemoveEvent(string eventName)
    {
        foreach (var key in handlerDic.Keys.Where(k => k.Contains(eventName)).ToArray())
            handlerDic.Remove(key);
    }
    public static void RemoveAllEvent()
    {
        handlerDic.Clear();
    }
    #endregion
}
