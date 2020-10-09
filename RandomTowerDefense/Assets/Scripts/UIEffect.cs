using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class UIEffect : MonoBehaviour
{
    public int EffectID = 0;
    Text text;
    Vector3 oriPos;
    // Start is called before the first frame update
    void Start()
    {
        text = this.GetComponent<Text>();
        oriPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        switch (EffectID) {
            case 0:
                if (text) text.color =new Color (text.color.r, text.color.g, text.color.b,Mathf.Sin(Time.time*8.0f));
                break;
            case 1:
                this.transform.position = oriPos + Mathf.Sin(Time.time)*new Vector3(1,0,0);
                break;
            case 2:
                this.transform.position = oriPos - Mathf.Sin(Time.time) * new Vector3(1, 0, 0);
                break;
        }
    }
}
