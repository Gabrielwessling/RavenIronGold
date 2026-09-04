using System;

namespace Probe
{
    /// <summary>
    /// Marks a parameterless method as a one-tap debug action. A button appears
    /// on the debug panel; tapping it runs the method — no typing needed, which
    /// is perfect for on-device / mobile QA.
    ///
    /// Static methods are discovered automatically. Instance methods need the
    /// live object registered with <see cref="ProbeActions.RegisterInstance"/>.
    ///
    /// Example:
    ///   [DebugButton("Add 100 Gold")]
    ///   static void AddGold() => Player.Gold += 100;
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class DebugButtonAttribute : Attribute
    {
        /// <summary>Button label. Falls back to the method name when omitted.</summary>
        public string Label { get; }

        public DebugButtonAttribute(string label = null) => Label = label;
    }
}
