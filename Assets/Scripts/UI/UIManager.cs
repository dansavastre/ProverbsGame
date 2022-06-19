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

    void Awake()
    {
        // Makes sure the sound effect is not destroyed when switching scenes
        if (WoodButton != null) DontDestroyOnLoad(WoodButton);

        // Initializes the proverb level up variables
        if (ProverbLevelUp != null) 
        {
            Debug.Log("Enabling proverb level up.");
            ProverbLevelUp.SetActive(true);
            ConfettiPS = GameObject.Find("Confetti").GetComponent<ParticleSystem>();
            Debug.Log("Confetti found.");
            ProficiencyText = GameObject.Find("Proficiency").GetComponent<TextMeshProUGUI>();
            Debug.Log("Proficiency found.");
            disableCongratulations();
            Debug.Log("Disabling proverb level up.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && (ProverbLevelUp != null)) disableCongratulations();
    }

    // Shows the proverb level up pop up
    public void enableCongratulations(string proficiencyText)
    {
        Debug.Log("PARTYYYYYY");
        ProverbLevelUp.SetActive(true);
        ConfettiPS.Play();
        ProficiencyText.text = proficiencyText + "!";
    }

    // Hides the proverb level up pop up
    public void disableCongratulations()
    {
        ConfettiPS.Stop();
        ProverbLevelUp.SetActive(false);
    }
}