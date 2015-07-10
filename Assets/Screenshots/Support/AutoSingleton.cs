using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UObject = UnityEngine.Object;

public abstract class AutoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	static T _instance;
	public static T GetInstance()
	{
		if(_instance == null)
		{
			_instance = UObject.FindObjectOfType<T>();
			if(_instance == null)
			{
			    _instance = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent(typeof(T)) as T;

                var singleton = _instance as AutoSingleton<T>;
                singleton.OnEstablishInstance();

                if(singleton.ShouldBeMarkedAsDontDestroyOnLoad())
			        DontDestroyOnLoad(singleton);
			}
            else
            {
                var singleton = _instance as AutoSingleton<T>;
                singleton.OnEstablishInstance();

                if(singleton.ShouldBeMarkedAsDontDestroyOnLoad())
				    DontDestroyOnLoad(singleton);
            }
		}
		return _instance;
	}

	public static bool HasInstance()
	{
		return _instance != null;
	}

    protected virtual bool ShouldBeMarkedAsDontDestroyOnLoad()
    { return true; }

    protected virtual void OnEstablishInstance()
	{ }
}