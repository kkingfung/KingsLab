using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour 
{
    [HideInInspector]
    public bool isUIshown = true;
    bool isUIshownHistory = true;
     
    List<Renderer> allUI = new List<Renderer>();

    private void Start()
    {
        GameObject[] allObj = FindObjectsOfType<GameObject>();
        for (int i = 0; i < allObj.Length; i++)
        {
            if (allObj[i].layer == LayerMask.NameToLayer("UI")) {
                Renderer renderer = allObj[i].GetComponent<Renderer>();
                if (renderer) allUI.Add(renderer);
            }
        }
    }

    private void LateUpdate () {
        if (isUIshown != isUIshownHistory) {
            for (int i = 0; i < allUI.Count; i++) {
                allUI[i].enabled = isUIshown;
            }
            isUIshownHistory = isUIshown;
        }
    }
}