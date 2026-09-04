using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Probe
{
    /// <summary>
    /// Discovers [DebugButton] methods and exposes them as tappable actions for
    /// the debug panel. Static actions are found automatically; instance actions
    /// need <see cref="RegisterInstance"/>.
    /// </summary>
    public static class ProbeActions
    {
        public sealed class Action
        {
            public string Label;
            public MethodInfo Method;
            public object Target;     // null for static actions

            public void Invoke()
            {
                try { Method.Invoke(Target, null); }
                catch (TargetInvocationException e) { Debug.LogException(e.InnerException ?? e); }
            }
        }

        static readonly List<Action> _actions = new List<Action>();
        static readonly List<object> _instances = new List<object>();
        static bool _built;

        /// <summary>Raised when the action list changes, so the panel can rebuild its buttons.</summary>
        public static event System.Action Changed;

        public static IReadOnlyList<Action> All { get { EnsureBuilt(); return _actions; } }

        // Clear stale state between play sessions when domain reload is disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetState()
        {
            _actions.Clear();
            _instances.Clear();
            Changed = null;
            _built = false;
        }

        public static void EnsureBuilt()
        {
            if (_built) return;
            _built = true;

            foreach (var type in ProbeReflection.UserTypes())
            {
                try
                {
                    Scan(type.GetMethods(BindingFlags.Static | BindingFlags.Public |
                                         BindingFlags.NonPublic | BindingFlags.DeclaredOnly), null);
                }
                catch { /* skip types that can't be reflected */ }
            }

            foreach (var inst in _instances) ScanInstance(inst);
        }

        /// <summary>Register a live object so its [DebugButton] instance methods appear.</summary>
        public static void RegisterInstance(object target)
        {
            if (target == null) return;
            EnsureBuilt();
            if (!_instances.Contains(target)) _instances.Add(target);
            _actions.RemoveAll(a => ReferenceEquals(a.Target, target)); // avoid dupes on re-register
            ScanInstance(target);
        }

        public static void UnregisterInstance(object target)
        {
            if (target == null) return;
            _instances.Remove(target);
            if (_actions.RemoveAll(a => ReferenceEquals(a.Target, target)) > 0) Changed?.Invoke();
        }

        static void ScanInstance(object target)
            => Scan(target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), target);

        static void Scan(MethodInfo[] methods, object target)
        {
            bool added = false;
            foreach (var m in methods)
            {
                var attr = (DebugButtonAttribute)System.Attribute.GetCustomAttribute(m, typeof(DebugButtonAttribute));
                if (attr == null) continue;

                if (m.GetParameters().Length != 0)
                {
                    Debug.LogWarning($"[Probe] [DebugButton] '{m.Name}' is ignored — button methods must take no parameters.");
                    continue;
                }

                _actions.Add(new Action
                {
                    Label = string.IsNullOrEmpty(attr.Label) ? m.Name : attr.Label,
                    Method = m,
                    Target = target
                });
                added = true;
            }
            if (added) Changed?.Invoke();
        }
    }
}
