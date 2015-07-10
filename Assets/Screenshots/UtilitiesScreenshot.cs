using UnityEngine;
using System.Collections;
using TrinketCore;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ScreenshotAutomator
{
	public static class UtilitiesScreenshot
	{
#if UNITY_EDITOR
		static System.Reflection.MethodInfo _getMainGameViewMethod;

		static System.Reflection.MethodInfo _gameViewAspectWasChangedMethod;
		static System.Reflection.MethodInfo gameViewAspectWasChangedMethod
		{
			get
			{
				if(_gameViewAspectWasChangedMethod == null)
				{
					var type = System.Type.GetType("UnityEditor.GameView,UnityEditor");
					_gameViewAspectWasChangedMethod = type.GetMethod("GameViewAspectWasChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				}
				return _gameViewAspectWasChangedMethod;
			}
		}

		static System.Reflection.PropertyInfo _selectedSizeIndexProperty;
		static System.Reflection.PropertyInfo selectedSizeIndexProperty
		{
			get
			{
				if(_selectedSizeIndexProperty == null)
				{
					var type = System.Type.GetType("UnityEditor.GameView,UnityEditor");
					_selectedSizeIndexProperty = type.GetProperty("selectedSizeIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				}
				return _selectedSizeIndexProperty;
			}
		}

		public static EditorWindow GetMainGameView()
		{
			if(_getMainGameViewMethod == null)
			{
				var type = System.Type.GetType("UnityEditor.GameView,UnityEditor");
				_getMainGameViewMethod = type.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			}

			if(_getMainGameViewMethod != null)
			{
				var returnValue = _getMainGameViewMethod.Invoke(null, null);
				return (EditorWindow)returnValue;
			}

			return null;
		}

		public static int GetSelectedSizeIndexProperty()
		{
			if(selectedSizeIndexProperty != null && GetMainGameView())
			{
				var returnValue = selectedSizeIndexProperty.GetValue(GetMainGameView(), null);
				return (int)returnValue;
			}

			return -1;
		}

		public static void SetSelectedSizeIndexProperty(int inValue)
		{
			if(selectedSizeIndexProperty != null && GetMainGameView())
			{
				selectedSizeIndexProperty.SetValue(GetMainGameView(), inValue, null);

				if(gameViewAspectWasChangedMethod != null)
					gameViewAspectWasChangedMethod.Invoke(GetMainGameView(), null);
			}
		}

		public static Vector2 GetGameViewSize()
		{
			if(GetMainGameView() == null)
				return Vector2.one;

			return new Vector2(GetMainGameView().position.width, GetMainGameView().position.height - 17);
		}

		public static void SetGameViewSize(Vector2 inSize)
		{
			if(GetMainGameView() == null)
				return;

			var pos = GetMainGameView().position;
			pos.height = inSize.y + 17;
			pos.width = inSize.x;
			GetMainGameView().position = pos;
			GetMainGameView().Repaint();
		}
#endif
	}
}