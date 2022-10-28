using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour 
{
    public static MenuManager Instance;

    [SerializeField] Menu[] menus;

    /// <summary>
    /// Executed when an instance of the class is initialized.
    /// </summary>
    private void Awake() {
        Instance = this;
    }

    /// <summary>
    /// Method for opening a certain menu.
    /// </summary>
    /// <param name="menuName">String denoting the name of the menu to be opened.</param>
    public void OpenMenu(string menuName) 
    {
        for (int i = 0; i < menus.Length; ++i) {
            if (menus[i].menuName == menuName) menus[i].Open();
            else if (menus[i].open) CloseMenu(menus[i]);
        }
    }

    /// <summary>
    /// Method for opening a certain menu.
    /// </summary>
    /// <param name="menu">The menu object to be opened.</param>
    public void OpenMenu(Menu menu) 
    {
        for (int i = 0; i < menus.Length; ++i) {
            if (menus[i].open) CloseMenu(menus[i]);
        }
        menu.Open();
    }

    /// <summary>
    /// Method for closing a certain menu.
    /// </summary>
    /// <param name="menu">The menu object to be closed.</param>
    public void CloseMenu(Menu menu) 
    {
        menu.Close();
    }

    /// <summary>
    /// Method for opening the multi-player FillInBlanks scene.
    /// </summary>
    public void OpenFIBScene() 
    {
        SceneManager.LoadScene("FillInBlanks");
    }

    /// <summary>
    /// Method for opening the multi-player MeaningMatching scene.
    /// </summary>
    public void OpenMMScene() 
    {
        SceneManager.LoadScene("MeaningMatching");
    }
}