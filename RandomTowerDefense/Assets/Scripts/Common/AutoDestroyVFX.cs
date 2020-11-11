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
            if (ps.<=0)
            {
                Destroy(gameObject);
            }
        }
        else {
            ps = GetComponent<VisualEffect>();
        }
    }
}