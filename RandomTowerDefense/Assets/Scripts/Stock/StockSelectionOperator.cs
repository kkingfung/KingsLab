using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockSelectionOperator : MonoBehaviour
{
    bool isTouch;
    Vector2 DragRefPos;
    private void Start()
    {
        isTouch = FindObjectOfType<InputManager>().GetUseTouch();
        DragRefPos = FindObjectOfType<PlayerManager>().GetDragPos();
    }
    // Update is called once per frame
    void Update()
    {
        if (isTouch && Input.touchCount > 0)
            transform.localEulerAngles = new Vector3(0, 0, 
                (-90f+ Mathf.Rad2Deg * Mathf.Atan2(Input.touches[0].position.y-DragRefPos.y, Input.touches[0].position.x - DragRefPos.x)));
        if (!isTouch)
            transform.localEulerAngles = new Vector3(0, 0,
                (-90f + Mathf.Rad2Deg * Mathf.Atan2(Input.mousePosition.y - DragRefPos.y, Input.mousePosition.x - DragRefPos.x)));
    }
}
