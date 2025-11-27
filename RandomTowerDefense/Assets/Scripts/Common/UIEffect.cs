using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RandomTowerDefense.Common
{
    /// <summary>
    /// UIエフェクトユーティリティ - 汎用UIアニメーションとインタラクション効果
    ///
    /// 主な機能:
    /// - 12種類UIエフェクト（タイトル、矢印、アニメーション、フェード等）
    /// - マルチレンダラー対応（Text、Image、TextMesh、SpriteRenderer等）
    /// - クロスプラットフォームタッチ・マウス入力対応
    /// - シーン固有UIアニメーション（タイトル、ステージ選択、ゲーム）
    /// - レイキャストベースインタラクション検知
    /// - 動的テキストアニメーションとタイプライター効果
    /// </summary>
    public class UIEffect : MonoBehaviour
{
    private readonly Vector3 enlargeVec = new Vector3(0.03f, 0.03f, 0);

    public int EffectID = 0;
    public int uiID = 0;//For any purposes
    public float magnitude = 0;
    public Camera targetCam = null;
    public Camera subCam = null;

    private Text text;
    private Slider slider;
    private Image image;
    private TextMesh textMesh;
    private SpriteRenderer spr;

    //private Vector3 oriPos;
    private Vector3 oriPosRect;
    private Vector3 oriRot;
    private Vector3 oriScale;
    private Color oriColour;

    private float alpha;

    private string fullText;
    private float textCnt;

    public List<GameObject> relatedObjs;

    private List<TextMesh> relatedObjsTextMesh;
    private List<SpriteRenderer> relatedObjsSpriteRenderer;
    private RectTransform rectTrans;
    private StageSelectOperation selectionSceneManager;
    private InGameOperation gameSceneManager;
    private PlayerManager playerManager;
    private InputManager inputManager;

    // Start is called before the first frame update
    private void Start()
    {
        text = this.GetComponent<Text>();
        slider = this.GetComponentInParent<Slider>();
        image = this.GetComponentInParent<Image>();
        if (image) oriColour = image.color;

        textMesh = this.GetComponentInParent<TextMesh>();
        spr = this.GetComponentInParent<SpriteRenderer>();
        if (image==null && spr) oriColour = spr.color;
        
        selectionSceneManager = FindObjectOfType<StageSelectOperation>();
        gameSceneManager= FindObjectOfType<InGameOperation>();
        playerManager = GameObject.FindObjectOfType<PlayerManager>();
        inputManager = GameObject.FindObjectOfType<InputManager>();
        //oriPos = this.transform.localPosition;
        if(this.GetComponent<RectTransform>())

        oriRot = this.transform.localEulerAngles;
        oriScale = this.transform.localScale;
        alpha = 0f;
        textCnt = 0;
        if (textMesh) fullText = textMesh.text;
        else if(text) fullText = text.text;

        relatedObjsTextMesh = new List<TextMesh>();
        relatedObjsSpriteRenderer = new List<SpriteRenderer>();
        foreach (GameObject i in relatedObjs)
        {
            relatedObjsTextMesh.Add(i.GetComponent<TextMesh>());
            relatedObjsSpriteRenderer.Add(i.GetComponent<SpriteRenderer>());
        }
        rectTrans = this.GetComponent<RectTransform>();
        if(rectTrans)
        oriPosRect = new Vector3(rectTrans.localPosition.x, rectTrans.localPosition.y, rectTrans.localPosition.z);

    }

    // Update is called once per frame
    private void Update()
    {
            switch (EffectID) {
            case -1://for Title Scene Record
                RaycastHit hit;
                Ray ray = new Ray() ;

                //Get Ray according to orientation
                if (Screen.width > Screen.height)
                {
                    if (inputManager.GetUseTouch())
                    {
                        if (Input.touchCount > 0)
                            ray = targetCam.ScreenPointToRay(Input.GetTouch(0).position);
                    }
                    else { ray = targetCam.ScreenPointToRay(Input.mousePosition); }

                }
                else
                {
                    if (inputManager.GetUseTouch())
                    {
                        if (Input.touchCount > 0)
                            ray = subCam.ScreenPointToRay(Input.GetTouch(0).position);
                    }
                    else { ray = subCam.ScreenPointToRay(Input.mousePosition);}
                }

                float temp;
                if (Physics.Raycast(ray, out hit, 200, LayerMask.GetMask("RecordBroad")))
                {
                    for (int i= 0; i < relatedObjs.Count; ++i)
                    {
                        if (relatedObjsTextMesh[i])
                            relatedObjsTextMesh[i].color = new Color(relatedObjsTextMesh[i].color.r, relatedObjsTextMesh[i].color.g, relatedObjsTextMesh[i].color.b, 1f);
                        else if (relatedObjsSpriteRenderer[i])
                            relatedObjsSpriteRenderer[i].color = new Color(relatedObjsSpriteRenderer[i].color.r, relatedObjsSpriteRenderer[i].color.g, relatedObjsSpriteRenderer[i].color.b, 1f);
                    }
                }
                else 
                {
                    for (int i = 0; i < relatedObjs.Count; ++i)
                    {
                        if (relatedObjsTextMesh[i])
                        {
                            temp = Mathf.Max(0.0f, relatedObjsTextMesh[i].color.a - 0.005f);
                            relatedObjsTextMesh[i].color = new Color(relatedObjsTextMesh[i].color.r, relatedObjsTextMesh[i].color.g, relatedObjsTextMesh[i].color.b, temp);
                        }
                        else if (relatedObjsSpriteRenderer[i])
                        {
                            temp = Mathf.Max(0.0f, relatedObjsSpriteRenderer[i].color.a - 0.005f);
                            relatedObjsSpriteRenderer[i].color = new Color(relatedObjsSpriteRenderer[i].color.r, relatedObjsSpriteRenderer[i].color.g, relatedObjsSpriteRenderer[i].color.b, temp);
                        }
                    }
                }
                break;
            case 0://for Title Scene Instruction
                if (text) text.color =new Color (text.color.r, text.color.g, text.color.b,Mathf.Abs(Mathf.Sin(Time.time* magnitude)));
                break;
            case 1://for Selection Scene Arrow Vertical
                rectTrans.localPosition = oriPosRect + Mathf.Sin(Time.time) * magnitude * targetCam.transform.forward;   
                break;
            case 2://for Selection Scene Arrow Horizontal
                rectTrans.localPosition = oriPosRect + Mathf.Sin(Time.time) * magnitude * targetCam.transform.right;
                break;
            case 3://for Option Canva Gyro
                if (slider) this.transform.localEulerAngles = new Vector3(
                      oriRot.x, oriRot.y, oriRot.z - 360f * slider.value);
                break;
            case 4://for Selection Scene Custom Island Information
                if (spr == null) break;
                alpha = (selectionSceneManager.CurrentIslandNum() == selectionSceneManager.NextIslandNum()
                    && selectionSceneManager.CurrentIslandNum() == uiID) ? (alpha < 1f ? alpha + .1f : 1f) : 0;
                spr.color = new Color(oriColour.r, oriColour.g, oriColour.b, alpha);
                break;
            case 5://for Selection Scene Island Information
                if (selectionSceneManager == null || textMesh==null) break;
                textCnt = (selectionSceneManager.CurrentIslandNum()==uiID) ? Mathf.Min( textCnt + 1,fullText.Length) : 0;
                textMesh.text = fullText.Substring(0, (int)textCnt);
                break;
            case 6://for Selection Scene Boss Spr
                if (selectionSceneManager == null || spr == null) break;
                //spr.color = (selectionSceneManager.EnabledtIslandNum()  -1 > uiID) ? oriColour : Color.black;
                spr.color = Color.black;
                break;
            case 7://for Selection Scene Clear Mark
                if (selectionSceneManager == null || image == null) break;
                image.color = (selectionSceneManager.EnabledtIslandNum() - 1 > uiID) ? oriColour : Color.clear;
                break;
            case 8://for Selection Scene Boss Frame
                //this.transform.localScale = oriScale - Mathf.Sin(Time.time*12.0f) * enlargeVec;
                break;
            case 9://for Game Scene SellingMark
                if (playerManager.isSelling && gameSceneManager.GetOptionStatus()!=true && gameSceneManager.currScreenShown == (int)InGameOperation.ScreenShownID.SSIDArena)
                {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Abs(Mathf.Sin(Time.time * magnitude)));
                }
                else {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
                }
                break;
            case 10://for Game Scene Wave Num
                if (text && text.color.a > 0) {
                    text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a- magnitude);
                }
                break;
            case 11://for Game Scene Score Information
                if (text == null) break;
                textCnt =  Mathf.Min(textCnt + 0.5f, fullText.Length);
                text.text = fullText.Substring(0, (int)textCnt);
                break;
        }
    }

    public void ResetForScoreShow()
    {
        textCnt = 0;
        if (text) fullText = text.text;
    }
    }
}
