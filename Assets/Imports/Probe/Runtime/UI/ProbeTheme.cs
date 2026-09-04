using UnityEngine;

namespace Probe
{
    /// <summary>Central place for Probe's colors and sizes. Tweak to reskin.</summary>
    static class ProbeTheme
    {
        // Surfaces
        public static readonly Color Panel = new Color(0.04f, 0.05f, 0.07f, 0.92f);
        public static readonly Color InputField = new Color(1f, 1f, 1f, 0.06f);
        public static readonly Color Accent = new Color(0.10f, 0.45f, 1f, 0.92f);
        public static readonly Color Button = new Color(0.16f, 0.18f, 0.22f, 0.96f);
        public static readonly Color SliderTrack = new Color(1f, 1f, 1f, 0.10f);
        public static readonly Color SliderFill = new Color(0.10f, 0.45f, 1f, 0.85f);
        public static readonly Color SliderHandle = new Color(0.90f, 0.92f, 0.96f, 1f);

        // Text
        public static readonly Color Info = new Color(0.48f, 0.64f, 1f);
        public static readonly Color Normal = new Color(0.84f, 0.85f, 0.88f);
        public static readonly Color Warn = new Color(1f, 0.80f, 0.33f);
        public static readonly Color Error = new Color(1f, 0.42f, 0.42f);
        public static readonly Color Placeholder = new Color(0.55f, 0.58f, 0.64f);
        public static readonly Color InputText = new Color(0.92f, 0.93f, 0.95f);

        public const int FontSize = 22;

        public static Color ForLog(LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception) return Error;
            if (type == LogType.Warning) return Warn;
            return Normal;
        }
    }
}
