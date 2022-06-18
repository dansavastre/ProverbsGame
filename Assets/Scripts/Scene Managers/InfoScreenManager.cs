using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO: Remove this manager, only uses shared methods

public class InfoScreenManager : MonoBehaviour 
{
    // Audio source for button sound
    private static AudioSource WoodButton;

    private void Start() 
    {
        WoodButton = AccountManager.WoodButton;
    }

    // Plays the button clicked sound once
    // TODO: Share method
    public void PlonkNoise()
    {
        WoodButton.Play();
    }

    // Switches to another scene
    // TODO: Share method
    public void SwitchScene(int sceneIndex)
    {
        SceneManager.LoadScene(SessionManager.scenes[sceneIndex]);
    }
}