using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AutoDestroyVFX : MonoBehaviour
{
    private VisualEffect ps;
    public float Timer;

    private void Start()
    {
        ps = GetComponent<VisualEffect>();
    }
    private void Update()
    {
            //No ps.isPlaying in VFX
            Destroy(gameObject, Timer);
    }
}