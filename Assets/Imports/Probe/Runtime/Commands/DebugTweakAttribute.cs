using System;

namespace Probe
{
    /// <summary>
    /// Puts a live slider on the debug panel for a numeric field or property,
    /// so you can tune values at runtime without recompiling.
    ///
    ///   [DebugTweak(0f, 20f)] public float moveSpeed = 5f;
    ///   [DebugTweak(1, 10)]   public int   enemyCount = 3;
    ///
    /// Works on float / int / double / long members. Static members are found
    /// automatically; instance members need <see cref="ProbeTweaks.RegisterInstance"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DebugTweakAttribute : Attribute
    {
        public float Min { get; }
        public float Max { get; }

        /// <summary>Slider label. Falls back to the member name when omitted.</summary>
        public string Label { get; }

        public DebugTweakAttribute(float min, float max, string label = null)
        {
            Min = min;
            Max = max;
            Label = label;
        }
    }
}
