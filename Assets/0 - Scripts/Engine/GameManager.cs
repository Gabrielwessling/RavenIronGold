using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public string gameSceneName = "GameScene";

    public static GameManager Instance => instance;

    private bool isSceneProxy;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            isSceneProxy = true;
        }
    }

    #region Commands
    public void StartNewGame()
    {
        if (isSceneProxy)
        {
            instance.StartNewGame();
            return;
        }

        Debug.Log("Starting New Game on scene: " + gameSceneName);
        SceneManager.LoadScene(gameSceneName);
    }
    public void GoToScene(string sceneName)
    {
        if (isSceneProxy)
        {
            instance.GoToScene(sceneName);
            return;
        }

        Debug.Log("Going to scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
    public void LoadGame()
    {
        if (isSceneProxy)
        {
            instance.LoadGame();
            return;
        }

        if (PlayerPrefs.HasKey("SavedGame"))
        {
            Debug.Log("Game Loaded!");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogWarning("No saved game found!");
        }
    }

    public void SaveGame(string saveData)
    {
        if (isSceneProxy)
        {
            instance.SaveGame(saveData);
            return;
        }

        PlayerPrefs.SetString("SavedGame", saveData);
        Debug.Log("Game Saved! Save Data: " + saveData);
    }
    public void QuitGame()
    {
        if (isSceneProxy)
        {
            instance.QuitGame();
            return;
        }

        Debug.Log("Quitting Game...");
        Application.Quit();
    }
    #endregion
}
