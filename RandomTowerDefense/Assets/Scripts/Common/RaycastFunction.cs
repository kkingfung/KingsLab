﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.TerrainAPI;

public class RaycastFunction : MonoBehaviour
{
    private enum EnumRenderType {
        NotChecked = 0,
        FoundMeshRenderer,
        FoundRawImg,
        FoundSprRenderer,
    }
    public int ActionID;
    public int InfoID;
    private EnumRenderType rendertype;
    private StageSelectOperation SelectionSceneManager;
    private InGameOperation sceneManager;

    private StoreManager storeManager;
    private AudioManager audioManager;

    private MeshRenderer chkMeshRender;
    private RawImage chkRawImg;
    private SpriteRenderer chkSprRender;

    private Color oriColor;
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
        GameScene_StoreMagicPetrification,//Mind
        GameScene_StoreMagicMinions,//Metal
    }

    private void Awake()
    {
        rendertype = EnumRenderType.NotChecked;
        oriColor = Color.white;

    }
    private void Start()
    {
        SelectionSceneManager = FindObjectOfType<StageSelectOperation>();
        sceneManager = FindObjectOfType<InGameOperation>();

        storeManager = FindObjectOfType<StoreManager>();
        audioManager = FindObjectOfType<AudioManager>();

        if (rendertype == EnumRenderType.NotChecked)
            GetColor(oriColor);
    }

    public void GetColor(Color color) {
        chkMeshRender = GetComponent<MeshRenderer>();
        if (chkMeshRender)
        {
            color = chkMeshRender.material.color;
            rendertype = EnumRenderType.FoundMeshRenderer;
        }
        else {
            chkRawImg = GetComponent<RawImage>();
            if (chkRawImg)
            {
                color = chkRawImg.color;
                rendertype = EnumRenderType.FoundRawImg;
            }
            else
            {
                chkSprRender = GetComponent<SpriteRenderer>();
                if (chkSprRender)
                {
                    color = chkSprRender.color;
                    rendertype = EnumRenderType.FoundSprRenderer;
                }
            }
        }
    }
    public void ActionFunc()
    {
        if (SelectionSceneManager == null && sceneManager==null) return;

        if (sceneManager&&sceneManager.GetOptionStatus()) return;
        if (SelectionSceneManager && SelectionSceneManager.GetOptionStatus()) return;
        switch ((ActionTypeID)ActionID)
        {
            //For Stage Selection Scene
            case ActionTypeID.StageSelection_Keybroad:
                SelectionSceneManager.TouchKeybroad(InfoID);
                break;
            case ActionTypeID.StageSelection_PreviousStage:
                SelectionSceneManager.StageInfoChgSubtract(InfoID);
                audioManager.PlayAudio("se_Button");
                break;
            case ActionTypeID.StageSelection_NextStage:
                SelectionSceneManager.StageInfoChgAdd(InfoID);
                audioManager.PlayAudio("se_Button");
                break;

            //For Game Scene (Screen to See)
            case ActionTypeID.GameScene_UpArrow:
                sceneManager.nextScreenShown = (int)InGameOperation.ScreenShownID.SSIDTop;
                audioManager.PlayAudio("se_Button");
                break;
            case ActionTypeID.GameScene_DownArrow:
                sceneManager.nextScreenShown = (int)InGameOperation.ScreenShownID.SSIDArena;
                audioManager.PlayAudio("se_Button");
                break;
            case ActionTypeID.GameScene_LeftArrow:
                sceneManager.nextScreenShown = 
                    (sceneManager.currScreenShown == (int)InGameOperation.ScreenShownID.SSIDTopRight) ?
                    (int)InGameOperation.ScreenShownID.SSIDTop : (int)InGameOperation.ScreenShownID.SSIDTopLeft;
                audioManager.PlayAudio("se_Button");
                break;
            case ActionTypeID.GameScene_RightArrow:
                sceneManager.nextScreenShown =
                (sceneManager.currScreenShown == (int)InGameOperation.ScreenShownID.SSIDTopLeft) ?
                (int)InGameOperation.ScreenShownID.SSIDTop : (int)InGameOperation.ScreenShownID.SSIDTopRight;
                audioManager.PlayAudio("se_Button");
                break;

            //For Game Scene (Store Left)
            case ActionTypeID.GameScene_StoreArmy1:
                storeManager.raycastAction(Upgrades.StoreItems.Army1,InfoID);
                audioManager.PlayAudio("se_Button");
                break;
            case ActionTypeID.GameScene_StoreArmy2:
                storeManager.raycastAction(Upgrades.StoreItems.Army2, InfoID);
                audioManager.PlayAudio("se_Button");
                break;
            case ActionTypeID.GameScene_StoreArmy3:
                storeManager.raycastAction(Upgrades.StoreItems.Army3, InfoID);
                audioManager.PlayAudio("se_Button");
                break;
            case ActionTypeID.GameScene_StoreArmy4:
                storeManager.raycastAction(Upgrades.StoreItems.Army4, InfoID);
                audioManager.PlayAudio("se_Button");
                break;

            //For Game Scene (Store Top)
            case ActionTypeID.GameScene_StoreCastleHP:
                storeManager.raycastAction(Upgrades.StoreItems.CastleHP, InfoID);
                audioManager.PlayAudio("se_Button");
                break;
            case ActionTypeID.GameScene_StoreBonusBoss1:
                storeManager.raycastAction(Upgrades.StoreItems.BonusBoss1, InfoID);
                audioManager.PlayAudio("se_Button");
                break;
            case ActionTypeID.GameScene_StoreBonusBoss2:
                storeManager.raycastAction(Upgrades.StoreItems.BonusBoss2, InfoID);
                audioManager.PlayAudio("se_Button");
                break;
            case ActionTypeID.GameScene_StoreBonusBoss3:
                storeManager.raycastAction(Upgrades.StoreItems.BonusBoss3, InfoID);
                audioManager.PlayAudio("se_Button");
                break;

            //For Game Scene (Store Right)
            case ActionTypeID.GameScene_StoreMagicMeteor:
                storeManager.raycastAction(Upgrades.StoreItems.MagicMeteor, InfoID);
                audioManager.PlayAudio("se_Button");
                break;
            case ActionTypeID.GameScene_StoreMagicBlizzard:
                storeManager.raycastAction(Upgrades.StoreItems.MagicBlizzard, InfoID);
                audioManager.PlayAudio("se_Button");
                break;
            case ActionTypeID.GameScene_StoreMagicMinions:
                storeManager.raycastAction(Upgrades.StoreItems.MagicMinions, InfoID);
                audioManager.PlayAudio("se_Button");
                break;
            case ActionTypeID.GameScene_StoreMagicPetrification:
                storeManager.raycastAction(Upgrades.StoreItems.MagicPetrification, InfoID);
                audioManager.PlayAudio("se_Button");
                break;

        }
        StartCoroutine(ColorRoutine());
    }

    private IEnumerator ColorRoutine()
    {
        Color color=new Color();
        switch (rendertype) {
            case EnumRenderType.NotChecked:
                GetColor(color);break;
            case EnumRenderType.FoundMeshRenderer:
                color = chkMeshRender.material.color; break;
            case EnumRenderType.FoundRawImg:
                color = chkRawImg.color; break;
            case EnumRenderType.FoundSprRenderer:
                color = chkSprRender.color; break;
        }

        color.r = 0; color.g = 0; color.b = 0;
        float reqTime =0;
        while (reqTime < 1f)
        {
            reqTime += Time.deltaTime;
            color.r = reqTime * oriColor.r;
            color.g = reqTime * oriColor.g;
            color.b = reqTime * oriColor.b;
            switch (rendertype)
            {
                case EnumRenderType.FoundMeshRenderer:
                    chkMeshRender.material.color=color; break;
                case EnumRenderType.FoundRawImg:
                    chkRawImg.color = color; break;
                case EnumRenderType.FoundSprRenderer:
                    chkSprRender.color = color; break;
            }
            yield return new WaitForSeconds(0f);
        }

        switch (rendertype)
        {
            case EnumRenderType.FoundMeshRenderer:
                chkMeshRender.material.color = oriColor; break;
            case EnumRenderType.FoundRawImg:
                chkRawImg.color = oriColor; break;
            case EnumRenderType.FoundSprRenderer:
                chkSprRender.color = oriColor; break;
        }
    }
}
