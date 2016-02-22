using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour
{
    public Camera Camera;

	// Use this for initialization
	void Start ()
    {
	    if(Camera == null) Camera = Camera.main;
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        transform.LookAt(Camera.transform);
    }
}
