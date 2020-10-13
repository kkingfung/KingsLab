using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

public class RaycastFunction : MonoBehaviour
{
    public int ActionID;
    public int InfoID;
    StageSelectOperation SceneManager;

    private void Start()
    {
        SceneManager = FindObjectOfType<StageSelectOperation>(); ;
    }
    public void ActionFunc()
    {
        if (SceneManager == null) return;

        switch (ActionID)
        {
            case 0:
                SceneManager.TouchKeybroad(InfoID);
                break;
            case 1:
                SceneManager.StageInfoChgSubtract(InfoID);
                break;
            case 2:
                SceneManager.StageInfoChgAdd(InfoID);
                break;
        }
    }
}
