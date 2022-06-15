using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public AudioSource WoodButton;

    // Start is called before the first frame update
    void Awake()
    {
        // GameObject[] buttonAudio = GameObject.FindGameObjectsWithTag("WoodButtonAudio");
        DontDestroyOnLoad(WoodButton);
    }

    public void onMouseClick() 
    {
        WoodButton.Play();
    }
}
