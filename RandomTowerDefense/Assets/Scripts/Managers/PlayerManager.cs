using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public bool isDamageable = false;

    [HideInInspector]
    public InputManager input;
    [HideInInspector]
    public Vector3 velocity;

    // Start is called before the first frame update
    void Awake()
    {
        input = FindObjectOfType<InputManager>();
    }

}
