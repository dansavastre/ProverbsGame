using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
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
    public static string[] modes =
    {
        "FunFact",              // 0 Fun fact page
        "MultipleChoice",       // 1 Multiple choice
        "RecognizeImage",       // 2 Recognize image
        "FillBlanks",           // 3 Fill in the blanks
        "FormSentence"          // 4 Form a sentence
    };
    
    private ParticleSystem ConfettiPS;
    private TextMeshProUGUI ProficiencyText;

    [SerializeField] private AudioSource WoodButton;
    [SerializeField] private GameObject ProverbLevelUp;

    /// <summary>
    /// Gets the name of the next scene depending on the stage.
    /// </summary>
    /// <param name="stage">The number of the stage that the proverb is currently in.</param>
    /// <returns>A string denoting the name of the scene that must be loaded next.</returns>
    public static int NextSceneName(int stage)
    {
        if (stage == 1 || stage == 3 || stage == 5) return 1;
        else if (stage == 2) return 2;
        else if (stage == 4) return 3;
        else if (stage == 6) return 4;
        return -1;
    }

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
    /// <param name="proficiencyText">String denoting the proficiency that the player leveled the proverb to.</param>
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
    /// Switches to another scene.
    /// </summary>
    /// <param name="sceneIndex">The index of the scene to switch to.</param>
    public static void SwitchScene(int sceneIndex)
    {
        SceneManager.LoadScene(scenes[sceneIndex]);
    }

    /// <summary>
    /// Switches to another scene in the singleplayer modes.
    /// </summary>
    /// <param name="modeIndex">The index of the mode to switch to.</param>
    public static void SwitchMode(int modeIndex)
    {
        SceneManager.LoadScene(modes[modeIndex]);
    }
}