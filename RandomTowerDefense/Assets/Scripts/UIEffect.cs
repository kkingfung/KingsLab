using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEffect : MonoBehaviour
{
    public int EffectID = 0;
    Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (EffectID) {
            case 0:
                if (text) text.color =new Color (text.color.r, text.color.g, text.color.b,Mathf.Sin(Time.time*8.0f));
                break;
        }
    }
}
