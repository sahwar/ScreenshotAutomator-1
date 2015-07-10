using System;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using System.Reflection;
#endif

namespace TrinketCore
{
	public class ScriptableObjectSingletonAttribute : Attribute
	{
		public readonly string fileName;
		public readonly string directory;
		public readonly string extension;

		public ScriptableObjectSingletonAttribute(string inFileName, string inDirectory)
		{
			fileName = inFileName;
			directory = inDirectory;
		}

		public ScriptableObjectSingletonAttribute(string inFileName, string inDirectory, string inExtension)
		{
			fileName = inFileName;
			directory = inDirectory;
			extension = inExtension;
		}
	}

	public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObjectSingleton<T>
	{
		static T _instance = null;
		public static T Instance
		{
			get
			{
				if(_instance == null)
				{
					SetupInstance();
				}
				return _instance;
			}
		}

		static ScriptableObjectSingletonAttribute _attribute;

		public static string directory
		{
			get
			{
				GetAttribute();
				return _attribute == null ? string.Empty : Application.dataPath + "/" + _attribute.directory;
			}
		}

		public static string fileName
		{
			get
			{
				GetAttribute();
				return _attribute == null ? string.Empty : _attribute.fileName;
			}
		}

		public static string extension
		{
			get
			{
				GetAttribute();
				return _attribute == null ? string.Empty : _attribute.extension;
			}
		}

		public static string fullPath
		{
			get
			{
				return directory + "/" + fileName;
			}
		}

		public static string fullPathWithExtension
		{
			get
			{
				GetAttribute();
				return directory + "/" + fileName + _attribute.extension;
			}
		}

		static void GetAttribute()
		{
			_attribute = (ScriptableObjectSingletonAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(ScriptableObjectSingletonAttribute));
			if(_attribute == null)
			{
				Debug.LogError("Class type " + typeof(T).ToString() + " does not have a ScriptableObjectSingletonAttribute assigned, and it is required.");
			}
		}

		static string RemoveSuffix(string str, string suffix)
		{
			string trimmedString = str;
			if(trimmedString.EndsWith(suffix))
				trimmedString = trimmedString.Substring(0, trimmedString.Length - suffix.Length);
			return trimmedString;
		}

		static string RemovePrefix(string str, string prefix)
		{
			string trimmedString = str;
			if(trimmedString.StartsWith(prefix))
				trimmedString = trimmedString.Substring(prefix.Length, trimmedString.Length - prefix.Length);
			return trimmedString;
		}

		public static void SetupInstance()
		{
			GetAttribute();
			if(_attribute == null || string.IsNullOrEmpty(_attribute.fileName))
			{
				Debug.LogError("FAILED to locate ScriptableObject attribute, or the fileName was empty for type " + typeof(T).ToString());
				return;
			}

			//Debug.Log("Attempting to load ScriptableObjectSingleton via Resources with name " + _attribute.fileName);
			var returnValue = Resources.Load(_attribute.fileName);
			_instance = returnValue as T;

			if(returnValue != null && _instance == null)
				Debug.LogError("Loaded something via Resources but was unable to cast with name " + _attribute.fileName + ", cast attempt was to type " + typeof(T).ToString() + ", and type of Resource is " + returnValue.GetType().ToString());

#if UNITY_EDITOR
			if(_instance == null)
			{
				Debug.Log("Failed to load ScriptableObjectSingleton resource with name " + _attribute.fileName);
				_instance = ScriptableObject.CreateInstance(typeof(T)) as T;

				var directory = _attribute.directory;
				var fileName = _attribute.fileName;

				var slashStateOK = directory.EndsWith("/") ^ fileName.StartsWith("/");
				if(!slashStateOK)
				{
					directory = RemoveSuffix(directory, "/");
					fileName = RemovePrefix(fileName, "/");
					directory = directory + "/";
				}

				var directoryPath = "Assets/" + directory;
				if(!Directory.Exists(directoryPath))
					Directory.CreateDirectory(directoryPath);

				var fullPath = directoryPath + fileName + ".asset";
				AssetDatabase.CreateAsset(_instance, fullPath);

				AssetDatabase.Refresh();
			}
#else
		if(_instance == null)
		{
			Debug.LogError("FAILED to load singleton with fileName: " + _attribute.fileName + ", and directory: " + _attribute.directory + ", for type " + typeof(T).ToString());
		}
#endif
		}
	}

#if UNITY_EDITOR
	[InitializeOnLoad]
	public class ScriptableObjectSingletonInitializer
	{
		static ScriptableObjectSingletonInitializer()
		{
			Initialize();
		}

		[MenuItem("Utilities/Force ScriptableObject initialization")]
		static void Initialize()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.Location.Contains("Plugins")).ToList();
			var matches = assemblies.SelectMany(
			s => s.GetTypes()).Where(p => Attribute.GetCustomAttribute(p, typeof(ScriptableObjectSingletonAttribute)) != null
			).Select(s => s).ToList();

			foreach(Type type in matches)
			{
				MethodInfo methodInfo = type.GetMethod("SetupInstance", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
				if(methodInfo != null)
					methodInfo.Invoke(null, null);
			}
		}
	}
#endif
}