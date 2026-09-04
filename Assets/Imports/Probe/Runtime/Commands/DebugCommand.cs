using System.Reflection;
using System.Text;

namespace Probe
{
    /// <summary>A single registered debug command.</summary>
    public sealed class DebugCommand
    {
        public string Name;
        public string Description;
        public MethodInfo Method;
        public object Target;            // null for static commands
        public ParameterInfo[] Parameters;

        /// <summary>Human-readable usage, e.g. "give_gold &lt;amount&gt;".</summary>
        public string Usage
        {
            get
            {
                var sb = new StringBuilder(Name);
                foreach (var p in Parameters)
                    sb.Append(p.HasDefaultValue ? $" [{p.Name}]" : $" <{p.Name}>");
                return sb.ToString();
            }
        }
    }
}
