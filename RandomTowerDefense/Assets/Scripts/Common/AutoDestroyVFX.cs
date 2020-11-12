using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AutoDestroyVFX : MonoBehaviour
{
    private VisualEffect ps;
    public float Timer;
    public void Update()
    {
        if (ps)
        {
            //No ps.isPlaying in VFX
            Destroy(gameObject, Timer);
        }
        else {
            ps = GetComponent<VisualEffect>();
        }
    }
}