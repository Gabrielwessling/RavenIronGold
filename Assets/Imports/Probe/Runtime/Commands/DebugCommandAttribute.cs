using System;

namespace Probe
{
    /// <summary>
    /// Marks a method as a debug command runnable from the Probe console.
    /// Works on static methods (auto-discovered at startup) and on instance
    /// methods (register the instance with <see cref="CommandRegistry.RegisterInstance"/>).
    ///
    /// Supported parameter types: string, int, float, double, bool, and enums.
    /// Optional parameters (with default values) are supported.
    ///
    /// Example:
    ///   [DebugCommand("give_gold", "Adds gold to the player")]
    ///   static void GiveGold(int amount) { ... }
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class DebugCommandAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public DebugCommandAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }
    }
}
