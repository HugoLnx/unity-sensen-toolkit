using System;
using System.Linq;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace SensenToolkit
{
    public class ScreenService : APermanentSingleton<ScreenService>
    {
        [SerializeField] private bool _setModeManually = false;

        [ConditionalField(nameof(_setModeManually))]
        [SerializeField] private FullScreenMode _forcedMode;

        [SerializeField] private bool _setResolutionManually = false;

        [ConditionalField(nameof(_setResolutionManually))]
        [SerializeField] private int _forcedWidth = 1024;
        [ConditionalField(nameof(_setResolutionManually))]
        [SerializeField] private int _forcedHeight = 768;

        private readonly Resolution[] _defaultResolutions = new Resolution[] {
            new(){width=7680, height=4320},
            new(){width=3840, height=2160},
            new(){width=2560, height=1440},
            new(){width=2560, height=1080},
            new(){width=1920, height=1080},
            new(){width=1600, height=900},
            new(){width=1536, height=864},
            new(){width=1366, height=768},
            new(){width=1440, height=900},
            new(){width=1280, height=720},
            new(){width=1024, height=768},
            new(){width=800, height=600},
        };

        [NonSerialized] private Resolution[] _resolutions;
        public Resolution[] Resolutions => _resolutions ??= BuildResolutions();
        [NonSerialized] private string[] _resolutionKeys;
        public string[] ResolutionKeys => _resolutionKeys ??= BuildResolutionsKeys();
        public static Resolution WindowSize => new()
        {
            width = Display.main.renderingWidth,
            height = Display.main.renderingHeight,
        };

        public bool IsFullscreen => Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen
                                   || Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
        public static Resolution FullscreenResolution => new()
        {
            width = Display.main.systemWidth,
            height = Display.main.systemHeight,
        };

        private void Start()
        {
            Resolution? resolution = _setResolutionManually
                ? new Resolution { width = _forcedWidth, height = _forcedHeight }
                : null;
            FullScreenMode? mode = _setModeManually ? _forcedMode : null;
            Enforce(resolution, mode);
        }

        public void Enforce(Vector2Int resolution, FullScreenMode? mode = null)
            => Enforce(
                new Resolution { width = resolution.x, height = resolution.y },
                mode);

        public void Enforce(Resolution? resolution = null, FullScreenMode? mode = null)
        {
            Resolution res = resolution ?? WindowSize;
            FullScreenMode screenMode = mode ?? Screen.fullScreenMode;
            Screen.SetResolution(res.width, res.height, screenMode);
        }

        public string[] GetUpdatedResolutionKeys()
        {
            _resolutions = BuildResolutions();
            _resolutionKeys = BuildResolutionsKeys();
            return _resolutionKeys;
        }

        public Resolution[] GetUpdatedResolutions()
        {
            _resolutions = BuildResolutions();
            return _resolutions;
        }

        public void EnforceFullscreenResolution()
        {
            Enforce(FullscreenResolution);
        }

        public static FullScreenMode ScreenModeFromKey(string key)
        {
            return key switch
            {
                "windowed" => FullScreenMode.Windowed,
                "maximized-window" => FullScreenMode.MaximizedWindow,
                "fullscreen-window" => FullScreenMode.FullScreenWindow,
                "exclusive-fullscreen" => FullScreenMode.ExclusiveFullScreen,
                _ => throw new System.ArgumentException($"Unknown FullScreenMode key: {key}")
            };
        }

        public static string ScreenModeToKey(FullScreenMode mode)
        {
            return mode switch
            {
                FullScreenMode.Windowed => "windowed",
                FullScreenMode.MaximizedWindow => "maximized-window",
                FullScreenMode.FullScreenWindow => "fullscreen-window",
                FullScreenMode.ExclusiveFullScreen => "exclusive-fullscreen",
                _ => throw new System.ArgumentException($"Unknown FullScreenMode: {mode}")
            };
        }

        private Resolution[] BuildResolutions()
        {
            return Screen.resolutions
                .Concat(_defaultResolutions)
                .OrderByDescending(OrderByWidthAndArea)
                .Distinct()
                .ToArray();
        }

        private string[] BuildResolutionsKeys()
        {
            return Resolutions
                .Select(res => GetResolutionKey(res))
                .Distinct()
                .ToArray();
        }

        public static string GetResolutionKey(Resolution res) => $"{res.width}x{res.height}";

        private static object OrderByWidthAndArea(Resolution res)
            => ((ulong)res.width * 100000000) + ((ulong)res.height * (ulong)res.width);
    }
}
