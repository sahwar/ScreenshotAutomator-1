using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//Based on http://answers.unity3d.com/questions/179775/game-window-size-from-editor-window-in-editor-mode.html
public class SetGameViewSize: EditorWindow
{
	enum ScreenshotSize
	{
		Small,
		Medium,
		Large,
	}
	
	Dictionary<ScreenshotSize, Vector2> nameToSize = new Dictionary<ScreenshotSize, Vector2>()
	{
		{ ScreenshotSize.Small,	new Vector2(960, 640) },
		{ ScreenshotSize.Medium,	new Vector2(1136, 640) },
		{ ScreenshotSize.Large,	new Vector2(2048, 1536) },
	};
	ScreenshotSize size;
	
	private int		_superSizeScale = 1;
	private bool    _invert 		= false;
	private Vector2 _size 			= new Vector2 (800, 600);

	[MenuItem("Window/Set Game Window Size", false, 1000)]
	static void Init ()
	{
		EditorWindow.GetWindow(typeof(SetGameViewSize));
	}

	void OnGUI ()
	{
        var gameView = UtilitiesScreenshot.GetMainGameView();
		if(gameView == null)
			return;
		
		Vector2 viewRect = new Vector2(gameView.position.width, gameView.position.height - 17);
		
		bool matches = true;
		if (viewRect.y != _size.y || viewRect.x != _size.x)
		{
			matches = false;
		}
		
		GUI.color = matches ? Color.white : Color.red;
		
		_size.x = EditorGUILayout.IntField ("X (" + viewRect.x  + ")", (int)_size.x);
		_size.y = EditorGUILayout.IntField ("Y (" + viewRect.y + ")", (int)_size.y);
		
		GUI.color = Color.white;
		
		_invert = EditorGUILayout.Toggle("Invert", _invert);
		_superSizeScale = EditorGUILayout.IntSlider("Supersize", _superSizeScale, 1, 4);
		
		GUILayout.Space(10.0f);
		ScreenshotSize newSize = (ScreenshotSize)EditorGUILayout.EnumPopup("iOS screenshot size", size);
		if(newSize != size)
		{
			size = newSize;
			Vector2 desiredSize = nameToSize[newSize];
			if(_invert)
			{
				Vector2 invert = desiredSize;
				desiredSize.x = invert.y;
				desiredSize.y = invert.x;
			}
			
			desiredSize.x /= (float)_superSizeScale;
			desiredSize.y /= (float)_superSizeScale;
			
			_size = desiredSize;
			
			UtilitiesScreenshot.SetGameViewSize(_size);
            Repaint();
		}
		GUILayout.Space(10.0f);
		
		GUI.color = Color.white;
		
		if (GUILayout.Button ("Set"))
        {
			UtilitiesScreenshot.SetGameViewSize(_size);
            Repaint();
		}
		
		if(GUILayout.Button ("Screenshot"))
			TakeScreenshot();
		
		GUILayout.Label("If this doesn't work, try turning on Stats in the GameView and dragging the window until it's the size you want.");
		
	}
	
	void TakeScreenshot()
	{
		//TODO: turn off debug info, possibly HUD
		
		System.IO.Directory.CreateDirectory("Screenshots");
		Application.CaptureScreenshot(
			"Screenshots/" + Application.loadedLevelName + "_" +
			(_size.x * _superSizeScale) + "x" +
			(_size.y * _superSizeScale) + "_" +
			System.DateTime.Now.Ticks + ".png", _superSizeScale
		);
	}
	
}