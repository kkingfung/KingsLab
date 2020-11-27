using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AutoDestroyVFX : MonoBehaviour
{
    private VisualEffect ps;
    public float Timer;
    private bool tobeDestroy;
    private void Start()
    {
        ps = GetComponent<VisualEffect>();
        tobeDestroy = false;
    }
    private void Update()
    {
        Timer -= Time.deltaTime;
        if (tobeDestroy==false && Timer < 0)
        {
            ps.Stop();
            //No ps.isPlaying in VFX
            tobeDestroy = true;
            Destroy(gameObject, 2f);
        }
    }
}