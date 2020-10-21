﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

public class RaycastFunction : MonoBehaviour
{
    public int ActionID;
    public int InfoID;

    StageSelectOperation SelectionSceneManager;
    InGameOperation GameSceneManager;

    StoreManager storeManager;

    private enum ActionTypeID {
        StageSelection_Keybroad=0,
        StageSelection_PreviousStage,
        StageSelection_NextStage,

        GameScene_UpArrow = 10,
        GameScene_DownArrow,
        GameScene_LeftArrow,
        GameScene_RightArrow,

        GameScene_StoreArmy1 = 20, //Nightmare
        GameScene_StoreArmy2,//SoulEater
        GameScene_StoreArmy3,//TerrorBringer
        GameScene_StoreArmy4,//Usurper

        GameScene_StoreCastleHP = 30,
        GameScene_StoreBonusBoss1,//Green Metalon
        GameScene_StoreBonusBoss2,//Purple Metalon
        GameScene_StoreBonusBoss3,//Red Metalon

        GameScene_StoreMagicMeteor = 40,//Fire
        GameScene_StoreMagicBlizzard,//Ice
        GameScene_StoreMagicSummon,//Metal
        GameScene_StoreMagicPetrification,//Mind
    }

    private void Start()
    {
        SelectionSceneManager = FindObjectOfType<StageSelectOperation>();
        GameSceneManager = FindObjectOfType<InGameOperation>();

        storeManager = FindObjectOfType<StoreManager>();
    }

    public void ActionFunc()
    {
        if (SelectionSceneManager == null) return;
        switch ((ActionTypeID)ActionID)
        {
            //For Stage Selection Scene
            case ActionTypeID.StageSelection_Keybroad:
                SelectionSceneManager.TouchKeybroad(InfoID);
                break;
            case ActionTypeID.StageSelection_PreviousStage:
                SelectionSceneManager.StageInfoChgSubtract(InfoID);
                break;
            case ActionTypeID.StageSelection_NextStage:
                SelectionSceneManager.StageInfoChgAdd(InfoID);
                break;

            //For Game Scene (Screen to See)
            case ActionTypeID.GameScene_UpArrow:
                GameSceneManager.currScreenShown = (int)InGameOperation.ScreenShownID.SSIDTop;
                break;
            case ActionTypeID.GameScene_DownArrow:
                GameSceneManager.currScreenShown = (int)InGameOperation.ScreenShownID.SSIDArena;
                break;
            case ActionTypeID.GameScene_LeftArrow:
                GameSceneManager.currScreenShown = 
                    (GameSceneManager.currScreenShown == (int)InGameOperation.ScreenShownID.SSIDTopRight) ?
                    (int)InGameOperation.ScreenShownID.SSIDTop : (int)InGameOperation.ScreenShownID.SSIDTopLeft;
                break;
            case ActionTypeID.GameScene_RightArrow:
                GameSceneManager.currScreenShown =
                (GameSceneManager.currScreenShown == (int)InGameOperation.ScreenShownID.SSIDTopLeft) ?
                (int)InGameOperation.ScreenShownID.SSIDTop : (int)InGameOperation.ScreenShownID.SSIDTopRight;
                break;

            //For Game Scene (Store Left)
            case ActionTypeID.GameScene_StoreArmy1:
                storeManager.raycastAction(Upgrades.StoreItems.Army1,InfoID);
                break;
            case ActionTypeID.GameScene_StoreArmy2:
                storeManager.raycastAction(Upgrades.StoreItems.Army2, InfoID);
                break;
            case ActionTypeID.GameScene_StoreArmy3:
                storeManager.raycastAction(Upgrades.StoreItems.Army3, InfoID);
                break;
            case ActionTypeID.GameScene_StoreArmy4:
                storeManager.raycastAction(Upgrades.StoreItems.Army4, InfoID);
                break;

            //For Game Scene (Store Top)
            case ActionTypeID.GameScene_StoreCastleHP:
                storeManager.raycastAction(Upgrades.StoreItems.CastleHP, InfoID);
                break;
            case ActionTypeID.GameScene_StoreBonusBoss1:
                storeManager.raycastAction(Upgrades.StoreItems.BonusBoss1, InfoID);
                break;
            case ActionTypeID.GameScene_StoreBonusBoss2:
                storeManager.raycastAction(Upgrades.StoreItems.BonusBoss2, InfoID);
                break;
            case ActionTypeID.GameScene_StoreBonusBoss3:
                storeManager.raycastAction(Upgrades.StoreItems.BonusBoss3, InfoID);
                break;

            //For Game Scene (Store Right)
            case ActionTypeID.GameScene_StoreMagicMeteor:
                storeManager.raycastAction(Upgrades.StoreItems.MagicMeteor, InfoID);
                break;
            case ActionTypeID.GameScene_StoreMagicBlizzard:
                storeManager.raycastAction(Upgrades.StoreItems.MagicBlizzard, InfoID);
                break;
            case ActionTypeID.GameScene_StoreMagicSummon:
                storeManager.raycastAction(Upgrades.StoreItems.MagicSummon, InfoID);
                break;
            case ActionTypeID.GameScene_StoreMagicPetrification:
                storeManager.raycastAction(Upgrades.StoreItems.MagicPetrification, InfoID);
                break;

        }
    }
}
