using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour
{
    public static InGameMenu Instance;

    public GameObject LoosePanel;
    public GameObject VictoryPanel;
    public GameObject GamePanel;

    public Text CurrentLevel;

    void Awake()
    {
        Instance = this;
        CurrentLevel.text = LevelManager.Instance.CurrentGamePlayLevel + "/" + LevelManager.CurrentMaxLevels;

        var cur = PlayerPrefs.GetInt("Music", 1) != 1;
        if (cur)
        {
            Toggle.overrideSprite = MusicOff;
        }
        SoundManager.SetMusicMuted(cur);
    }

    public void ShowLooseMenu()
    {
        GamePanel.SetActive(false);
        LoosePanel.SetActive(true);
        LoosePanel.GetComponent<CanvasGroup>().alpha = 0;
        LoosePanel.GetComponent<CanvasGroup>().DOFade(1, 1);
    }

    public void ShowVictoryMenu()
    {
        GamePanel.SetActive(false);
        VictoryPanel.SetActive(true);
        VictoryPanel.GetComponent<CanvasGroup>().alpha = 0;
        VictoryPanel.GetComponent<CanvasGroup>().DOFade(1, 1);
    }

    public void Restart()
    {
        LevelManager.Instance.Restart();
    }

    public void ExitToMenu()
    {
        LevelManager.Instance.LoadMainMenu();
    }

    public void Next()
    {
        if (LevelManager.Instance.IsNextLevelAvailable())
        {
            LevelManager.Instance.LoadNextLevel();
        }
        else
        {
            LevelManager.Instance.LoadCredits();
        }
    }


    public Sprite MusicOff;
    public Image Toggle;

    public void ToggleMusic()
    {
        var cur = PlayerPrefs.GetInt("Music",1) == 1;
        var neew = !cur;
        if (neew)
        {
            Toggle.overrideSprite = null;
        }
        else
        {
            Toggle.overrideSprite = MusicOff;
        }
        SoundManager.SetMusicMuted(!neew);
        PlayerPrefs.SetInt("Music", neew? 1:0);
        PlayerPrefs.Save();
    }

    
}
