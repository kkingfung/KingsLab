﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEffect : MonoBehaviour
{
    public int EffectID = 0;
    public int uiID = 0;//For any purposes
    public float magnitude = 0;
    public Camera targetCam = null;

    private Text text;
    private Slider slider;
    private Image image;
    private TextMesh textMesh;
    private SpriteRenderer spr;

    private Vector3 oriPos;
    private Vector3 oriPosRect;
    private Vector3 oriRot;
    private Vector3 oriScale;
    private Color oriColour;

    private float alpha;

    private string fullText;
    private int textCnt;

    private bool Orientation;

    private StageSelectOperation sceneManager;

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
        
        sceneManager = FindObjectOfType<StageSelectOperation>();

        oriPos = this.transform.localPosition;
        if(this.GetComponent<RectTransform>())
        oriPosRect = this.GetComponent<RectTransform>().localPosition;
        oriRot = this.transform.localEulerAngles;
        oriScale = this.transform.localScale;
        alpha = 0f;
        textCnt = 0;
        if (textMesh) fullText = textMesh.text;
        Orientation = Screen.width > Screen.height;

    }

    // Update is called once per frame
    private void Update()
    {
            switch (EffectID) {
            case 0://for Title Scene Instruction
                if (text) text.color =new Color (text.color.r, text.color.g, text.color.b,Mathf.Sin(Time.time*8.0f));
                break;
            case 1://for Selection Scene Arrow Horizontal
                this.GetComponent<RectTransform>().localPosition = oriPosRect + Mathf.Sin(Time.time) * magnitude * targetCam.transform.up;   
                break;
            case 2://for Selection Scene Arrow Vertical
                this.GetComponent<RectTransform>().localPosition = oriPosRect + Mathf.Sin(Time.time) * magnitude * targetCam.transform.right;
                break;
            case 3://for Option Canva Gyro
                if (slider) this.transform.localEulerAngles = new Vector3(
                      oriRot.x, oriRot.y, oriRot.z - 360f * slider.value);
                break;
            case 4://for Selection Scene Custom Island Information
                if (spr == null) break;
                alpha = (sceneManager.CurrentIslandNum() == sceneManager.NextIslandNum()
                    && sceneManager.CurrentIslandNum() == uiID) ? (alpha < 1f ? alpha + .1f : 1f) : 0;
                spr.color = new Color(oriColour.r, oriColour.g, oriColour.b, alpha);
                break;
            case 5://for Selection Scene Island Information
                if (sceneManager == null || textMesh==null) break;
                textCnt = (sceneManager.CurrentIslandNum()==uiID) ? Mathf.Min( textCnt + 1,fullText.Length) : 0;
                textMesh.text = fullText.Substring(0, textCnt);
                break;
            case 6://for Selection Scene Boss Spr
                if (sceneManager == null || spr == null) break;
                spr.color = (sceneManager.EnabledtIslandNum()  -1 > uiID) ? oriColour : new Color(0, 0, 0, 1);
                break;
            case 7://for Selection Scene Clear Mark
                if (sceneManager == null || image == null) break;
                image.color = (sceneManager.EnabledtIslandNum() - 1 > uiID) ? oriColour : new Color(0, 0, 0, 0);
                break;
            case 8://for Selection Scene Boss Frame
                this.transform.localScale = oriScale - Mathf.Sin(Time.time*12.0f) * new Vector3(0.1f, 0.1f, 0);
                break;
        }
    }
}
