using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class LanguageDropDown : MonoBehaviour
{
    public TMP_Dropdown m_Dropdown;

    void Start()
    {
        // Add listener for when the value of the Dropdown changes
        m_Dropdown.onValueChanged.AddListener(delegate {
                DropdownValueChanged(m_Dropdown);
            });
    }

    // Use the dropdown value to set the language of the app
    void DropdownValueChanged(TMP_Dropdown change)
    {
        print("New Value : " + change.value);
        StartCoroutine(SetLocale(change.value));
    }

    IEnumerator SetLocale(int localeID)
    {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
    }
}
