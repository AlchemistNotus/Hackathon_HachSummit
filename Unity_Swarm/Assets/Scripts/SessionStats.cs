using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SessionStats : MonoBehaviour
{
    #region Singleton
    private static SessionStats _instance;
    public static SessionStats Instance
    {
        get { return _instance ?? new GameObject("SessionStats").AddComponent<SessionStats>(); }
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

    public int BugsKilled;
    public int CurrentDifficulty;
    public DateTime Start1Level;
    public DateTime End12Level;

    public List<Type> SavedBugs = new List<Type>();
}
