using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu = default;
    [SerializeField] private GameObject helpMenu = default;
    // Start is called before the first frame update
    void Start() {
        ShowMainMenu();
    }
    
    public void PlayGame () {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ShowMainMenu() {
        mainMenu.SetActive(true);
        helpMenu.SetActive(false);
    }
    public void ShowInstructions() {
        mainMenu.SetActive(false);
        helpMenu.SetActive(true);
    }

    public void QuitGame() {
        Application.Quit();
    }
}
