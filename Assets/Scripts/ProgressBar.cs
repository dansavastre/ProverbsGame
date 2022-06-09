using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{

    private Slider slider;

    public static float FillSpeed = 0.5f;
    public static float target = 0;

    private void Awake()
    {
        slider = gameObject.GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(target > 1f)
        {
            target = 1f;
        }
        if(target < 0f)
        {
            target = 0f;
        }
        if(slider.value == 0f)
        {
            slider.fillRect.gameObject.SetActive(false);
        }
        if(target > slider.value)
        {
            slider.fillRect.gameObject.SetActive(true);
            slider.value += FillSpeed * Time.deltaTime;
        }
        else if(target < slider.value)
        {
            slider.value = target;
        }
    }

    public void UpdateProgress(float newProgress)
    {
        target = newProgress;
    }

    public void SetProgress(float newProgress)
    {
        slider.value = newProgress;
        target = slider.value;
    }
}
