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
        DragRefPos = FindObjectOfType<InputManager>().GetDragPos();
    }
    // Update is called once per frame
    void Update()
    {
        if (isTouch && Input.touchCount > 0)
            transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg*Mathf.Acos(Vector2.Dot(Input.touches[0].position - DragRefPos, Vector2.up)));
        if (!isTouch)
            transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Acos(Vector2.Dot(new Vector2(Input.mousePosition.x, Input.mousePosition.y) - DragRefPos, Vector2.up)));
    }
}
