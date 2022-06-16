using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] public AudioSource WoodButton;
    [SerializeField] private GameObject ProverbLevelUp;
    private ParticleSystem ConfettiPS;
    private TextMeshProUGUI ProficiencyText;

    // Start is called before the first frame update
    void Awake()
    {
        if (WoodButton != null) DontDestroyOnLoad(WoodButton);
        if (ProverbLevelUp != null) 
        {
            ProverbLevelUp.SetActive(true);
            ConfettiPS = GameObject.Find("Confetti").GetComponent<ParticleSystem>();
            ProficiencyText = GameObject.Find("Proficiency").GetComponent<TextMeshProUGUI>();
            disableCongratulations();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && (ProverbLevelUp != null)) disableCongratulations();
    }

    public void onMouseClick() 
    {
        WoodButton.Play();
    }

    public void enableCongratulations(string proficiencyText)
    {
        ProverbLevelUp.SetActive(true);
        ConfettiPS.Play();
        ProficiencyText.text = proficiencyText + "!";
    }

    public void disableCongratulations()
    {
        ConfettiPS.Stop();
        ProverbLevelUp.SetActive(false);
    }
}
