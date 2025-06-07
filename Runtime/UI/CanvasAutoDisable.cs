#if DOTWEEN
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class CanvasAutoDisable : MonoBehaviour
    {
        [SerializeField, AutoProperty]
        private Canvas _canvas;

        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private PanelFadable[] _allChildPanels;
        private HashSet<PanelFadable> _dominantPanels;
        private HashSet<PanelFadable> _visiblePanels = new();
        private bool HasAnyVisiblePanel => _visiblePanels.Count > 0;

        private void OnEnable()
        {
            RefreshDominantPanels();
            foreach (PanelFadable panel in _dominantPanels)
            {
                panel.OnPrepareToShow += OnPanelPrepareToShow;
                panel.OnHidden += OnPanelHidden;
            }
            ForceRefreshVisiblePanels();
            RefreshCanvasState();
        }

        private void OnPanelPrepareToShow(PanelFadable panel)
        {
            _visiblePanels.Add(panel);
            RefreshCanvasState();
        }

        private void OnPanelHidden(PanelFadable panel)
        {
            _visiblePanels.Remove(panel);
            RefreshCanvasState();
        }

        private void OnDisable()
        {
            _canvas.enabled = false;
        }

        private void RefreshCanvasState()
        {
            if (_canvas.enabled == HasAnyVisiblePanel) return;
            ForceRefreshVisiblePanels();
            _canvas.enabled = HasAnyVisiblePanel;
        }

        private void ForceRefreshVisiblePanels()
        {
            _visiblePanels.Clear();
            foreach (PanelFadable panel in _dominantPanels)
            {
                if (panel.IsVisible)
                {
                    _visiblePanels.Add(panel);
                }
            }
        }

        private void RefreshDominantPanels()
        {
            _dominantPanels = new HashSet<PanelFadable>(_allChildPanels);
            foreach (PanelFadable panel in _allChildPanels)
            {
                if (!panel.IsDominant)
                {
                    _dominantPanels.Remove(panel);
                }
            }
        }
    }
}
#endif
