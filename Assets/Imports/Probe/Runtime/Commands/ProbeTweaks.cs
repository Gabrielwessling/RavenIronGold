using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Probe
{
    /// <summary>
    /// Discovers [DebugTweak] numeric members and exposes them as live sliders
    /// for the debug panel. Static members are found automatically; instance
    /// members need <see cref="RegisterInstance"/>. You can also add tweaks
    /// programmatically with <see cref="Add"/>.
    /// </summary>
    public static class ProbeTweaks
    {
        public sealed class Tweak
        {
            public string Label;
            public float Min, Max;
            public bool IsInt;
            public object Target;     // null for static / programmatic tweaks
            public float Default;     // value at registration time (for Reset)
            public Func<float> Get;
            public Action<float> Set;
        }

        static readonly List<Tweak> _tweaks = new List<Tweak>();
        static readonly List<object> _instances = new List<object>();
        static bool _built;

        /// <summary>Raised when the tweak list changes, so the panel can rebuild its sliders.</summary>
        public static event System.Action Changed;

        public static IReadOnlyList<Tweak> All { get { EnsureBuilt(); return _tweaks; } }

        // Clear stale state between play sessions when domain reload is disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetState()
        {
            _tweaks.Clear();
            _instances.Clear();
            Changed = null;
            _built = false;
        }

        /// <summary>Add a slider directly, without an attribute.</summary>
        public static void Add(string label, float min, float max, Func<float> get, Action<float> set, bool isInt = false)
        {
            EnsureBuilt();
            _tweaks.RemoveAll(t => t.Label == label);
            float def = 0f; try { def = get(); } catch { }
            _tweaks.Add(new Tweak { Label = label, Min = min, Max = max, Get = get, Set = set, IsInt = isInt, Default = def });
            Changed?.Invoke();
        }

        public static void EnsureBuilt()
        {
            if (_built) return;
            _built = true;

            foreach (var type in ProbeReflection.UserTypes())
            {
                try { ScanType(type, null, staticMembers: true); }
                catch { /* skip types that can't be reflected */ }
            }

            foreach (var inst in _instances) ScanType(inst.GetType(), inst, staticMembers: false);
        }

        /// <summary>Register a live object so its [DebugTweak] instance members appear.</summary>
        public static void RegisterInstance(object target)
        {
            if (target == null) return;
            EnsureBuilt();
            if (!_instances.Contains(target)) _instances.Add(target);
            _tweaks.RemoveAll(t => ReferenceEquals(t.Target, target)); // avoid dupes on re-register
            ScanType(target.GetType(), target, staticMembers: false);
        }

        public static void UnregisterInstance(object target)
        {
            if (target == null) return;
            _instances.Remove(target);
            if (_tweaks.RemoveAll(t => ReferenceEquals(t.Target, target)) > 0) Changed?.Invoke();
        }

        static void ScanType(Type type, object target, bool staticMembers)
        {
            var flags = staticMembers
                ? BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly
                : BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            bool added = false;

            foreach (var f in type.GetFields(flags))
            {
                var attr = (DebugTweakAttribute)Attribute.GetCustomAttribute(f, typeof(DebugTweakAttribute));
                if (attr == null) continue;
                if (!IsNumeric(f.FieldType)) { WarnType(f.Name, f.FieldType); continue; }

                var member = f;
                added |= AddTweak(attr, member.Name, member.FieldType, target,
                    () => member.GetValue(target),
                    v => member.SetValue(target, v));
            }

            foreach (var p in type.GetProperties(flags))
            {
                var attr = (DebugTweakAttribute)Attribute.GetCustomAttribute(p, typeof(DebugTweakAttribute));
                if (attr == null) continue;
                if (p.GetIndexParameters().Length != 0 || !p.CanRead || !p.CanWrite || !IsNumeric(p.PropertyType))
                {
                    WarnType(p.Name, p.PropertyType); continue;
                }

                var member = p;
                added |= AddTweak(attr, member.Name, member.PropertyType, target,
                    () => member.GetValue(target),
                    v => member.SetValue(target, v));
            }

            if (added) Changed?.Invoke();
        }

        static bool AddTweak(DebugTweakAttribute attr, string name, Type type, object target,
            Func<object> rawGet, Action<object> rawSet)
        {
            bool isInt = type == typeof(int) || type == typeof(long);
            float def = 0f; try { def = Convert.ToSingle(rawGet()); } catch { }
            string label = UniqueLabel(string.IsNullOrEmpty(attr.Label) ? name : attr.Label);
            _tweaks.Add(new Tweak
            {
                Label = label,
                Min = attr.Min,
                Max = attr.Max,
                IsInt = isInt,
                Target = target,
                Default = def,
                Get = () => Convert.ToSingle(rawGet()),
                Set = v =>
                {
                    object boxed = Convert.ChangeType(isInt ? Mathf.Round(v) : v, type);
                    rawSet(boxed);
                }
            });
            return true;
        }

        // Labels are how presets address tweaks, so they must be unique. If two
        // members resolve to the same label, suffix the later one and warn.
        static string UniqueLabel(string label)
        {
            if (!_tweaks.Exists(t => t.Label == label)) return label;

            string baseLabel = label;
            int n = 2;
            while (_tweaks.Exists(t => t.Label == label)) label = $"{baseLabel} ({n++})";
            Debug.LogWarning($"[Probe] Duplicate [DebugTweak] label '{baseLabel}' — showing it as '{label}'. " +
                             "Give it an explicit label to keep presets stable.");
            return label;
        }

        static bool IsNumeric(Type t)
            => t == typeof(float) || t == typeof(int) || t == typeof(double) || t == typeof(long);

        static void WarnType(string member, Type type)
            => Debug.LogWarning($"[Probe] [DebugTweak] '{member}' is ignored — only float/int/double/long are supported (got {type.Name}).");
    }
}
