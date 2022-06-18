using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO: Combine this with another manager, mostly uses shared methods

public class ProfileManager : MonoBehaviour
{
    // UI elements
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI email;

    // Audio source for button sound
    private static AudioSource WoodButton;

    // Start is called before the first frame update
    void Start()
    {
        // Get the GameObject that contains the audio source for button sound
        WoodButton = AccountManager.WoodButton;

        // Instantiate the text fields with player info
        username.text = AccountManager.playerName;
        email.text = AccountManager.playerEmail;
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