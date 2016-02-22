using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    #region Singleton
    private static LevelManager _instance;
    public static LevelManager Instance
    {
        get { return _instance ?? new GameObject("LevelManager").AddComponent<LevelManager>(); }
    }

    public void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    private int _currentGamePlayLevel = 1;

    private int _currentMaxLevels = 10;

    public void LoadFirstLevel()
    {
        SceneManager.LoadScene("Level1");
    }

    public bool IsNextLevelAvailable()
    {
        string nextLevelName = string.Format("Level{0}", _currentGamePlayLevel + 1);
        
        Scene nextScene = SceneManager.GetSceneByName(nextLevelName);
        return (_currentGamePlayLevel+1) <= _currentMaxLevels;
    }

    public void LoadNextLevel()
    {
        string nextLevelName = string.Format("Level{0}", _currentGamePlayLevel+1);
        _currentGamePlayLevel++;
        SceneManager.LoadScene(nextLevelName);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
