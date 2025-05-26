using MyBox;
using UnityEngine;
using TMPro;

namespace SensenToolkit
{
    public class FPSTrackerUI : MonoBehaviour
    {
        private const string TEXT_FORMAT = "Ever: {0} ~ {1} FPS\nLatest: {2} ~ {3} FPS\n{4} FPS";
        private const float EVER_TRACKING_DELAY = 5f;
        // How long to snapshot lastest min/max
        private const float LATEST_SNAPSHOT_DELAY_SECS = 0.5f;

        // Latest buffer size. It defines the time window for the latest min/max
        // FPS.
        // The latest min/max FPS includes latest (DELAY * LENGTH) seconds.
        private const int LATEST_SNAPSHOT_LENGTH = 20;
        private const float WEIGHT_REDUCTION = 0.99f;

        [Tooltip("Destroy the whole canvas if not in debug build")]
        [SerializeField]
        private bool _hasExclusiveCanvas = true;
        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private TMP_Text _text;
        [SerializeField, AutoProperty(AutoPropertyMode.Parent)]
        private Canvas _ancestorCanvas;
        private float _time = 0f;
        private float _nowFps = 0f;
        private float _smoothedFps = 0f;
        private RangedFloat[] _latestSnapshots = new RangedFloat[LATEST_SNAPSHOT_LENGTH];
        private int _snapshotInx = 0;
        private float _latestMinFps = Mathf.Infinity;
        private float _latestMaxFps = -Mathf.Infinity;
        private float _snapshotMinFps = Mathf.Infinity;
        private float _snapshotMaxFps = -Mathf.Infinity;
        private float _snapshotTime = 0f;
        private float _everMinFps = Mathf.Infinity;
        private float _everMaxFps = -Mathf.Infinity;

#if !(SENSEN_DEBUG_BUILD || UNITY_EDITOR)
        private void Awake()
        {
            Destroy(_hasExclusiveCanvas ? _ancestorCanvas.gameObject : this.gameObject);
        }
#endif

        private void OnEnable()
        {
            for (int i = 0; i < _latestSnapshots.Length; i++)
            {
                _latestSnapshots[i] = new RangedFloat(Mathf.Infinity, -Mathf.Infinity);
            }
            _latestMinFps = Mathf.Infinity;
            _latestMaxFps = -Mathf.Infinity;
            _snapshotMinFps = Mathf.Infinity;
            _snapshotMaxFps = -Mathf.Infinity;
            _snapshotTime = 0f;
        }

        private void Update()
        {
            _nowFps = 1f / Time.unscaledDeltaTime;
            _time = Mathf.Min(_time + Time.unscaledDeltaTime, 1f * 24f * 60f * 60f);
            _smoothedFps = WEIGHT_REDUCTION * _smoothedFps + (1f - WEIGHT_REDUCTION) * _nowFps;

            TrackLatestMinMax();
            TrackEverMinMax();

            RefreshUI();
        }

        private void RefreshUI()
        {
            _text.text = string.Format(
                TEXT_FORMAT,
                Mathf.RoundToInt(_everMinFps),
                Mathf.RoundToInt(_everMaxFps),
                Mathf.RoundToInt(_latestMinFps),
                Mathf.RoundToInt(_latestMaxFps),
                Mathf.RoundToInt(_smoothedFps)
            );
        }

        private void TrackLatestMinMax()
        {
            _snapshotTime += Time.unscaledDeltaTime;
            _snapshotMinFps = Mathf.Min(_snapshotMinFps, _nowFps);
            _snapshotMaxFps = Mathf.Max(_snapshotMaxFps, _nowFps);
            _latestMaxFps = Mathf.Max(_latestMaxFps, _nowFps);
            _latestMinFps = Mathf.Min(_latestMinFps, _nowFps);

            if (_snapshotTime <= LATEST_SNAPSHOT_DELAY_SECS) return;

            _latestMinFps = Mathf.Infinity;
            _latestMaxFps = -Mathf.Infinity;
            for (int i = 0; i < _latestSnapshots.Length; i++)
            {
                _latestMinFps = Mathf.Min(_latestMinFps, _snapshotMinFps);
                _latestMaxFps = Mathf.Max(_latestMaxFps, _snapshotMaxFps);
            }

            _latestSnapshots[_snapshotInx] = new RangedFloat(_snapshotMinFps, _snapshotMaxFps);
            _snapshotInx = (_snapshotInx + 1) % LATEST_SNAPSHOT_LENGTH;
            _snapshotMinFps = Mathf.Infinity;
            _snapshotMaxFps = -Mathf.Infinity;
            _snapshotTime = 0f;
        }

        private void TrackEverMinMax()
        {
            if (_time <= EVER_TRACKING_DELAY) return;
            _everMinFps = Mathf.Min(_everMinFps, _nowFps);
            _everMaxFps = Mathf.Max(_everMaxFps, _nowFps);
        }
    }
}
