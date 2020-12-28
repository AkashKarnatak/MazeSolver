using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panelBackground = default;
    [SerializeField] private GameObject pauseMenu = default;
    [SerializeField] private GameObject helpMenu = default;
    [SerializeField] private GameObject optionsMenu = default;
    [SerializeField] Slider mouseSlider = default;
    [SerializeField] Slider movementSlider = default;
    private MoveSpectator moveSpectator = default;
    private bool isGamePaused = false;

    void Start() {
        moveSpectator = FindObjectOfType<MoveSpectator>();
        mouseSlider.value = (moveSpectator.mouseSensitivity - 5f)/(200f-5f);
        movementSlider.value = (moveSpectator.movementSpeed - 0.2f)/(10f-0.2f);
        panelBackground.SetActive(false);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        helpMenu.SetActive(false);
    }

    public void Pause() {
        Cursor.lockState = CursorLockMode.None;
        isGamePaused = true;
        Time.timeScale = 0f;
        panelBackground.SetActive(true);
        optionsMenu.SetActive(false);
        helpMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void Resume() {
        Cursor.lockState = CursorLockMode.Locked;
        isGamePaused = false;
        Time.timeScale = 1f;
        panelBackground.SetActive(false);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        helpMenu.SetActive(false);
    } 

    public void ShowInstructions() {
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        helpMenu.SetActive(true);
    }

    public void Options() {
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(true);
        helpMenu.SetActive(false);
    }

    public void MainMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    } 

    public void AccessMouseSlider(float value) {
        // value's range -> [0,1]
        moveSpectator.mouseSensitivity = 5f + (200f - 5f) * value; // range -> [5, 200]
    }

    public void AccessMovementSlider(float value) {
        // value's range -> [0,1]
        moveSpectator.movementSpeed = 0.2f + (10f - 0.2f) * value; // range -> [0.2, 10]
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) {
            if(isGamePaused) {
                Resume();
            } else {
                Pause();
            }
        }
    }

}
