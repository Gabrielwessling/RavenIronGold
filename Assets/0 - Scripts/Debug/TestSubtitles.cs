using UnityEngine;

public class TestSubtitles : MonoBehaviour
{
    public bool showNow = false;
    public Subtitle subtitleToShow;
    
    void Update()
    {
        if (showNow)
        {
            SubtitleManager.Instance.ShowSubtitle(subtitleToShow);
            showNow = false;
        }
    }
}
