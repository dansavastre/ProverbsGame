using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{

    private Slider slider; // reference to the progress bar

    // attributes for the tweaking the behaviour of the progress bar
    public static float FillSpeed = 0.5f;
    public static float target = 0;

    /// <summary>
    /// Executed when an instance of this class is initialized.
    /// </summary>
    private void Awake()
    {
        slider = gameObject.GetComponent<Slider>();
    }

    /// <summary>
    /// Executes on each frame update.
    /// </summary>
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

    /// <summary>
    /// Method for updating the progress on the bar.
    /// </summary>
    /// <param name="newProgress">a value denoting the progress that has now been reached</param>
    public void UpdateProgress(float newProgress)
    {
        target = newProgress;
    }

    /// <summary>
    /// Method for setting the progress on the bar to a certain bar.
    /// </summary>
    /// <param name="newProgress">a value denoting the progress that the bar should be set to</param>
    public void SetProgress(float newProgress)
    {
        slider.value = newProgress;
        target = slider.value;
    }
}
