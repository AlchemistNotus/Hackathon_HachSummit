using UnityEngine;
using System.Collections;

public class SfxFactory : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public static GameObject CreateSfx(Object prefab, Vector3 position, float delayDeath)
    {
        if (prefab == null)
            return null;
        GameObject go = (GameObject) Instantiate(prefab, position, Quaternion.identity);
        Destroy(go, delayDeath);
        return go;
    }

    public static GameObject CreateSfx(Object prefab, Vector3 position, Vector3 rotation, float delayDeath)
    {
        if (prefab == null)
            return null;
        GameObject go = (GameObject)Instantiate(prefab, position, Quaternion.Euler(rotation));
        Destroy(go, delayDeath);
        return go;
    }
}
