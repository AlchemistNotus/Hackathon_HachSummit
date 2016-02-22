using System;
using UnityEngine;
using System.Collections;



/// <summary>
/// Usage example:
/// 
/// Settings.Instance.SeeSettingForDifficulties[0].BigBugDamage
/// </summary>

[CreateAssetMenu]
public class Settings : ScriptableObject
{
    private static Settings _instance;

    public static Settings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<Settings>("Settings");
            }

            return _instance;
        }
    }

    public int MaxSquad;
    [Header("bugs")]
    public float SmallBugHealth;
    public float MidBugHealth;
    public float BigBugHealth;
    public float SmallBugSpeed;
    public float MidBugSpeed;
    public float BigBugSpeed;
    public float SmallBugDamage;
    public float MidBugDamage;
    public float BigBugDamage;
    [Space(3)]
    public float FindEnemyDistance = 3;

    [Header("towers")]
    public float RocketHealth;
    public float MiniGunHealth;
    public float TeslaHealth;
    public float LaserHealth;

    public float RocketDamage;
    public float MiniGunDamage;
    public float TeslaDamage;
    public float LaserDamage;
}

