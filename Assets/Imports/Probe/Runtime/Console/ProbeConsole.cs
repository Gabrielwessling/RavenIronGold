using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Probe
{
    /// <summary>
    /// A zero-setup in-game debug console. Captures Unity logs and runs
    /// [DebugCommand] commands. The whole UI is built from code — no prefab to
    /// wire up. It auto-creates itself at startup; toggle with the backquote (`)
    /// key, the on-screen button, or <see cref="Toggle"/>.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class ProbeConsole : MonoBehaviour
    {
        public static ProbeConsole Instance { get; private set; }

        [Tooltip("Key that shows/hides the console (legacy Input Manager).")]
        public KeyCode toggleKey = KeyCode.BackQuote;

        [Tooltip("Max number of lines kept in the log view.")]
        public int maxLines = 200;

        const int CanvasSortOrder = 30000;

        sealed class Row { public Text Label; public LogType Type; public string Message; }

        bool _visible;
        GameObject _panel;
        RectTransform _content;
        ScrollRect _scroll;
        InputField _input;
        readonly List<Row> _rows = new List<Row>();

        readonly List<string> _history = new List<string>();
        int _historyIndex;   // == _history.Count means "fresh empty line"

        // Log filtering
        InputField _search;
        Button _logBtn, _warnBtn, _errBtn;
        string _searchText = string.Empty;
        bool _showLog = true, _showWarn = true, _showErr = true;

        bool _scrollPending;   // coalesce scroll-to-bottom into one update per frame

        // ---- Lifecycle ---------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetState()
        {
            Instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (!ProbeBoot.AutoStartEnabled) return;   // not in release builds by default
            if (Instance != null) return;
            var go = new GameObject("[ProbeConsole]");
            DontDestroyOnLoad(go);
            go.AddComponent<ProbeConsole>();
        }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            CommandRegistry.EnsureBuilt();
            BuildUI();
            SetVisible(false);

            Application.logMessageReceived += OnLog;
            AppendLine("Probe console ready. Type 'help' for commands.", ProbeTheme.Info);
        }

        void Start()
        {
            // Scene EventSystems are available after BeforeSceneLoad bootstrap completes.
            ProbeUI.EnsureEventSystem();
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= OnLog;
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (ToggleKeyPressed()) { Toggle(); return; }
            if (!_visible) return;

            if (KeyPressed(KeyCode.UpArrow)) NavigateHistory(-1);
            else if (KeyPressed(KeyCode.DownArrow)) NavigateHistory(+1);
            else if (KeyPressed(KeyCode.Tab)) AutoComplete();
        }

        // Reads the toggle key under whichever input backend the project uses.
        bool ToggleKeyPressed() => KeyPressed(toggleKey);

        // Was a key pressed this frame, under whichever input backend is active?
        bool KeyPressed(KeyCode legacyKey)
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                var k = ToKey(legacyKey);
                if (k != UnityEngine.InputSystem.Key.None && kb[k].wasPressedThisFrame) return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(legacyKey)) return true;
#endif
            return false;
        }

#if ENABLE_INPUT_SYSTEM
        // Maps a KeyCode to the new Input System's Key enum. Most names match
        // (F1, A, Tab, BackQuote, UpArrow…), so parse by name first, then handle
        // the few that differ. This lets users pick any sensible toggle key.
        static UnityEngine.InputSystem.Key ToKey(KeyCode code)
        {
            if (System.Enum.TryParse<UnityEngine.InputSystem.Key>(code.ToString(), ignoreCase: true, out var key))
                return key;

            switch (code) // names that don't line up
            {
                case KeyCode.Return: return UnityEngine.InputSystem.Key.Enter;
                case KeyCode.Alpha0: return UnityEngine.InputSystem.Key.Digit0;
                case KeyCode.Alpha1: return UnityEngine.InputSystem.Key.Digit1;
                case KeyCode.Alpha2: return UnityEngine.InputSystem.Key.Digit2;
                case KeyCode.Alpha3: return UnityEngine.InputSystem.Key.Digit3;
                case KeyCode.Alpha4: return UnityEngine.InputSystem.Key.Digit4;
                case KeyCode.Alpha5: return UnityEngine.InputSystem.Key.Digit5;
                case KeyCode.Alpha6: return UnityEngine.InputSystem.Key.Digit6;
                case KeyCode.Alpha7: return UnityEngine.InputSystem.Key.Digit7;
                case KeyCode.Alpha8: return UnityEngine.InputSystem.Key.Digit8;
                case KeyCode.Alpha9: return UnityEngine.InputSystem.Key.Digit9;
                default:             return UnityEngine.InputSystem.Key.None;
            }
        }
