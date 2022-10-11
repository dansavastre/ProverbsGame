using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // UI elements
    [SerializeField] private AudioSource WoodButton;
    [SerializeField] private GameObject ProverbLevelUp;

    // Proverb level up variables
    private ParticleSystem ConfettiPS;
    private TextMeshProUGUI ProficiencyText;

    public static string[] scenes =
    {
        "FirstScreen",          // 0 First screen on app launch
        "Register",             // 1 Screen to register
        "Login",                // 2 Screen to login
        "MainMenu",             // 3 Singleplayer menu
        "FillInBlanks",         // 4 Multiplayer menu
        "InfoScreen",           // 5 Information page
        "ProfilePage",          // 6 Profile page
        "Dictionary"            // 7 Proverb dictionary
    };

    /// <summary>
    /// Executed when an instance of this class is initialized.
    /// </summary>
    void Awake()
    {
        // Initializes the proverb level up variables
        if (ProverbLevelUp != null) 
        {
            ProverbLevelUp.SetActive(true);
            ConfettiPS = GameObject.Find("Confetti").GetComponent<ParticleSystem>();
            ProficiencyText = GameObject.Find("Proficiency").GetComponent<TextMeshProUGUI>();
            disableCongratulations();
        }

        // Find the GameObject that contains the audio source for button sound
        WoodButton = GameObject.Find("WoodButtonAudio").GetComponent<AudioSource>();
    }

    /// <summary>
    /// Executes on each frame update.
    /// </summary>
    void Update()
    {
        // If clicked on the screen and the proverb level up is displayed, disable it
        if (Input.GetMouseButtonDown(0) && (ProverbLevelUp != null)) disableCongratulations();
    }

    /// <summary>
    /// Shows the proverb level up pop up.
    /// </summary>
    /// <param name="proficiencyText">string denoting the proficiency that the player leveled the proverb to</param>
    public void enableCongratulations(string proficiencyText)
    {
        ProverbLevelUp.SetActive(true);
        ConfettiPS.Play();
        ProficiencyText.text = proficiencyText + "!";
    }

    /// <summary>
    /// Hides the proverb level up pop up.
    /// </summary>
    public void disableCongratulations()
    {
        ConfettiPS.Stop();
        ProverbLevelUp.SetActive(false);
    }

    /// <summary>
    /// Plays the button clicked sound once.
    /// </summary>
    public void PlonkNoise()
    {
        WoodButton.Play();
    }

    /// <summary>
    /// Switches to another scene
    /// </summary>
    /// <param name="sceneIndex"></param>
    public static void SwitchScene(int sceneIndex)
    {
        SceneManager.LoadScene(scenes[sceneIndex]);
    }
}