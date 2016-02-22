using System;
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

    public int CurrentGamePlayLevel = 1;

    public const int CurrentMaxLevels = 12;

    public void LoadFirstLevel()
    {
        SceneManager.LoadScene("Level1");
        CurrentGamePlayLevel = 1;
        SessionStats.Instance.Start1Level = DateTime.UtcNow;
    }

    public bool IsNextLevelAvailable()
    {
        string nextLevelName = string.Format("Level{0}", CurrentGamePlayLevel + 1);
        
        Scene nextScene = SceneManager.GetSceneByName(nextLevelName);
        return (CurrentGamePlayLevel+1) <= CurrentMaxLevels;
    }

    public void LoadNextLevel()
    {
        string nextLevelName = string.Format("Level{0}", CurrentGamePlayLevel+1);
        CurrentGamePlayLevel++;
        SceneManager.LoadScene(nextLevelName);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadCredits()
    {
        SessionStats.Instance.End12Level = DateTime.UtcNow;
        SceneManager.LoadScene("Credits");
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
