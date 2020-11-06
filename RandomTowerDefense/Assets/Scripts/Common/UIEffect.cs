using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEffect : MonoBehaviour
{
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

    private Vector3 oriPos;
    private Vector3 oriPosRect;
    private Vector3 oriRot;
    private Vector3 oriScale;
    private Color oriColour;

    private float alpha;

    private string fullText;
    private int textCnt;

    [HideInInspector]
    public bool LandscapeOrientation;

    public List<GameObject> relatedObjs;

    private StageSelectOperation selectionSceneManager;
    private InGameOperation gameSceneManager;

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
        oriPos = this.transform.localPosition;
        if(this.GetComponent<RectTransform>())
        oriPosRect = this.GetComponent<RectTransform>().localPosition;
        oriRot = this.transform.localEulerAngles;
        oriScale = this.transform.localScale;
        alpha = 0f;
        textCnt = 0;
        if (textMesh) fullText = textMesh.text;
        else if(text) fullText = text.text;
        LandscapeOrientation = Screen.width > Screen.height;

    }

    // Update is called once per frame
    private void Update()
    {
            switch (EffectID) {
            case -1://for Title Scene Record
                RaycastHit hit;
                Ray ray;

                //Get Ray according to orientation
                if (Screen.width > Screen.height)
                {
                    if (Input.touchCount > 0)
                        ray = targetCam.ScreenPointToRay(Input.GetTouch(0).position);
                    else 
                        ray = targetCam.ScreenPointToRay(Input.mousePosition);
                }
                else
                {
                    if (Input.touchCount > 0)
                        ray = targetCam.ScreenPointToRay(Input.GetTouch(0).position);
                    else
                        ray = subCam.ScreenPointToRay(Input.mousePosition);
                }

                float temp;
                if ((Input.touchCount > 0 || (Input.mousePosition.x > 0 && Input.mousePosition.y > 0 && 
                    Input.mousePosition.x < Screen.width && Input.mousePosition.y < Screen.height)) &&
                        Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("RecordBroad")))
                {
                    foreach (GameObject i in relatedObjs) 
                    {
                        TextMesh textMesh = i.GetComponent<TextMesh>();
                        SpriteRenderer spr = i.GetComponent<SpriteRenderer>();
                        if (textMesh)
                            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 1f);
                        if (spr)
                            spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, 1f);
                    }
                }
                else 
                {
                    foreach (GameObject i in relatedObjs)
                    {
                        TextMesh textMesh = i.GetComponent<TextMesh>();
                        SpriteRenderer spr = i.GetComponent<SpriteRenderer>();
                        if (textMesh)
                        {
                            temp = Mathf.Max(0.0f, textMesh.color.a - 0.02f);
                            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, temp);
                        }
                        if (spr)
                        {
                            temp = Mathf.Max(0.0f, spr.color.a - 0.02f);
                            spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, temp);
                        }
                    }
                }
                break;
            case 0://for Title Scene Instruction
                if (text) text.color =new Color (text.color.r, text.color.g, text.color.b,Mathf.Abs(Mathf.Sin(Time.time* magnitude)));
                break;
            case 1://for Selection Scene Arrow Vertical
                this.GetComponent<RectTransform>().localPosition = oriPosRect + Mathf.Sin(Time.time) * magnitude * targetCam.transform.up;   
                break;
            case 2://for Selection Scene Arrow Horizontal
                this.GetComponent<RectTransform>().localPosition = oriPosRect + Mathf.Sin(Time.time) * magnitude * targetCam.transform.right;
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
                textMesh.text = fullText.Substring(0, textCnt);
                break;
            case 6://for Selection Scene Boss Spr
                if (selectionSceneManager == null || spr == null) break;
                spr.color = (selectionSceneManager.EnabledtIslandNum()  -1 > uiID) ? oriColour : new Color(0, 0, 0, 1);
                break;
            case 7://for Selection Scene Clear Mark
                if (selectionSceneManager == null || image == null) break;
                image.color = (selectionSceneManager.EnabledtIslandNum() - 1 > uiID) ? oriColour : new Color(0, 0, 0, 0);
                break;
            case 8://for Selection Scene Boss Frame
                this.transform.localScale = oriScale - Mathf.Sin(Time.time*12.0f) * new Vector3(0.1f, 0.1f, 0);
                break;
            case 9://for Game Scene SellingMark
                if (GameObject.FindObjectOfType<PlayerManager>().isSelling && gameSceneManager.GetOptionStatus()!=true && gameSceneManager.currScreenShown ==0)
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
                textCnt =  Mathf.Min(textCnt + 1, fullText.Length);
                text.text = fullText.Substring(0, textCnt);
                break;
        }
    }

    public void ResetForScoreShow()
    {
        textCnt = 0;
        if (text) fullText = text.text;
    }
}
