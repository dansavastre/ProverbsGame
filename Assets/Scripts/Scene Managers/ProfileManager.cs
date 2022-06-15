using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProfileManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI email;

    private string[] scenes = SessionManager.scenes;

    private static AudioSource WoodButton;

    // Start is called before the first frame update
    void Start()
    {
        username.text = AccountManager.playerName;
        email.text = AccountManager.playerEmail;

        WoodButton = AccountManager.WoodButton;
    }

    public void PlonkNoise()
    {
        WoodButton.Play();
    }

    public void OnClickLogout()
    {
        // TODO: A proper logout, the name and email are still saved
        SwitchScene(0);
    }

    public void SwitchScene(int sceneIndex)
    {
        SceneManager.LoadScene(scenes[sceneIndex]);
    }
}
