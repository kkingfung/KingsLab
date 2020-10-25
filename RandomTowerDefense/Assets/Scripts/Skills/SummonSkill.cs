using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonSkill : MonoBehaviour
{
    private AudioSource audio;
    private AudioManager audioManager;
    //TODO: Ability Ratio

    // Start is called before the first frame update
    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        audio = GetComponent<AudioSource>();
        //audio.clip = audioManager.GetAudio("se_MagicSummon");
        //audio.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ResetforPlayer() {
        GameObject.FindObjectOfType<PlayerManager>().isSkillActive = false;
    }
}
