using System;
using System.Collections.Generic;
using System.Reflection;

namespace Probe
{
    /// <summary>
    /// Discovers and stores all [DebugCommand] methods. Static commands are
    /// found automatically by scanning user assemblies once; instance commands
    /// are added at runtime via <see cref="RegisterInstance"/>.
    /// </summary>
    public static class CommandRegistry
    {
        static readonly Dictionary<string, DebugCommand> _commands =
            new Dictionary<string, DebugCommand>(StringComparer.OrdinalIgnoreCase);
        static readonly List<object> _instances = new List<object>();
        static bool _built;

        public static IReadOnlyDictionary<string, DebugCommand> Commands
        {
            get { EnsureBuilt(); return _commands; }
        }

        // Static fields survive across play sessions when "Enter Play Mode" has
        // domain reload disabled. Clear them so we don't keep stale commands or
        // dead instance references from a previous run.
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetState()
        {
            _commands.Clear();
            _instances.Clear();
            _built = false;
        }

        public static void EnsureBuilt()
        {
            if (_built) return;
            _built = true;
            ScanStaticCommands();
            foreach (var inst in _instances) ScanInstance(inst);
        }

        /// <summary>Register a live object so its [DebugCommand] instance methods work.</summary>
        public static void RegisterInstance(object target)
        {
            if (target == null) return;
            if (!_instances.Contains(target)) _instances.Add(target);
            EnsureBuilt();
            ScanInstance(target);
        }

        public static void UnregisterInstance(object target)
        {
            if (target == null) return;
            _instances.Remove(target);
            var toRemove = new List<string>();
            foreach (var kv in _commands)
                if (ReferenceEquals(kv.Value.Target, target)) toRemove.Add(kv.Key);
            foreach (var key in toRemove) _commands.Remove(key);
        }

        static void ScanStaticCommands()
        {
            foreach (var type in ProbeReflection.UserTypes())
            {
                try
                {
                    AddCommandsFrom(type.GetMethods(BindingFlags.Static | BindingFlags.Public |
                                                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly), null);
                }
                catch { /* skip types that can't be reflected */ }
            }
        }

        static void ScanInstance(object target)
            => AddCommandsFrom(target.GetType().GetMethods(
                   BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), target);

        static void AddCommandsFrom(MethodInfo[] methods, object target)
        {
            foreach (var m in methods)
            {
                var attr = (DebugCommandAttribute)Attribute.GetCustomAttribute(
                    m, typeof(DebugCommandAttribute));
                if (attr != null) Add(attr, m, target);
            }
        }

        static void Add(DebugCommandAttribute attr, MethodInfo method, object target)
        {
            _commands[attr.Name] = new DebugCommand
            {
                Name = attr.Name,
                Description = attr.Description,
                Method = method,
                Target = target,
                Parameters = method.GetParameters()
            };
        }
    }
}
