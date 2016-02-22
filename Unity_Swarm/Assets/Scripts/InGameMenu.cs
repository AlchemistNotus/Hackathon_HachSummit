using UnityEngine;
using System.Collections;
using DG.Tweening;

public class InGameMenu : MonoBehaviour
{
    public static InGameMenu Instance;

    public GameObject LoosePanel;
    public GameObject VictoryPanel;

    void Awake()
    {
        Instance = this;
    }

    public void ShowLooseMenu()
    {
        LoosePanel.SetActive(true);
        LoosePanel.GetComponent<CanvasGroup>().alpha = 0;
        LoosePanel.GetComponent<CanvasGroup>().DOFade(1, 1);
    }

    public void ShowVictoryMenu()
    {
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

}
