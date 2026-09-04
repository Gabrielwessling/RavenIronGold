using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Probe
{
    /// <summary>Tiny helpers for building uGUI from code. Shared by the console and panels.</summary>
    static class ProbeUI
    {
        static Font _font;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetState()
        {
            _font = null;
        }

        /// <summary>Unity's built-in runtime font (name differs across versions).</summary>
        public static Font Font
        {
            get
            {
                if (_font == null)
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                         ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                return _font;
            }
        }

        public static GameObject Create(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        public static RectTransform Rect(GameObject go) => (RectTransform)go.transform;

        /// <summary>Stretch a RectTransform to fill its parent, with optional padding.</summary>
        public static RectTransform Fill(RectTransform rt, float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        public static Image Image(GameObject go, Color color)
        {
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static Text Text(GameObject go, string value, Color color,
            TextAnchor anchor = TextAnchor.UpperLeft, int size = ProbeTheme.FontSize,
            bool bold = false)
        {
            var t = go.AddComponent<Text>();
            t.font = Font;
            t.fontSize = size;
            t.color = color;
            t.alignment = anchor;
            t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            t.supportRichText = false; // Probe colors each line itself; '<>' must show literally
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.text = value;
            return t;
        }

        /// <summary>A compact text button with a fixed height, sized for layout groups.</summary>
        public static Button Button(Transform parent, string label, UnityAction onClick)
        {
            var go = Create(parent, "Button");
            var le = go.AddComponent<LayoutElement>();
            le.minWidth = 120;
            le.preferredHeight = 34;

            var img = Image(go, ProbeTheme.Button);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var labelGO = Create(go.transform, "Label");
            Fill(Rect(labelGO));
            Text(labelGO, label, ProbeTheme.Normal, TextAnchor.MiddleCenter, 18);
            return btn;
        }

        /// <summary>A horizontal uGUI slider built from code (mirrors Unity's default hierarchy).</summary>
        public static Slider Slider(Transform parent, float min, float max, float value,
            bool wholeNumbers, UnityAction<float> onChanged)
        {
            var root = Create(parent, "Slider");

            var bg = Create(root.transform, "Background");
            var bgRT = Rect(bg);
            bgRT.anchorMin = new Vector2(0, 0.25f);
            bgRT.anchorMax = new Vector2(1, 0.75f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            Image(bg, ProbeTheme.SliderTrack);

            var fillArea = Create(root.transform, "Fill Area");
            var faRT = Rect(fillArea);
            faRT.anchorMin = new Vector2(0, 0.25f);
            faRT.anchorMax = new Vector2(1, 0.75f);
            faRT.sizeDelta = new Vector2(-20, 0);
            faRT.anchoredPosition = new Vector2(-5, 0);

            var fill = Create(fillArea.transform, "Fill");
            var fillRT = Rect(fill);
            fillRT.sizeDelta = new Vector2(10, 0);
            Image(fill, ProbeTheme.SliderFill);

            var handleArea = Create(root.transform, "Handle Slide Area");
            var haRT = Rect(handleArea);
            haRT.anchorMin = new Vector2(0, 0);
            haRT.anchorMax = new Vector2(1, 1);
            haRT.sizeDelta = new Vector2(-20, 0);

            var handle = Create(handleArea.transform, "Handle");
            var handleRT = Rect(handle);
            handleRT.sizeDelta = new Vector2(16, 0);
            var handleImg = Image(handle, ProbeTheme.SliderHandle);

            var slider = root.AddComponent<Slider>();
            slider.fillRect = fillRT;
            slider.handleRect = handleRT;
            slider.targetGraphic = handleImg;
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = wholeNumbers;
            slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(onChanged);
            return slider;
        }

        /// <summary>Create an EventSystem if the scene has none (needed for UI input).</summary>
        public static void EnsureEventSystem()
        {
            if (HasEventSystem()) return;
            var es = new GameObject("EventSystem", typeof(EventSystem));
            UnityEngine.Object.DontDestroyOnLoad(es);

            // Prefer the new Input System UI module when the package is present.
            var newModule = Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (newModule != null) es.AddComponent(newModule);
            else es.AddComponent<StandaloneInputModule>();
        }

        static bool HasEventSystem()
        {
            if (EventSystem.current != null) return true;

#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null;
#else
            // Probe initializes early, sometimes before a scene EventSystem has set EventSystem.current.
            foreach (var eventSystem in Resources.FindObjectsOfTypeAll<EventSystem>())
            {
                if (eventSystem != null && eventSystem.gameObject.scene.IsValid())
                    return true;
            }
            return false;
#endif
        }
    }
}
