# Probe — Zero-Setup Runtime Tuning Console for Unity

A free, zero-setup **runtime tuning console** for Unity. Watch live values, run
commands, tweak variables with sliders, and **save your tuning as presets** —
drop the folder in and press Play. No prefabs, no scene changes.

Free asset by **HanoStudio**. The Asset Store distribution follows the Unity
Asset Store EULA.

---

## Why Probe

- **Zero setup.** The console and panel build their own UI from code and
  bootstrap themselves at startup. You don't add anything to a scene.
- **Attribute-based commands.** Turn any method into a console command with a
  single `[DebugCommand]` attribute. Arguments are parsed and type-checked for you.
- **Live debug panel.** FPS, memory, and any value you want to watch — top-left,
  always there, one line of code each.
- **One-tap buttons & live sliders.** `[DebugButton]` runs a method with a tap;
  `[DebugTweak]` puts a slider on a variable so you can tune it at runtime — then
  **save / load tuning presets** as JSON.
- **Render-pipeline agnostic.** Pure uGUI. Works in Built-in, URP, and HDRP.
- **Both input backends.** Works with the new Input System and the legacy
  Input Manager (and both at once).

---

## Quick start

1. Import the package.
2. Press Play in any scene. The **debug panel** appears top-left.
3. Open the **console** with the backquote key `` ` `` or the on-screen `</>`
   button (top-right).
4. Type `help`.

That's it. Nothing to set up.

> Want a guided tour? Open **`Probe/Demo/ProbeDemo.unity`** and press Play — a
> cube you can `heal`, `teleport`, and bob with live `[DebugTweak]` sliders, plus
> example commands, buttons, and watches. (Or add the `ProbeDemo` component to
> any GameObject yourself.)

For the complete offline setup guide and script reference, open
**`Probe/Documentation/Probe_User_Guide.pdf`**.

---

## Commands

Mark any method with `[DebugCommand]`:

```csharp
using Probe;

public static class Cheats
{
    [DebugCommand("give_gold", "Add gold to the player")]
    static string GiveGold(int amount)
    {
        Player.Gold += amount;
        return $"Gold is now {Player.Gold}";
    }
}
```

Now type `give_gold 500` in the console.

**Static methods** are discovered automatically — no registration needed.

**Instance methods** (on a MonoBehaviour) need the live object registered:

```csharp
public class Player : MonoBehaviour
{
    void Awake()  => CommandRegistry.RegisterInstance(this);
    void OnDestroy() => CommandRegistry.UnregisterInstance(this);

    [DebugCommand("heal", "Heal the player")]
    string Heal(int amount) { /* ... */ return $"HP +{amount}"; }
}
```

**Supported argument types:** `string`, `int`, `float`, `double`, `bool`, and
`enum`. Optional parameters (with default values) are supported, and quoted
`"strings with spaces"` work.

Whatever your method returns is printed to the console.

### Console shortcuts

| Key            | Action                          |
| -------------- | ------------------------------- |
| `` ` ``        | Show / hide the console         |
| `Tab`          | Autocomplete the command name   |
| `↑` / `↓`      | Recall previous commands        |
| `help`         | List commands                   |
| `help <name>`  | Show usage for one command      |

The console captures Unity's log output (color-coded by severity). Use the
**search box** and the **Log / Warn / Err** toggles at the top to filter it.

---

## Debug panel

The panel shows **FPS** and **memory** out of the box. Add your own live values
from anywhere:

```csharp
ProbePanel.Watch("HP", () => player.health);
ProbePanel.Watch("State", () => enemy.currentState);
ProbePanel.Unwatch("HP");      // remove one
ProbePanel.Toggle();           // show / hide
```

The panel hides itself automatically while the console is open, so they never
overlap.

---

## One-tap buttons

For things you trigger constantly, skip the typing. Mark a **parameterless**
method with `[DebugButton]` and a button appears on the panel — perfect for
on-device / mobile QA:

```csharp
[DebugButton("Add 100 Gold")]
static void AddGold() => Player.Gold += 100;

[DebugButton("Kill All Enemies")]
void KillAll() { /* ... */ }   // instance method
```

Static buttons are found automatically. For instance methods, register the
object: `ProbeActions.RegisterInstance(this);` (and `UnregisterInstance` in
`OnDestroy`). Omit the label to use the method name.

---

## Live tweaks (sliders)

