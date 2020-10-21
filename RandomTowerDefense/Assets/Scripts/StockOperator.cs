using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockOperator : MonoBehaviour
{
    private readonly Vector3 TargetScale = new Vector3(5, 5, 5);
    private readonly int Rotation = 5;

    InputManager inputManager;

    // Start is called before the first frame update
    void Start()
    {
        inputManager = FindObjectOfType<InputManager>();
        StartCoroutine(StartAnimation());
    }

    // Update is called once per frame
    void Update()
    {
        if (inputManager && inputManager.GetDraggingStatus() == false) Destroy(this.gameObject);   
    }

    private IEnumerator StartAnimation()
    {
        int frame = 30;
        float rotateChgsbyFrame = (Rotation * 360f - this.transform.localEulerAngles.z) / frame;
        float scaleChgsbyFrame = (TargetScale.x - this.transform.localScale.x) / frame;
        while (frame-- > 0)
        {
            this.transform.localEulerAngles = new Vector3(this.transform.localEulerAngles.x,
                this.transform.localEulerAngles.y, this.transform.localEulerAngles.z+ rotateChgsbyFrame);
            this.transform.localScale = new Vector3(this.transform.localScale.x + scaleChgsbyFrame,
    this.transform.localScale.y + scaleChgsbyFrame, this.transform.localScale.z + scaleChgsbyFrame);
            yield return new WaitForSeconds(0f);
        }
    }
}
