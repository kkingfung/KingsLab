using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

    float interactionInfoDisplayTimeRemaining;
    static UIManager instance;
    bool isUIshown = true;
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

    private void Update () {
        if (isUIshown != isUIshownHistory) {
            for (int i = 0; i < allUI.Count; i++) {
                allUI[i].enabled = isUIshown;
            }
            isUIshownHistory = isUIshown;
        }
    }

    public static void DisplayInteractionInfo (string info) {
        if (Instance) {
            Instance.interactionInfoDisplayTimeRemaining = 3;
        } else {
            Debug.Log ($"{info} (no UI instance found)");
        }
    }

    public static void CancelInteractionDisplay () {
        if (Instance) {
            Instance.interactionInfoDisplayTimeRemaining = 0;
        }
    }

    static UIManager Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<UIManager> ();
            }
            return instance;
        }
    }
}