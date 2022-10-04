using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // UI elements
    [SerializeField] public AudioSource WoodButton;
    [SerializeField] private GameObject ProverbLevelUp;

    // Proverb level up variables
    private ParticleSystem ConfettiPS;
    private TextMeshProUGUI ProficiencyText;

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
}