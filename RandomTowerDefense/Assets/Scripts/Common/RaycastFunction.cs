using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.TerrainAPI;

namespace RandomTowerDefense.Common
{
    /// <summary>
    /// レイキャストベースのインタラクションとUIエフェクトを管理
    /// </summary>
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
    private PlayerManager playerManager;

    private StoreManager storeManager;
    private AudioManager audioManager;

    private MeshRenderer chkMeshRender;
    private RawImage chkRawImg;
    private SpriteRenderer chkSprRender;

    private Color oriColor;
    private bool ColorRoutineRunning;

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
        ColorRoutineRunning = false;

    }
    private void Start()
    {
        SelectionSceneManager = FindObjectOfType<StageSelectOperation>();
        sceneManager = FindObjectOfType<InGameOperation>();
        playerManager = FindObjectOfType<PlayerManager>();
        storeManager = FindObjectOfType<StoreManager>();
        audioManager = FindObjectOfType<AudioManager>();

        if (rendertype == EnumRenderType.NotChecked)
            GetColor();
    }

    private void DarkenStoreSpr() {
        if (playerManager.hitStore == null || this.transform != playerManager.hitStore)
        {
            switch (rendertype)
            {
                case EnumRenderType.FoundMeshRenderer:
                    chkMeshRender.material.color = new Color(Mathf.Clamp(chkMeshRender.material.color.r, 0, 0.3f),
                        Mathf.Clamp(chkMeshRender.material.color.g, 0, 0.3f), Mathf.Clamp(chkMeshRender.material.color.b, 0, 0.3f)); break;
                case EnumRenderType.FoundRawImg:
                    chkRawImg.color = new Color(Mathf.Clamp(chkRawImg.color.r, 0, 0.3f), 
                        Mathf.Clamp(chkRawImg.color.g, 0, 0.3f), Mathf.Clamp(chkRawImg.color.b, 0, 0.3f)); break;
                case EnumRenderType.FoundSprRenderer:
                    chkSprRender.color = new Color(Mathf.Clamp(chkSprRender.color.r, 0, 0.3f), 
                        Mathf.Clamp(chkSprRender.color.g, 0, 0.3f), Mathf.Clamp(chkSprRender.color.b, 0, 0.3f)); break;
            }
        }
        else 
        {
            if (ColorRoutineRunning==false) 
            {
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
    }

    private void LateUpdate()
    {
        switch ((ActionTypeID)ActionID)
        {
            //For Game Scene (Store Left)
            case ActionTypeID.GameScene_StoreArmy1:
            case ActionTypeID.GameScene_StoreArmy2:
            case ActionTypeID.GameScene_StoreArmy3:
            case ActionTypeID.GameScene_StoreArmy4:
                DarkenStoreSpr();break;
            //For Game Scene (Store Top)
            case ActionTypeID.GameScene_StoreCastleHP:
            case ActionTypeID.GameScene_StoreBonusBoss1:
            case ActionTypeID.GameScene_StoreBonusBoss2:
            case ActionTypeID.GameScene_StoreBonusBoss3:
                DarkenStoreSpr(); break;

            //For Game Scene (Store Right)
            case ActionTypeID.GameScene_StoreMagicMeteor:
            case ActionTypeID.GameScene_StoreMagicBlizzard:
            case ActionTypeID.GameScene_StoreMagicMinions:
            case ActionTypeID.GameScene_StoreMagicPetrification:
                DarkenStoreSpr(); break;
        }
    }

    public Color GetColor() {
        chkMeshRender = GetComponent<MeshRenderer>();
        if (chkMeshRender)
        {
            oriColor = chkMeshRender.material.color;
            rendertype = EnumRenderType.FoundMeshRenderer;
        }
        else {
            chkRawImg = GetComponent<RawImage>();
            if (chkRawImg)
            {
                oriColor = chkRawImg.color;
                rendertype = EnumRenderType.FoundRawImg;
            }
            else
            {
                chkSprRender = GetComponent<SpriteRenderer>();
                if (chkSprRender)
                {
                    oriColor = chkSprRender.color;
                    rendertype = EnumRenderType.FoundSprRenderer;
                }
            }
        }
        return oriColor;
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
        ColorRoutineRunning = true;
           Color color=new Color();
        switch (rendertype) {
            case EnumRenderType.NotChecked:
                color=GetColor();break;
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
            reqTime += Time.deltaTime * 3f;
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
        ColorRoutineRunning = false;
    }
}
}
