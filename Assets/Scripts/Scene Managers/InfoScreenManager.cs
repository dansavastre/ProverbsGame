using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO: Remove this manager, only uses shared methods

public class InfoScreenManager : MonoBehaviour 
{
    // Audio source for button sound
    private static AudioSource WoodButton;

    /// <summary>
    /// Executed when the game is started.
    /// </summary>
    private void Start() 
    {
        WoodButton = AccountManager.WoodButton;
    }

    /// <summary>
    /// Plays the button clicked sound once.
    /// </summary>
    // TODO: Share method
    public void PlonkNoise()
    {
        WoodButton.Play();
    }

    /// <summary>
    /// Switches to another scene.
    /// </summary>
    /// <param name="sceneIndex">the index of the scene to be switched to</param>
    // TODO: Share method
    public void SwitchScene(int sceneIndex)
    {
        SceneManager.LoadScene(SessionManager.scenes[sceneIndex]);
    }
}