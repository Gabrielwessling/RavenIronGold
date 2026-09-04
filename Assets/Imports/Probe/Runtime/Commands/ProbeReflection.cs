using System;
using System.Collections.Generic;
using System.Reflection;

namespace Probe
{
    /// <summary>
    /// Shared reflection helpers for discovering Probe attributes across the
    /// user's assemblies (skipping engine/framework ones for a faster scan).
    /// </summary>
    static class ProbeReflection
    {
        /// <summary>All user-code types, with engine/framework assemblies skipped.</summary>
        public static IEnumerable<Type> UserTypes()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (Skip(asm)) continue;

                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types; }
                catch { continue; }

                foreach (var t in types)
                    if (t != null) yield return t;
            }
        }

        // User code lives in Assembly-CSharp and custom asmdefs, which are not skipped.
        static bool Skip(Assembly asm)
        {
            var n = asm.FullName;
            return n.StartsWith("System", StringComparison.Ordinal)
                || n.StartsWith("Unity", StringComparison.Ordinal)
                || n.StartsWith("mscorlib", StringComparison.Ordinal)
                || n.StartsWith("netstandard", StringComparison.Ordinal)
                || n.StartsWith("Mono.", StringComparison.Ordinal)
                || n.StartsWith("nunit", StringComparison.Ordinal);
        }
    }
}
