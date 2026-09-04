using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Probe
{
    /// <summary>
    /// A lightweight on-screen debug panel: FPS, memory, and any values you
    /// watch. Auto-creates itself; visible by default (top-left corner).
    ///
    ///   ProbePanel.Watch("HP", () => player.health);
    ///   ProbePanel.Watch("State", () => enemy.currentState);
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class ProbePanel : MonoBehaviour
    {
        public static ProbePanel Instance { get; private set; }

        [Tooltip("How often (seconds) the panel refreshes.")]
        public float refreshInterval = 0.25f;

        struct Entry { public string Label; public Func<object> Getter; }

        static readonly List<Entry> _watches = new List<Entry>();

        sealed class TweakRow { public ProbeTweaks.Tweak Tweak; public Text Value; public Slider Slider; public GameObject Go; }

        GameObject _panel;
        Text _text;
        readonly StringBuilder _sb = new StringBuilder(256);
        readonly List<GameObject> _buttons = new List<GameObject>();
        readonly List<TweakRow> _tweaks = new List<TweakRow>();
        GameObject _presetRow;
        float _accum, _timer, _fps;
        int _frames;
        bool _userVisible = true;   // what the user asked for, regardless of auto-hide

        // ---- Public API --------------------------------------------------

        /// <summary>Show a live value on the panel. Call again with the same label to replace it.</summary>
        public static void Watch(string label, Func<object> getter)
        {
            EnsureInstance();
            for (int i = 0; i < _watches.Count; i++)
            {
                if (_watches[i].Label == label)
                {
                    _watches[i] = new Entry { Label = label, Getter = getter };
                    return;
                }
            }
            _watches.Add(new Entry { Label = label, Getter = getter });
        }

        public static void Unwatch(string label) => _watches.RemoveAll(w => w.Label == label);

        public static void Toggle()
        {
            if (Instance != null) Instance._userVisible = !Instance._userVisible;
        }

        public static void SetVisible(bool visible)
        {
            if (Instance != null) Instance._userVisible = visible;
        }

        // ---- Lifecycle ---------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetState()
        {
            Instance = null;
            _watches.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (!ProbeBoot.AutoStartEnabled) return;   // not in release builds by default
            EnsureInstance();
        }

        static void EnsureInstance()
        {
            if (Instance != null) return;
            var go = new GameObject("[ProbePanel]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<ProbePanel>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
        }

        void Start()
        {
            // Scene EventSystems are available after BeforeSceneLoad bootstrap completes.
            ProbeUI.EnsureEventSystem();
        }

        void OnDestroy()
        {
            ProbeActions.Changed -= RebuildButtons;
            ProbeTweaks.Changed -= RebuildTweaks;
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            // Hide while the console is open so they don't overlap.
            bool consoleOpen = ProbeConsole.Instance != null && ProbeConsole.Instance.IsVisible;
            bool show = _userVisible && !consoleOpen;
            if (_panel.activeSelf != show) _panel.SetActive(show);

            _accum += Time.unscaledDeltaTime;
            _frames++;
            _timer += Time.unscaledDeltaTime;
            if (_timer < refreshInterval) return;

            _fps = _accum > 0 ? _frames / _accum : 0;
            _accum = _timer = 0;
            _frames = 0;
            if (show) Refresh();
        }

        void Refresh()
        {
            if (_text == null || !_panel.activeSelf) return;

            const int col = 9; // align all values to the same column

            _sb.Length = 0;
            _sb.Append("FPS").Append(' ', col - 3).Append(Mathf.RoundToInt(_fps));

            long mem = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            if (mem <= 0) mem = GC.GetTotalMemory(false);
            _sb.Append("\nMEM").Append(' ', col - 3).Append((mem / (1024f * 1024f)).ToString("0.0")).Append(" MB");

            for (int i = 0; i < _watches.Count; i++)
            {
                object value;
                try { value = _watches[i].Getter?.Invoke(); }
                catch (Exception e) { value = "err: " + e.Message; }

                var label = _watches[i].Label;
                _sb.Append('\n').Append(label).Append(' ', Mathf.Max(1, col - label.Length)).Append(value);
            }

            _text.text = _sb.ToString();

            // Keep each slider's value readout in sync with the real value.
            for (int i = 0; i < _tweaks.Count; i++)
            {
                if (_tweaks[i].Value == null) continue;
                try
                {
                    float v = _tweaks[i].Tweak.Get();
                    _tweaks[i].Value.text = _tweaks[i].Tweak.IsInt ? Mathf.RoundToInt(v).ToString() : v.ToString("0.0#");
                }
                catch { _tweaks[i].Value.text = "—"; } // dead object / throwing getter — don't spam
            }
        }

        // ---- UI ----------------------------------------------------------

        void BuildUI()
        {
            var canvasGO = ProbeUI.Create(transform, "Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 29990;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            _panel = ProbeUI.Create(canvasGO.transform, "Panel");
            var rt = ProbeUI.Rect(_panel);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(16, -16);
            ProbeUI.Image(_panel, ProbeTheme.Panel);

            var vlg = _panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 8, 8);
            vlg.spacing = 6;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;   // buttons stretch to the panel width
            vlg.childForceExpandHeight = false;

            var fitter = _panel.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var textGO = ProbeUI.Create(_panel.transform, "Text");
            _text = ProbeUI.Text(textGO, "FPS\nMEM", ProbeTheme.Normal, TextAnchor.UpperLeft, 20);
            _text.horizontalOverflow = HorizontalWrapMode.Overflow; // stats shouldn't wrap

            // Build once first (this triggers the initial scan), THEN subscribe —
            // otherwise the scan's Changed event re-enters and doubles the rows.
            RebuildButtons();
            RebuildTweaks();
            ProbeActions.Changed += RebuildButtons;
            ProbeTweaks.Changed += RebuildTweaks;
        }

        // Rebuild the [DebugButton] action buttons below the watch text.
        void RebuildButtons()
        {
            if (_panel == null) return;

            foreach (var go in _buttons)
                if (go != null) Destroy(go);
            _buttons.Clear();

            foreach (var action in ProbeActions.All)
            {
                var a = action; // capture per-iteration for the closure
                var btn = ProbeUI.Button(_panel.transform, a.Label, () => a.Invoke());
                _buttons.Add(btn.gameObject);
            }
        }

        // Rebuild the [DebugTweak] slider rows (plus the preset controls row).
        void RebuildTweaks()
        {
            if (_panel == null) return;

            foreach (var row in _tweaks)
                if (row.Go != null) Destroy(row.Go);
            _tweaks.Clear();
            if (_presetRow != null) Destroy(_presetRow);

            foreach (var tweak in ProbeTweaks.All)
                _tweaks.Add(BuildTweakRow(tweak));

            // Only show Save/Load/Reset when there's something to tune.
            _presetRow = _tweaks.Count > 0 ? BuildPresetRow() : null;
        }

        // A row of three buttons that save/load/reset the sliders. The buttons use
        // a single "quick" preset slot; the console commands handle named presets.
        GameObject BuildPresetRow()
        {
            var row = ProbeUI.Create(_panel.transform, "Presets");
            row.AddComponent<LayoutElement>().preferredHeight = 30;

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            AddPresetButton(row.transform, "Save",  () => { if (ProbePresets.Save("quick")) Debug.Log("[Probe] Saved preset 'quick'."); });
            AddPresetButton(row.transform, "Load",  () => { if (ProbePresets.Load("quick")) SyncSliders(); });
            AddPresetButton(row.transform, "Reset", () => { ProbePresets.Reset(); SyncSliders(); });
            return row;
        }

        void AddPresetButton(Transform parent, string label, System.Action onClick)
        {
            var btn = ProbeUI.Button(parent, label, () => onClick());
            var le = btn.GetComponent<LayoutElement>();
            le.minWidth = 50;
            le.flexibleWidth = 1;
        }

        // Move slider handles to match the current values (after Load / Reset).
        void SyncSliders()
        {
            foreach (var row in _tweaks)
            {
                if (row.Slider == null) continue;
                try { row.Slider.SetValueWithoutNotify(Mathf.Clamp(row.Tweak.Get(), row.Tweak.Min, row.Tweak.Max)); }
                catch { }
            }
        }

        // One row:  [ label ][ ===O=== slider ][ value ]
        TweakRow BuildTweakRow(ProbeTweaks.Tweak tweak)
        {
            var t = tweak; // capture for the closure

            var row = ProbeUI.Create(_panel.transform, "Tweak");
            var rowLE = row.AddComponent<LayoutElement>();
            rowLE.preferredHeight = 24;
            rowLE.minWidth = 220;

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            var labelGO = ProbeUI.Create(row.transform, "Label");
            labelGO.AddComponent<LayoutElement>().preferredWidth = 88;
            ProbeUI.Text(labelGO, t.Label, ProbeTheme.Normal, TextAnchor.MiddleLeft, 16);

            float initial = t.Min;
            try { initial = Mathf.Clamp(t.Get(), t.Min, t.Max); } catch { }
            var slider = ProbeUI.Slider(row.transform, t.Min, t.Max, initial, t.IsInt, _ => { });
            var sliderLE = slider.gameObject.AddComponent<LayoutElement>();
            sliderLE.flexibleWidth = 1;
            sliderLE.minWidth = 110;

            var valueGO = ProbeUI.Create(row.transform, "Value");
            valueGO.AddComponent<LayoutElement>().preferredWidth = 44;
            var valueText = ProbeUI.Text(valueGO, "", ProbeTheme.Info, TextAnchor.MiddleRight, 16);

            // Write the value back when the slider moves (swallow setter errors).
            slider.onValueChanged.AddListener(v =>
            {
                try { t.Set(v); }
                catch (Exception e) { Debug.LogException(e); }
            });

            return new TweakRow { Tweak = t, Value = valueText, Slider = slider, Go = row };
        }
    }
}
