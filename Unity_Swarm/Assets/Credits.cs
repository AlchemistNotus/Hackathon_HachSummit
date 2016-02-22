using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{

    public Text LabelTime;

    void Start()
    {
        LabelTime.text = (SessionStats.Instance.End12Level - SessionStats.Instance.Start1Level).ToString();
    }
}