#endif

        // ---- Public API --------------------------------------------------

        public bool IsVisible => _visible;

        public void Toggle() => SetVisible(!_visible);

        public void SetVisible(bool visible)
        {
            _visible = visible;
            if (_panel != null) _panel.SetActive(visible);
            if (visible && _input != null)
            {
                _input.text = string.Empty;
                _historyIndex = _history.Count;
                _input.ActivateInputField();
                ScrollToBottom();
            }
        }

        // ---- Logging -----------------------------------------------------

        void OnLog(string message, string stackTrace, LogType type)
            => AppendLine(message, ProbeTheme.ForLog(type), type);

        void AppendLine(string text, Color color, LogType type = LogType.Log)
        {
            if (_content == null) return;

            var go = ProbeUI.Create(_content, "Line");
            var label = ProbeUI.Text(go, text, color);
            var row = new Row { Label = label, Type = type, Message = text };
            _rows.Add(row);
            go.SetActive(PassesFilter(row));

            if (_rows.Count > maxLines)
            {
                Destroy(_rows[0].Label.gameObject);
                _rows.RemoveAt(0);
            }
            ScrollToBottom();
        }

        // ---- Log filtering -----------------------------------------------

        bool PassesFilter(Row r)
        {
            bool typeOk;
            if (r.Type == LogType.Warning) typeOk = _showWarn;
            else if (r.Type == LogType.Error || r.Type == LogType.Exception || r.Type == LogType.Assert) typeOk = _showErr;
            else typeOk = _showLog;

            if (!typeOk) return false;
            if (!string.IsNullOrEmpty(_searchText) &&
                r.Message.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) < 0) return false;
            return true;
        }

        void ApplyFilter()
        {
            foreach (var r in _rows)
                if (r.Label != null) r.Label.gameObject.SetActive(PassesFilter(r));
            ScrollToBottom();
        }

        void Submit(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return;

            AppendLine($"> {raw}", ProbeTheme.Info);
            var output = CommandParser.Execute(raw);
            if (!string.IsNullOrEmpty(output)) AppendLine(output, ProbeTheme.Normal);

            // Remember it for ↑/↓ recall (skip immediate duplicates).
            if (_history.Count == 0 || _history[_history.Count - 1] != raw) _history.Add(raw);
            _historyIndex = _history.Count;

            _input.text = string.Empty;
            _input.ActivateInputField();
        }

        // ---- History & autocomplete --------------------------------------

        void NavigateHistory(int dir)
        {
            if (_history.Count == 0) return;
            _historyIndex = Mathf.Clamp(_historyIndex + dir, 0, _history.Count);
            SetInput(_historyIndex >= _history.Count ? string.Empty : _history[_historyIndex]);
        }

        void AutoComplete()
        {
            var text = _input.text;
            if (string.IsNullOrEmpty(text) || text.Contains(" ")) return; // only the command word

            var matches = new List<string>();
            foreach (var name in CommandRegistry.Commands.Keys)
                if (name.StartsWith(text, StringComparison.OrdinalIgnoreCase)) matches.Add(name);

            if (matches.Count == 0) return;
            if (matches.Count == 1) { SetInput(matches[0] + " "); return; }

            matches.Sort(StringComparer.OrdinalIgnoreCase);
            var common = LongestCommonPrefix(matches);
            if (common.Length > text.Length) SetInput(common);
            AppendLine(string.Join("   ", matches), ProbeTheme.Info);
        }

        // Replace the input text and put the caret at the end.
        void SetInput(string value)
        {
            _input.text = value;
            _input.caretPosition = value.Length;
            _input.MoveTextEnd(false);
            _input.ActivateInputField();
        }

        static string LongestCommonPrefix(List<string> items)
        {
            var prefix = items[0];
            for (int i = 1; i < items.Count; i++)
            {
                int n = Mathf.Min(prefix.Length, items[i].Length);
                int j = 0;
                while (j < n && char.ToLowerInvariant(prefix[j]) == char.ToLowerInvariant(items[i][j])) j++;
                prefix = prefix.Substring(0, j);
                if (prefix.Length == 0) break;
            }
            return prefix;
        }

        // Request a scroll-to-bottom; the actual (expensive) canvas update is
        // batched once per frame in LateUpdate, and skipped while hidden.
        void ScrollToBottom() => _scrollPending = true;

        void LateUpdate()
        {
            if (!_scrollPending || !_visible || _scroll == null) return;
            _scrollPending = false;
            Canvas.ForceUpdateCanvases();
            _scroll.verticalNormalizedPosition = 0f;
        }

        // ---- UI construction --------------------------------------------

        void BuildUI()
        {
            // Canvas
            var canvasGO = ProbeUI.Create(transform, "Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CanvasSortOrder;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Panel (top 55% of the screen)
            _panel = ProbeUI.Create(canvasGO.transform, "Panel");
            var panelRT = ProbeUI.Rect(_panel);
            panelRT.anchorMin = new Vector2(0, 0.45f);
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            ProbeUI.Image(_panel, ProbeTheme.Panel);

            BuildFilterBar(_panel.transform);
            BuildLogView(_panel.transform);
            BuildInput(_panel.transform);
            BuildToggleButton(canvasGO.transform);
        }

        void BuildLogView(Transform parent)
        {
            var scrollGO = ProbeUI.Create(parent, "Scroll");
            ProbeUI.Fill(ProbeUI.Rect(scrollGO), 12, 56, 12, 52); // top clears the filter bar
            _scroll = scrollGO.AddComponent<ScrollRect>();
            _scroll.horizontal = false;
            _scroll.scrollSensitivity = 24;
            ProbeUI.Image(scrollGO, new Color(0, 0, 0, 0.001f)); // catches scroll drags
            scrollGO.AddComponent<RectMask2D>();

            var content = ProbeUI.Create(scrollGO.transform, "Content");
            var contentRT = ProbeUI.Rect(content);
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = Vector2.one;
            contentRT.pivot = new Vector2(0.5f, 1f);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = Vector2.zero;

            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 2f;
            vlg.padding = new RectOffset(4, 4, 4, 4);

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _scroll.content = contentRT;
            _content = contentRT;
        }

        void BuildInput(Transform parent)
        {
            var inputGO = ProbeUI.Create(parent, "Input");
            var inputRT = ProbeUI.Rect(inputGO);
            inputRT.anchorMin = new Vector2(0, 0);
            inputRT.anchorMax = new Vector2(1, 0);
            inputRT.pivot = new Vector2(0.5f, 0);
            inputRT.offsetMin = new Vector2(12, 12);
            inputRT.offsetMax = new Vector2(-12, 44);
            ProbeUI.Image(inputGO, ProbeTheme.InputField);

            _input = inputGO.AddComponent<InputField>();
            _input.lineType = InputField.LineType.SingleLine;
            // Stop Tab / arrows from moving UI focus — Probe uses them itself.
            var nav = _input.navigation;
            nav.mode = Navigation.Mode.None;
            _input.navigation = nav;

            var phGO = ProbeUI.Create(inputGO.transform, "Placeholder");
            ProbeUI.Fill(ProbeUI.Rect(phGO), 10, 0, 10, 0);
            var placeholder = ProbeUI.Text(phGO, "Type a command…   (Tab = autocomplete · ↑↓ = history)", ProbeTheme.Placeholder, TextAnchor.MiddleLeft);

            var textGO = ProbeUI.Create(inputGO.transform, "Text");
            ProbeUI.Fill(ProbeUI.Rect(textGO), 10, 0, 10, 0);
            var text = ProbeUI.Text(textGO, string.Empty, ProbeTheme.InputText, TextAnchor.MiddleLeft);

            _input.textComponent = text;
            _input.placeholder = placeholder;
            _input.onSubmit.AddListener(Submit);
        }

        void BuildToggleButton(Transform parent)
        {
            var btnGO = ProbeUI.Create(parent, "ToggleButton");
            var rt = ProbeUI.Rect(btnGO);
            rt.anchorMin = Vector2.one;
            rt.anchorMax = Vector2.one;
            rt.pivot = Vector2.one;
            rt.anchoredPosition = new Vector2(-20, -20);
            rt.sizeDelta = new Vector2(72, 72);

            var img = ProbeUI.Image(btnGO, ProbeTheme.Accent);
            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(Toggle);

            var labelGO = ProbeUI.Create(btnGO.transform, "Label");
            ProbeUI.Fill(ProbeUI.Rect(labelGO));
            ProbeUI.Text(labelGO, "</>", Color.white, TextAnchor.MiddleCenter, 28, bold: true);
        }

        // Top bar: a search field + Log / Warn / Err severity toggles.
        void BuildFilterBar(Transform parent)
        {
            var bar = ProbeUI.Create(parent, "FilterBar");
            var barRT = ProbeUI.Rect(bar);
            barRT.anchorMin = new Vector2(0, 1);
            barRT.anchorMax = new Vector2(1, 1);
            barRT.pivot = new Vector2(0.5f, 1);
            barRT.offsetMin = new Vector2(12, -44);
            barRT.offsetMax = new Vector2(-100, -10); // leave room for the </> button

            var hlg = bar.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Search field (takes the remaining width)
            var searchGO = ProbeUI.Create(bar.transform, "Search");
            searchGO.AddComponent<LayoutElement>().flexibleWidth = 1;
            ProbeUI.Image(searchGO, ProbeTheme.InputField);

            _search = searchGO.AddComponent<InputField>();
            _search.lineType = InputField.LineType.SingleLine;
            var snav = _search.navigation; snav.mode = Navigation.Mode.None; _search.navigation = snav;

            var sphGO = ProbeUI.Create(searchGO.transform, "Placeholder");
            ProbeUI.Fill(ProbeUI.Rect(sphGO), 10, 0, 10, 0);
            var sph = ProbeUI.Text(sphGO, "Search logs…", ProbeTheme.Placeholder, TextAnchor.MiddleLeft, 18);

            var stGO = ProbeUI.Create(searchGO.transform, "Text");
            ProbeUI.Fill(ProbeUI.Rect(stGO), 10, 0, 10, 0);
            var st = ProbeUI.Text(stGO, string.Empty, ProbeTheme.InputText, TextAnchor.MiddleLeft, 18);

            _search.textComponent = st;
            _search.placeholder = sph;
            _search.onValueChanged.AddListener(v => { _searchText = v; ApplyFilter(); });

            // Severity toggles
            _logBtn  = BuildFilterToggle(bar.transform, "Log",  () => { _showLog  = !_showLog;  UpdateToggle(_logBtn,  _showLog);  ApplyFilter(); });
            _warnBtn = BuildFilterToggle(bar.transform, "Warn", () => { _showWarn = !_showWarn; UpdateToggle(_warnBtn, _showWarn); ApplyFilter(); });
            _errBtn  = BuildFilterToggle(bar.transform, "Err",  () => { _showErr  = !_showErr;  UpdateToggle(_errBtn,  _showErr);  ApplyFilter(); });

            UpdateToggle(_logBtn, _showLog);
            UpdateToggle(_warnBtn, _showWarn);
            UpdateToggle(_errBtn, _showErr);
        }

        Button BuildFilterToggle(Transform parent, string label, System.Action onClick)
        {
            var go = ProbeUI.Create(parent, "Toggle_" + label);
            go.AddComponent<LayoutElement>().preferredWidth = 58;
            var img = ProbeUI.Image(go, ProbeTheme.Button);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick());

            var lblGO = ProbeUI.Create(go.transform, "Label");
            ProbeUI.Fill(ProbeUI.Rect(lblGO));
            ProbeUI.Text(lblGO, label, ProbeTheme.Normal, TextAnchor.MiddleCenter, 16);
            return btn;
        }

        // Active = accent color, inactive = dim, so the on/off state is obvious.
        static void UpdateToggle(Button btn, bool on)
        {
            if (btn != null && btn.targetGraphic is Image img)
                img.color = on ? ProbeTheme.Accent : ProbeTheme.Button;
        }
    }
}
