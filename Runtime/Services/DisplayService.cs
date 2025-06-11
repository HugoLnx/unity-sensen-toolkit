using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace SensenToolkit
{
    public class DisplayService : APermanentSingleton<DisplayService>
    {
        private WaitForSeconds _delayBetweenDisplayUpdates = new(3f);

        private DisplayInfo? _mainDisplay;
        private List<DisplayInfo> _displays = new();
        private List<DisplayInfo> _bufferDisplays = new();

        public DisplayInfo MainDisplay => _mainDisplay ??= UpdateMainDisplay();
        public IReadOnlyList<DisplayInfo> AllDisplays => _displays ??= UpdateDisplays();

        public event Action<List<DisplayInfo>> OnDisplaysChanged = delegate { };
        public event Action<DisplayInfo> OnMainDisplayChanged = delegate { };

        private void OnEnable()
        {
            StartCoroutine(DisplaysTrackingLoop());
        }

        public void EnsureGameOnDisplay(DisplayInfo display)
        {
            UpdateMainDisplay();
            UpdateDisplays();
            if (display.Equals(MainDisplay)) return;

            Resolution windowSize = ScreenService.WindowSize;
            Vector2Int screenCenterPosition = new(
                x: display.width / 2 - windowSize.width / 2,
                y: display.height / 2 - windowSize.height / 2
            );
            Screen.MoveMainWindowTo(display, screenCenterPosition);
        }

        public bool TryGetDisplayByName(string value, out DisplayInfo display)
        {
            UpdateDisplays();
            foreach (DisplayInfo d in AllDisplays)
            {
                if (d.name.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    display = d;
                    return true;
                }
            }

            display = default;
            return false;
        }

        public DisplayInfo GetUpdatedMainDisplay()
        {
            UpdateMainDisplay();
            return MainDisplay;
        }

        public IReadOnlyList<DisplayInfo> GetUpdatedDisplays()
        {
            UpdateDisplays();
            return AllDisplays;
        }

        private IEnumerator DisplaysTrackingLoop()
        {
            while (true)
            {
                Screen.GetDisplayLayout(_bufferDisplays);

                bool displaysChanged = false;
                bool mainDisplayChanged = false;
                if (!IsSameDisplays(_bufferDisplays, _displays))
                {
                    UpdateDisplays();
                    displaysChanged = true;
                }

                if (!_mainDisplay.Equals(Screen.mainWindowDisplayInfo))
                {
                    UpdateMainDisplay();
                    mainDisplayChanged = true;
                }

                if (displaysChanged) OnDisplaysChanged.Invoke(_displays);
                if (mainDisplayChanged) OnMainDisplayChanged.Invoke(_mainDisplay.Value);

                yield return _delayBetweenDisplayUpdates;
            }
        }

        private bool IsSameDisplays(List<DisplayInfo> displaysA, List<DisplayInfo> displaysB)
        {
            if (displaysA == null || displaysB == null) return false;
            if (displaysA.Count != displaysB.Count) return false;
            for (int i = 0; i < displaysA.Count; i++)
            {
                if (!displaysA[i].Equals(displaysB[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private DisplayInfo UpdateMainDisplay()
        {
            _mainDisplay = Screen.mainWindowDisplayInfo;
            return _mainDisplay.Value;
        }

        private List<DisplayInfo> UpdateDisplays()
        {
            _displays.Clear();
            Screen.GetDisplayLayout(_displays);
            return _displays;
        }

        public static string GenerateDisplaysSummary()
        {
            List<DisplayInfo> displayInfos = ListPool<DisplayInfo>.Get();
            Screen.GetDisplayLayout(displayInfos);
            string desc = $"\n\n## Screen.GetDisplayLayout - ({displayInfos.Count}) ##\n";
            for (int i = 0; i < displayInfos.Count; i++)
            {
                DisplayInfo info = displayInfos[i];
                desc += $"  [{i}] {info.name} {info.refreshRate.value:F2}Hz {info.width}x{info.height} {info.GetHashCode()}\n";
            }

            desc += $"\n\nScreen.currentResolution: {Screen.currentResolution}\n";
            desc += $"\n\nDisplay.main.rendering: {Display.main.renderingWidth}x{Display.main.renderingHeight}\n";
            return desc;
        }

        private static string DisplaySummary(Display display)
        {
            return $"{display.systemWidth}x{display.systemHeight} {display.renderingWidth}x{display.renderingHeight} {display.GetHashCode()} {(display.active ? "[ACTIVE]" : "")} {display}\n";
        }
    }
}