Tune values while the game runs — no recompiling. Mark a numeric field or
property with `[DebugTweak(min, max)]` and a slider appears on the panel:

```csharp
public class Movement : MonoBehaviour
{
    void Awake() => ProbeTweaks.RegisterInstance(this);   // instance members only

    [DebugTweak(0f, 20f)] public float moveSpeed = 5f;
    [DebugTweak(1, 10)]   public int   jumpCount = 2;
}
```

Drag the slider and `moveSpeed` updates live. Works on `float`, `int`,
`double`, and `long`. Static members are found automatically; instance members
need `ProbeTweaks.RegisterInstance(this)`.

You can also add one in code, without an attribute:

```csharp
ProbeTweaks.Add("Gravity", -30f, 0f, () => Physics.gravity.y,
                v => Physics.gravity = new Vector3(0, v, 0));
```

### Presets — save & restore a tuning

Found a feel you like? Save the whole set of slider values and bring it back
later. The panel has **Save / Load / Reset** buttons (a quick one-slot preset),
and the console supports named presets:

```
preset_save floaty_jump
preset_load floaty_jump
preset_list
preset_reset           # back to the values at startup
```

Presets are JSON files under `Application.persistentDataPath/ProbePresets`, so
they survive restarts and you can check them into source control.

```csharp
ProbePresets.Save("hard_mode");
ProbePresets.Load("hard_mode");
ProbePresets.Reset();
```

---

## Scripting reference

```csharp
// Console
ProbeConsole.Instance.Toggle();
ProbeConsole.Instance.SetVisible(true);
bool open = ProbeConsole.Instance.IsVisible;

// Commands
CommandRegistry.RegisterInstance(obj);
CommandRegistry.UnregisterInstance(obj);
string output = CommandParser.Execute("give_gold 500");

// One-tap buttons
ProbeActions.RegisterInstance(obj);
ProbeActions.UnregisterInstance(obj);

// Live tweaks (sliders)
ProbeTweaks.RegisterInstance(obj);
ProbeTweaks.UnregisterInstance(obj);
ProbeTweaks.Add("Label", min, max, getter, setter);

// Tweak presets
ProbePresets.Save("name");
ProbePresets.Load("name");
ProbePresets.Reset();

// Panel
ProbePanel.Watch("Label", () => value);
ProbePanel.Unwatch("Label");
ProbePanel.SetVisible(false);
```

---

## Release builds (Probe is off by default)

Probe auto-runs **only in the Editor and Development builds**. In a normal
release build it does nothing — the console and panel are never created, so it
costs nothing and never shows up in front of players.

Probe also resets its static registries, event subscribers, UI references, and
cached watches at play-mode startup, so it is safe when Domain Reload is disabled
for faster Enter Play Mode.

Control it with scripting define symbols (Project Settings ▸ Player):

| Symbol          | Effect                                         |
| --------------- | ---------------------------------------------- |
| *(none)*        | On in Editor + Development builds (default)    |
| `PROBE_ENABLE`  | Force on, even in release builds               |
| `PROBE_DISABLE` | Force off everywhere                           |

`[DebugCommand]` / `[DebugButton]` / `[DebugTweak]` on your own code compile into
every build, which is harmless (Probe just won't scan them when off). To strip
them too, wrap them in `#if UNITY_EDITOR || DEVELOPMENT_BUILD || PROBE_ENABLE`.

### IL2CPP / code stripping

With IL2CPP + Managed Stripping, members only reached by reflection (your
`[DebugCommand]` / `[DebugButton]` / `[DebugTweak]` methods) can be stripped from
release builds. If you ship Probe in a **release** build via `PROBE_ENABLE`, add
`[UnityEngine.Scripting.Preserve]` to those members, or a `link.xml` that
preserves your assembly. (The default Editor / Development setup is unaffected.)

> The `Probe/Demo` folder is just a sample — delete it any time. Its example
> commands only appear when you add the `ProbeDemo` component to a scene, so it
> never clutters your own console.

---

## Requirements

- Requires Unity 2021.3+. Validated on Unity 2022.3 LTS and Unity 6.
- No third-party dependencies. Built with Unity uGUI. TextMeshPro is not required.
- Any render pipeline (Built-in / URP / HDRP)
- Either input backend (new Input System or legacy Input Manager)

---

## Support

Questions or feature requests: **hellohanostudio@gmail.com**

Thanks for using Probe! — HanoStudio
