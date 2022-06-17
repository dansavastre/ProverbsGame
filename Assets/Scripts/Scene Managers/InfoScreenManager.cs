using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InfoScreenManager : MonoBehaviour {
    private static AudioSource WoodButton;

    public static string[] scenes;

    private void Start() {
        WoodButton = AccountManager.WoodButton;
        scenes = SessionManager.scenes;
    }

    public void PlonkNoise() {
        WoodButton.Play();
    }

    // Switch to the scene corresponding to the sceneIndex
    public void SwitchScene(int sceneIndex) {
        SceneManager.LoadScene(scenes[sceneIndex]);
    }
}
