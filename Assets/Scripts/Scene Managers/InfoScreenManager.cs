using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InfoScreenManager : MonoBehaviour {
    private static AudioSource WoodButton;

    public static string[] scenes = {
        "FirstScreen",          // First screen on app launch
        "Register",             // Screen to register
        "Login",                // Screen to login
        "SelectionMenu",        // Select singleplayer or multiplayer
        "SingleplayerMenu",     // Singleplayer menu
        "TitleMenu",            // Multiplayer menu
        "InfoScreen",           // Information page
        "ProfilePage",          // Profile page
    };

    private void Start() {
        WoodButton = AccountManager.WoodButton;
    }

    public void PlonkNoise() {
        WoodButton.Play();
    }

    // Switch to the scene corresponding to the sceneIndex
    public void SwitchScene(int sceneIndex) {
        SceneManager.LoadScene(scenes[sceneIndex]);
    }
}
