# Subtitle System

## Overview

The subtitle system uses `Subtitle` as runtime data. It is a plain C# class marked with `[System.Serializable]`, this allows subtitles to be created by any system, such as speech, dialogue, cutscenes, or gameplay events, and sent directly to `SubtitleManager`.

Relevant scripts:

- `Assets/0 - Scripts/Subtitles/Subtitle.cs`
- `Assets/0 - Scripts/Subtitles/SubtitleManager.cs`

## Creating a Subtitle at Runtime

Create a subtitle with an object initializer and pass it to the manager:

```csharp
Subtitle subtitle = new Subtitle
{
    Speaker = "Guard",
    Content = "Halt! Who goes there?",
    IsImportant = true,
    IsSkippable = true
};

SubtitleManager.Instance.ShowSubtitle(subtitle);
```

For a one-off subtitle, the same operation can be written inline:

```csharp
SubtitleManager.Instance.ShowSubtitle(new Subtitle
{
    Speaker = "Narrator",
    Content = "The door opened slowly.",
    IsImportant = false,
    IsSkippable = false
});
```

## Subtitle Fields

| Field | Type | Purpose |
| --- | --- | --- |
| `Speaker` | `string` | Name displayed before the subtitle content. |
| `Content` | `string` | Text displayed to the player. |
| `IsImportant` | `bool` | Controls whether this subtitle interrupts the currently displayed subtitle. |
| `IsSkippable` | `bool` | Stores whether the subtitle may be skipped by a caller. Skipping input is not handled by `SubtitleManager` yet. |

## Queue and Priority Behavior

`SubtitleManager` displays one subtitle at a time.

- If no subtitle is active, the new subtitle is displayed immediately.
- If the active subtitle is important, the new subtitle is queued.
- If the active subtitle is not important and the new subtitle is also not important, the new subtitle is queued.
- If the active subtitle is not important and the new subtitle is important, the active subtitle is replaced immediately.
- Queued subtitles are displayed in first-in, first-out order after the current subtitle finishes.

## Inspector Usage

Because `Subtitle` is serializable, it can still be used as a serialized field on a `MonoBehaviour`, including `TestSubtitles`. The data is stored inside that component rather than as a separate Unity asset.

```csharp
public class SpeechEmitter : MonoBehaviour
{
    [SerializeField] private string speaker;
    [TextArea]
    [SerializeField] private string content;

    public void Speak()
    {
        SubtitleManager.Instance.ShowSubtitle(new Subtitle
        {
            Speaker = speaker,
            Content = content,
            IsImportant = false,
            IsSkippable = true
        });
    }
}
```

## Text Splitting

Long content is split into parts using two manager settings:

- `minimumContentCharactersPerPart`: target minimum length for a part, defaulting to 40 characters.
- `maximumContentCharactersPerPart`: maximum length for a part, defaulting to 80 characters.

The character limits apply only to `Content`; the `Speaker` text and its separator are not counted. Once a part reaches the minimum length, phrase punctuation (`.`, `,`, `!`, `?`, `;`, or `:`) is used as a preferred stopping point. If punctuation is not reached before the maximum, the part ends at a word boundary. If the final part would be shorter than the minimum, whole words are moved from the previous part when possible. This avoids a very short ending while keeping parts within the maximum. Word boundaries can make the final part shorter than the minimum when rebalancing cannot reach it without exceeding the maximum. Empty or null content is handled as an empty part.

## Subtitle Timing

Timing is controlled globally by `SubtitleManager`; individual `Subtitle` objects do not contain a duration. The manager calculates each part's duration from its content length using `subtitleCharactersPerSecond`, then clamps it between:

- `minimumSubtitleDuration`: shortest display time, defaulting to 1 second.
- `maximumSubtitleDuration`: longest display time, defaulting to 5 seconds.

The default reading speed is 20 content characters per second. The speaker text is excluded from this calculation.

## Accessing State

The manager exposes the following methods:

```csharp
List<Subtitle> queuedSubtitles = SubtitleManager.Instance.GetSubtitles();
bool isShowing = SubtitleManager.Instance.IsShowingSubtitle();
Subtitle current = SubtitleManager.Instance.GetCurrentSubtitle();
```

`GetSubtitles()` returns the manager's queue directly, so callers should avoid modifying the returned list unless they intentionally need to alter queued subtitles.
