﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

public class GameController : MonoBehaviour
{
    public GameObject FlagPrefab;
    private GameObject _flag;

    public static GameController Instance;

    private bool _gameEnded;

    public enum GameStates
    {
        None,
        Play,
        Pause,
        GameOver
    }

    public GameStates State;

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        SoundManager.PlayMusic("music_gameplay");
        State = GameStates.Play;
    }

    void Update()
    {
        if (State == GameStates.Play)
        {
            CheckLoose();
        }
    }

    public bool IsGameFinished()
    {
        return State == GameStates.GameOver;
    }

    void CheckLoose()
    {
        var chars = GetAllControledCharacters();
        if (chars.Count == 0)
        {
            State = GameStates.GameOver;
            SoundManager.StopMusic();
            SoundManager.PlaySound("gameOver_loose");
            InGameMenu.Instance.ShowLooseMenu();
        }
    }

    public void Victory()
    {
        if (State != GameStates.Play)
            return;

        State = GameStates.GameOver;
        SoundManager.StopMusic();
        SoundManager.PlaySound("gameOver_win");
        InGameMenu.Instance.ShowVictoryMenu();


        var chars = GetAllControledCharacters();
        SessionStats.Instance.SavedBugs.Clear();
        foreach (var characterBase in chars)
        {
            SessionStats.Instance.SavedBugs.Add(characterBase.GetType());
        }
    }

    public void ClickedOnGround(Vector3 clickPosition)
    {
        MoveDestinationForCharacters(clickPosition);
    }

    public void ClickedOnEnemy(EnemyBase enemy)
    {
        var chars = GetAllControledCharacters();
        foreach (var characterBase in chars)
        {
            characterBase.Enemy = enemy;
        }
    }

    private void MoveDestinationForCharacters(Vector3 clickPosition)
    {
        SoundManager.PlaySound("bug_command");
        ShowFlagInPosition(clickPosition);
        CancelInvoke("HideFlag");
        Invoke("HideFlag", 2f);

        var chars = GetAllControledCharacters();
        foreach (var characterBase in chars)
        {
            characterBase.Destination = clickPosition;
        }
    }

    void ShowFlagInPosition(Vector3 clickPosition)
    {
        if (_flag == null)
        {
            _flag = Instantiate(FlagPrefab);
            _flag.transform.localScale = Vector3.one;
            _flag.transform.rotation = Quaternion.identity;
        }

        _flag.transform.position = clickPosition;
        _flag.SetActive(false); // Need this to refresh Animation
        _flag.SetActive(true);
    }

    void HideFlag()
    {
        _flag.SetActive(false);
    }


    #region Utils
    public static List<CharacterBase> GetAllControledCharacters()
    {
        List<CharacterBase> controlledCharacters = new List<CharacterBase>();

        foreach (var character in CharacterBase.AllCharacter)
        {
            if (character.CurrentState == CharacterBase.CharacterState.Possessed)
            {
                controlledCharacters.Add(character);
            }
        }

        return controlledCharacters;
    }
    #endregion // Utils
}
