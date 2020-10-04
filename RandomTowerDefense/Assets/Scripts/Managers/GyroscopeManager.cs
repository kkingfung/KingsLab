using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GyroscopeManager : MonoBehaviour
{
    [Range(0,1)]
    float sensitivity=1f;
    const float factor = 10f;
    public List<Slider> senseSlider;
    public bool isFunctioning=true;
    private void OnEnable()
    {
        sensitivity = PlayerPrefs.GetFloat("Gyro");
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat("Gyro", sensitivity);
    }

    public void SetGyro(float sensitivity) {
        this.sensitivity = sensitivity;
    }

    private void Update()
    {
        if (isFunctioning)
            foreach (Slider i in senseSlider)
                i.value = sensitivity;
    }
}
