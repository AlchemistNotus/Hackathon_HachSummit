using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GameCanvas : MonoBehaviour
{
    public static GameCanvas Instance;

    
    void OnEnable()
    {
        Instance = this;
    }
}
