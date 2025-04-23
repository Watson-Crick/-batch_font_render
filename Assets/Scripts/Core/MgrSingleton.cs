using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using UnityEngine;

namespace VNWGame
{
    /// <summary>
    /// 单例类模板
    /// </summary>
    public abstract class MgrSingleton<T> where T : MgrSingleton<T>
    {
    	private static T _instance=null;
    
    	public static bool IsInit()
    	{
    		return _instance != null;
    	}
    	protected MgrSingleton()
    	{
    		
    	}
    	public static T Instance
    	{
    		get
    		{ 
    			if (_instance == null)
    			{
    				// 先获取所有非public的构造方法
    				var ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
    				// 从ctors中获取无参的构造方法
    				var ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);
    				if (ctor == null)
    					throw new Exception("没有私有的构造方法");
    				// 调用构造方法
    				_instance = ctor.Invoke(null) as T;
                    // Debug.LogError($"MgrSingleton Instance:{_instance.GetType()}");
                    SingletonManagerForVNWGame.AddType(_instance.GetType());
    				_instance.Init();
    			}
    			return _instance;
    		}
    	}
    
        public virtual void Init()
    	{
			
    	}

        public virtual void Destroy()
        {
	        
        }
        
        public static void DestroyInstance() {
    		if(_instance != null) {
	            Debug.LogError($"DestroyInstance:{_instance.GetType()}");
	            _instance.Destroy();
	            SingletonManagerForVNWGame.RemoveType(_instance.GetType());
    			_instance = null;
    		}
    	}
    }
    
    public class SingletonManagerForVNWGame {
	    private static HashSet<Type> singletonTypes = new();
	    public static void AddType(Type t) {
		    singletonTypes.Add(t);
	    }
	    public static void RemoveType(Type t) {
		    singletonTypes.Remove(t);
	    }
	    
	    public static void DestroyAllInstance() {
		    Debug.LogError("DestroyAllInstance");
		    foreach(var t in singletonTypes.ToArray())
		    {
			    // var method = t.BaseType.GetMethod("DestroyInstance", BindingFlags.Static | BindingFlags.Public);
			    if (t.BaseType.GetMethod("DestroyInstance", BindingFlags.Static | BindingFlags.Public) is {} method)
			    {
				    method.Invoke(null, null);
			    }
			    // var field = t.BaseType.GetField("_instance", BindingFlags.NonPublic|BindingFlags.Static);
			    // field.SetValue(null, null);
		    }
		    singletonTypes.Clear();
	    }
    }
}

