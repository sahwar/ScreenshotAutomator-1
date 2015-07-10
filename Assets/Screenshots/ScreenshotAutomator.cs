using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScreenshotAutomator : AutoSingleton<ScreenshotAutomator>
{
    const string kFileNameInfoSeparator = "__";

    public enum ScreenshotType
    {
        Manual,
        Automatic,
        Triggered,
    }

    public enum Method
    {
        UnityAPI,
        RenderTexture,
    }

    /// <summary>
    /// The game camera _screenshotCamera should emulate when taling a screenshot with the RenderTexture Method.
    /// </summary>
    Camera GetTargetCamera()
    {
        return Camera.main;
    }

    RenderTexture _renderTexture;
    RenderTexture GetRenderTexture(Vector2 inSize)
    {
        if(_renderTexture == null || inSize.x != _renderTexture.width || inSize.y != _renderTexture.height)
        {
            _renderTexture = new RenderTexture((int)inSize.x, (int)inSize.y, 24);
        }
        return _renderTexture;
    }

    float _automaticTimer;

    Dictionary<ScreenshotType, int> _screenshotTypeToWriteIndex = new Dictionary<ScreenshotType, int>()
    {
        { ScreenshotType.Manual, 0 },
        { ScreenshotType.Automatic, 0 },
        { ScreenshotType.Triggered, 0 },
    };

    string GetLevelName()
    {
        //if(Cthulhu.HasInstance() && Cthulhu.GetInstance().CurrentLevel)
            //return Cthulhu.GetInstance().CurrentLevel.name;
        //else
            return Application.loadedLevelName;
    }

    void Start()
    {
        if(!Application.isEditor)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(MonitorForManualScreenshots());
            StartCoroutine(MonitorForAutomaticScreenshots());
        }
    }

    IEnumerator MonitorForManualScreenshots()
    {
        while(true)
        {
            // Wait for no keys to be pressed, then wait for all the keys specified by settings to be pressed simultaenously.
            while(Input.anyKeyDown)
                yield return null;

            while(true)
            {
                var all = true;
                foreach(var key in ScreenshotAutomatorSettings.Instance.ManualKeyStrokes)
                {
                    all &= Input.GetKey(key);
                    if(!all)
                        break;
                }

                if(all)
                    break;

                yield return null;
            }

            yield return new WaitForEndOfFrame();
            yield return StartCoroutine(TakeScreenshot(ScreenshotType.Manual));
            _automaticTimer = 0f;
        }
    }

    IEnumerator MonitorForAutomaticScreenshots()
    {
        while(true)
        {
            // Idle if the settings don't have us taking automatic screenshots.
            while(ScreenshotAutomatorSettings.Instance.AutomaticPeriod <= 0f)
                yield return null;

            float realTime = Time.realtimeSinceStartup;
            while(_automaticTimer < ScreenshotAutomatorSettings.Instance.AutomaticPeriod)
            {
                _automaticTimer += Time.realtimeSinceStartup - realTime;
                realTime = Time.realtimeSinceStartup;
                yield return null;
            }
            
            yield return new WaitForEndOfFrame();
            yield return StartCoroutine(TakeScreenshot(ScreenshotType.Automatic));
            _automaticTimer = 0f;
        }
    }

    IEnumerator TakeScreenshot(ScreenshotType inType)
    {
        // Determine directory.
        var directory = "Screenshots/" + inType.ToString();

        // Make sure the directory exists.
        Directory.CreateDirectory(directory);

        // Report level.
        var levelName = GetLevelName();
                        
        // Report time taken.
        var dateTime = DateTime.Now;
        var dateTimeFormat = "yyyy-M-d";
        var date = dateTime.ToString(dateTimeFormat);

        // Possibly append count.
        var index = "";
        if(DoesModeOverwrite(inType))
        {
            var count = 0;

            var existingFiles = Directory.GetFiles(directory);
            count = existingFiles.Length;

            var max = ScreenshotAutomatorSettings.Instance.MaxScreenshotsTillOverwrite;

            if(count >= max)
            {
                count = _screenshotTypeToWriteIndex[inType];

                for(int i = 0; i < existingFiles.Length; ++i)
                {
                    if(Path.GetFileName(existingFiles[i]).StartsWith(count.ToString()))
                    {
                        File.Delete(existingFiles[i]);
                        break;
                    }
                }

                _screenshotTypeToWriteIndex[inType] = (count + 1) % max;
            }

            index = count.ToString();
        }
        
        // Total supersizing.
        var superSize = (int)(ScreenshotAutomatorSettings.Instance.ResolutionScalingFactor);

        // Include dimensions.
        var screenSize = ScreenshotAutomatorSettings.Instance.Method == Method.UnityAPI ? GetScreenSize() : GetDesiredScreenshotSize();
        var dimensions = (screenSize.x * superSize) + "x" + (screenSize.y * superSize);
        
        // Final filename.
        var filename = (!string.IsNullOrEmpty(index) ? index : string.Empty) + kFileNameInfoSeparator + levelName + kFileNameInfoSeparator + dimensions + kFileNameInfoSeparator + date + ".png";
        var fullPath = directory + "/" + filename;

        if(ScreenshotAutomatorSettings.Instance.Method == Method.UnityAPI)
        {
            Vector2 startSize = Vector2.one; int startSelectedSizeIndex = 0;

            if(ScreenshotAutomatorSettings.Instance.AttemptToResizeGameWindow)
            {
    #if UNITY_EDITOR
                startSize = UtilitiesScreenshot.GetGameViewSize();
                startSelectedSizeIndex = UtilitiesScreenshot.GetSelectedSizeIndexProperty();
                UtilitiesScreenshot.SetSelectedSizeIndexProperty(0);
                UtilitiesScreenshot.SetGameViewSize(ScreenshotAutomatorSettings.Instance.DesiredResolution);
    #endif
            }

            Application.CaptureScreenshot(fullPath, Mathf.RoundToInt(ScreenshotAutomatorSettings.Instance.ResolutionScalingFactor));

            if(ScreenshotAutomatorSettings.Instance.AttemptToResizeGameWindow)
            {
    #if UNITY_EDITOR
                UtilitiesScreenshot.SetSelectedSizeIndexProperty(startSelectedSizeIndex);
                UtilitiesScreenshot.SetGameViewSize(startSize);
    #endif
            }
        }
        else
        {
            if(GetTargetCamera())
            {
                var previousRenderTexture = GetTargetCamera().targetTexture;
                RenderTexture.active = GetRenderTexture(GetDesiredScreenshotSize());
                GetTargetCamera().targetTexture = GetRenderTexture(GetDesiredScreenshotSize());
                GetTargetCamera().Render();

                var w = (int)(GetDesiredScreenshotSize().x * ScreenshotAutomatorSettings.Instance.ResolutionScalingFactor);
                var h = (int)(GetDesiredScreenshotSize().y * ScreenshotAutomatorSettings.Instance.ResolutionScalingFactor);
                Texture2D screenshot = new Texture2D(w, h);
                yield return new WaitForEndOfFrame();
                screenshot.ReadPixels(new Rect( 0, 0, w, h), 0, 0);
                screenshot.Apply();

                RenderTexture.active = null;
                GetTargetCamera().targetTexture = previousRenderTexture;

                byte[] bytes = screenshot.EncodeToPNG();
                File.WriteAllBytes(fullPath, bytes);
            }
        }
    }

    static Vector2 GetScreenSize()
    {
        Vector2 size = new Vector2(Screen.width, Screen.height);
        size.x = Mathf.Max(size.x, 1f);
        size.y = Mathf.Max(size.y, 1f);
        return size;
    }

    static Vector2 GetDesiredScreenshotSize()
    {
        return ScreenshotAutomatorSettings.Instance.DesiredResolution;
    }

    static bool DoesModeOverwrite(ScreenshotType inType)
    { 
        return inType == ScreenshotType.Automatic || inType == ScreenshotType.Triggered;
    }
}