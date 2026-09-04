using System.Text;

namespace Probe
{
    /// <summary>Commands that ship with Probe.</summary>
    static class BuiltinCommands
    {
        [DebugCommand("help", "List all commands, or show details: help <name>")]
        static string Help(string command = "")
        {
            if (!string.IsNullOrEmpty(command))
            {
                if (CommandRegistry.Commands.TryGetValue(command, out var c))
                    return $"{c.Usage}\n{c.Description}";
                return $"Unknown command: {command}";
            }

            var sb = new StringBuilder("Available commands:\n");
            foreach (var c in CommandRegistry.Commands.Values)
                sb.AppendLine($"  {c.Name,-16} {c.Description}");
            return sb.ToString().TrimEnd();
        }

        [DebugCommand("echo", "Print the given text back")]
        static string Echo(string text) => text;

        // ---- Tweak presets (save/load [DebugTweak] slider values) ----------

        [DebugCommand("preset_save", "Save current tweak values: preset_save <name>")]
        static string PresetSave(string name)
            => ProbePresets.Save(name) ? $"Saved preset '{name}'." : $"Couldn't save preset '{name}'.";

        [DebugCommand("preset_load", "Load tweak values from a preset: preset_load <name>")]
        static string PresetLoad(string name)
            => ProbePresets.Load(name) ? $"Loaded preset '{name}'." : $"No preset named '{name}'.";

        [DebugCommand("preset_list", "List saved tweak presets")]
        static string PresetList()
        {
            var names = ProbePresets.List();
            return names.Count == 0 ? "No presets saved yet." : "Presets: " + string.Join(", ", names);
        }

        [DebugCommand("preset_reset", "Reset all tweaks to their starting values")]
        static string PresetReset() { ProbePresets.Reset(); return "Tweaks reset to defaults."; }
    }
}
