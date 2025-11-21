using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AutoDestroyVFX : MonoBehaviour
{
    private VisualEffect ps;
    public float Timer;
    private bool tobeDestroy;
    public Skill skill;
    private void OnEnable()
    {
        ps = GetComponent<VisualEffect>();
        ps.Play();
        skill = gameObject.GetComponent<Skill>();
        tobeDestroy = false;
    }
    private void Update()
    {
        Timer = Timer + (tobeDestroy ? Time.deltaTime : -1 * Time.deltaTime);
        if (tobeDestroy == false && Timer < 0)
        {
            ps.Stop();
            //No ps.isPlaying in VFX
            tobeDestroy = true;
            if (skill != null)
            {
                Destroy(gameObject, 3f);
            }
        }

        if (skill == null && 
            tobeDestroy == true && Timer > 2)
        {
            gameObject.SetActive(false);
        }
    }
}