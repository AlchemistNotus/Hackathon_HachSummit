using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{

    public void LoadLevelName(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void StartGame()
    {
        LevelManager.Instance.LoadFirstLevel();
    }

    public void Restart()
    {
        LevelManager.Instance.Restart();
    }
}
