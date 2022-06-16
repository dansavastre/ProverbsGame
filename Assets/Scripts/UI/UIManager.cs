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
        // GameObject[] buttonAudio = GameObject.FindGameObjectsWithTag("WoodButtonAudio");
        if (WoodButton != null) DontDestroyOnLoad(WoodButton);
        ConfettiPS = GameObject.Find("Confetti").GetComponent<ParticleSystem>();
        ProficiencyText = GameObject.Find("Proficiency").GetComponent<TextMeshProUGUI>();
        ProverbLevelUp.SetActive(false);
        StartCoroutine(enableCongratulations());
    }

    public void onMouseClick() 
    {
        WoodButton.Play();
    }

    IEnumerator enableCongratulations()
    {
        yield return new WaitForSeconds(2);
        ProverbLevelUp.SetActive(true);
        ConfettiPS.Play();
        yield return new WaitForSeconds(5);
        disableCongratulations();
    }

    public void disableCongratulations()
    {
        ProverbLevelUp.SetActive(false);
        ConfettiPS.Stop();
    }
}
