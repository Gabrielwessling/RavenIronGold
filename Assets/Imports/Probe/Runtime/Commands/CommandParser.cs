using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Probe
{
    /// <summary>
    /// Parses a console input line, resolves the command, converts arguments to
    /// the method's parameter types, and invokes it. Returns output text.
    /// </summary>
    public static class CommandParser
    {
        public static string Execute(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var tokens = Tokenize(input);
            if (tokens.Count == 0) return string.Empty;

            var name = tokens[0];
            if (!CommandRegistry.Commands.TryGetValue(name, out var cmd))
                return $"Unknown command: '{name}'. Type 'help' for a list.";

            var args = tokens.GetRange(1, tokens.Count - 1);
            var ps = cmd.Parameters;

            if (args.Count > ps.Length)
                return $"Too many arguments. Usage: {cmd.Usage}";

            var values = new object[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                if (i < args.Count)
                {
                    if (!TryConvert(args[i], ps[i].ParameterType, out values[i]))
                        return $"Argument '{ps[i].Name}' must be {FriendlyType(ps[i].ParameterType)}. Usage: {cmd.Usage}";
                }
                else if (ps[i].HasDefaultValue)
                {
                    values[i] = ps[i].DefaultValue;
                }
                else
                {
                    return $"Missing argument '{ps[i].Name}'. Usage: {cmd.Usage}";
                }
            }

            try
            {
                var result = cmd.Method.Invoke(cmd.Target, values);
                return result != null ? result.ToString() : $"✓ {name}";
            }
            catch (TargetInvocationException e)
            {
                return $"Error: {e.InnerException?.Message ?? e.Message}";
            }
            catch (Exception e)
            {
                return $"Error: {e.Message}";
            }
        }

        static bool TryConvert(string s, Type t, out object value)
        {
            value = null;
            if (t == typeof(string)) { value = s; return true; }
            if (t == typeof(int)) { if (int.TryParse(s, out var i)) { value = i; return true; } return false; }
            if (t == typeof(float)) { if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) { value = f; return true; } return false; }
            if (t == typeof(double)) { if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) { value = d; return true; } return false; }
            if (t == typeof(bool))
            {
                if (bool.TryParse(s, out var b)) { value = b; return true; }
                if (s == "1") { value = true; return true; }
                if (s == "0") { value = false; return true; }
                return false;
            }
            if (t.IsEnum)
            {
                try { value = Enum.Parse(t, s, true); return true; }
                catch { return false; }
            }
            try { value = Convert.ChangeType(s, t, CultureInfo.InvariantCulture); return true; }
            catch { return false; }
        }

        static string FriendlyType(Type t)
        {
            if (t == typeof(int)) return "an integer";
            if (t == typeof(float) || t == typeof(double)) return "a number";
            if (t == typeof(bool)) return "true/false";
            if (t.IsEnum) return "one of: " + string.Join(", ", Enum.GetNames(t));
            return t.Name;
        }

        // Whitespace-separated tokens, with "quoted strings" kept intact.
        static List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            foreach (var c in input)
            {
                if (c == '"') { inQuotes = !inQuotes; continue; }
                if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (sb.Length > 0) { tokens.Add(sb.ToString()); sb.Clear(); }
                }
                else sb.Append(c);
            }
            if (sb.Length > 0) tokens.Add(sb.ToString());
            return tokens;
        }
    }
}
