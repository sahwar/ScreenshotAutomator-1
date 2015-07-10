using UnityEngine;
using System.Collections;
using TrinketCore;

namespace ScreenshotAutomator
{
	[ScriptableObjectSingleton("ScreenshotAutomatorSettings", "Data/Resources/")]
	public class ScreenshotAutomatorSettings : ScriptableObjectSingleton<ScreenshotAutomatorSettings>
	{
		[Tooltip("UnityAPI uses Application.TakeScreenshot. Uses current Game tab resolution unless AttemptToResizeGameWindow is set to true. RenderTexture attempts to render the scene from a duplicated camera which mirrors the main game camera.")]
		public ScreenshotAutomator.Method Method;

		[Tooltip("Scaling factor applied to DesiredResolution.")]
		[Range(1f, 50f)]
		public float ResolutionScalingFactor = 1f;

		[Tooltip("The resolution for which we want the screenshot.")]
		public Vector2 DesiredResolution = new Vector2(1920f, 1080f);

		[Tooltip("List of keys that must be pressed to take a screenshot.")]
		public KeyCode[] ManualKeyStrokes = new KeyCode[] { KeyCode.J };

		[Tooltip("How often to capture an automatic screenshot.")]
		[Range(0f, 60f)]
		public float AutomaticPeriod = 10f;

		[Tooltip("Number of screenshots to take until we start overwriting old ones.")]
		[Range(1, 50)]
		public int MaxScreenshotsTillOverwrite = 5;

		[Tooltip("For ScreenshotAutomator.Method.UnityAPI, indicates that we should attempt to resize the Game tab to acheieve the desired resolution.")]
		public bool AttemptToResizeGameWindow = false;
	}
}