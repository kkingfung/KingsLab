using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AutoDestroyVFX : MonoBehaviour
{
    private VisualEffect ps;

    public void Update()
    {
        if (ps)
        {
           //No ps.isPlaying in VFX
        }
        else {
            ps = GetComponent<VisualEffect>();
            Destroy(gameObject,5);
        }
    }
}