using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestPopUp : MonoBehaviour
{
    // UI elements
    public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(prefab, transform.position, Quaternion.identity, GameObject.Find("Canvas").transform);
    }
}
