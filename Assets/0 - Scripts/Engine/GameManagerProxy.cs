using UnityEngine;

public class GameManagerProxy : MonoBehaviour
{
    private GameManager Manager => GameManager.Instance;

    public void StartNewGame()
    {
        if (Manager != null)
            Manager.StartNewGame();
    }

    public void GoToScene(string sceneName)
    {
        if (Manager != null)
            Manager.GoToScene(sceneName);
    }

    public void LoadGame()
    {
        if (Manager != null)
            Manager.LoadGame();
    }

    public void SaveGame(string saveData)
    {
        if (Manager != null)
            Manager.SaveGame(saveData);
    }

    public void QuitGame()
    {
        if (Manager != null)
            Manager.QuitGame();
    }
}
