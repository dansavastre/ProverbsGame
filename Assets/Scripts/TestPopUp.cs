using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestPopUp : MonoBehaviour
{
    // UI elements
    [SerializeField] private TextMeshProUGUI congratulationPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnTestClick()
    {
        Instantiate(congratulationPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    }
}
