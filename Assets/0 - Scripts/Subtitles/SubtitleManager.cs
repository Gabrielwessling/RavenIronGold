using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class SubtitleManager : MonoBehaviour
{
    public TMP_Text subtitleText;

    public static SubtitleManager Instance { get; private set; }

    [SerializeField, Min(1)] private int minimumContentCharactersPerPart = 40;
    [SerializeField, Min(1)] private int maximumContentCharactersPerPart = 80;
    [SerializeField, Min(0.1f)] private float minimumSubtitleDuration = 1f;
    [SerializeField, Min(0.1f)] private float maximumSubtitleDuration = 5f;
    [SerializeField, Min(1f)] private float subtitleCharactersPerSecond = 20f;
    
    private bool isShowingSubtitle = false;
    private Subtitle currentSubtitle;
    private List<Subtitle> subtitleQueue = new List<Subtitle>();
    private List<string> currentSubtitleParts = new List<string>();
    private int currentPartIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void ShowSubtitle(Subtitle subtitle)
    {
        if (!isShowingSubtitle)
        {
            UseSubtitle(subtitle);
        }
        else if (currentSubtitle.IsImportant || !subtitle.IsImportant)
        {
            subtitleQueue.Add(subtitle);
        }
        else if (!currentSubtitle.IsImportant && subtitle.IsImportant)
        {
            UseSubtitle(subtitle);
        }
    }

    private void UseSubtitle(Subtitle subtitle)
    {
        CancelInvoke(nameof(HideSubtitle));
        currentSubtitle = subtitle;
        currentSubtitleParts = SplitContent(subtitle.Content);
        currentPartIndex = 0;
        isShowingSubtitle = true;
        ShowCurrentPart();
    }

    private void ShowCurrentPart()
    {
        subtitleText.text = $"{currentSubtitle.Speaker}: {currentSubtitleParts[currentPartIndex]}";
        Invoke(nameof(HideSubtitle), GetCurrentPartDuration());
    }

    private float GetCurrentPartDuration()
    {
        float minimumDuration = Mathf.Max(0.1f, minimumSubtitleDuration);
        float maximumDuration = Mathf.Max(minimumDuration, maximumSubtitleDuration);
        float duration = currentSubtitleParts[currentPartIndex].Length / Mathf.Max(1f, subtitleCharactersPerSecond);
        return Mathf.Clamp(duration, minimumDuration, maximumDuration);
    }

    private void HideSubtitle()
    {
        if (currentPartIndex < currentSubtitleParts.Count - 1)
        {
            currentPartIndex++;
            ShowCurrentPart();
            return;
        }

        isShowingSubtitle = false;
        subtitleText.text = "";
        if (subtitleQueue.Count > 0)
        {
            Subtitle nextSubtitle = subtitleQueue[0];
            subtitleQueue.RemoveAt(0);
            UseSubtitle(nextSubtitle);
        }
    }

    private List<string> SplitContent(string content)
    {
        List<string> parts = new List<string>();
        string[] words = (content ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string currentPart = "";
        int maxPartLength = Mathf.Max(1, maximumContentCharactersPerPart);
        int minPartLength = Mathf.Clamp(minimumContentCharactersPerPart, 1, maxPartLength);

        foreach (string word in words)
        {
            if (currentPart.Length == 0)
            {
                currentPart = word;
            }
            else if (currentPart.Length + word.Length + 1 <= maxPartLength)
            {
                currentPart += " " + word;
            }
            else
            {
                parts.Add(currentPart);
                currentPart = word;
            }

            if (currentPart.Length >= minPartLength && EndsWithPhrasePunctuation(word))
            {
                parts.Add(currentPart);
                currentPart = "";
            }
        }

        if (currentPart.Length > 0)
        {
            parts.Add(currentPart);
        }

        if (parts.Count == 0)
        {
            parts.Add(string.Empty);
        }

        BalanceFinalPart(parts, minPartLength, maxPartLength);
        return parts;
    }

    private bool EndsWithPhrasePunctuation(string word)
    {
        string trimmedWord = word.TrimEnd('"', '\'', ')', ']', '}');
        return trimmedWord.EndsWith(".") || trimmedWord.EndsWith(",") ||
               trimmedWord.EndsWith("!") || trimmedWord.EndsWith("?") ||
               trimmedWord.EndsWith(";") || trimmedWord.EndsWith(":");
    }

    private void BalanceFinalPart(List<string> parts, int minPartLength, int maxPartLength)
    {
        if (parts.Count < 2 || parts[parts.Count - 1].Length >= minPartLength ||
            EndsWithPhrasePunctuation(parts[parts.Count - 2]))
        {
            return;
        }

        int previousPartIndex = parts.Count - 2;
        string previousPart = parts[previousPartIndex];
        string finalPart = parts[parts.Count - 1];

        if (previousPart.Length + finalPart.Length + 1 <= maxPartLength)
        {
            parts[previousPartIndex] = previousPart + " " + finalPart;
            parts.RemoveAt(parts.Count - 1);
            return;
        }

        List<string> previousWords = new List<string>(previousPart.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        while (finalPart.Length < minPartLength && previousWords.Count > 1)
        {
            string movedWord = previousWords[previousWords.Count - 1];
            string candidateFinalPart = movedWord + " " + finalPart;
            if (candidateFinalPart.Length > maxPartLength)
            {
                break;
            }

            previousWords.RemoveAt(previousWords.Count - 1);
            previousPart = string.Join(" ", previousWords);
            finalPart = candidateFinalPart;
        }

        parts[previousPartIndex] = previousPart;
        parts[parts.Count - 1] = finalPart;
    }

    public List<Subtitle> GetSubtitles()
    {
        return subtitleQueue;
    }

    public bool IsShowingSubtitle()
    {
        return isShowingSubtitle;
    }

    public Subtitle GetCurrentSubtitle()
    {
        return currentSubtitle;
    }
}
