using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public void HideMenu() {
        this.gameObject.SetActive(false);
    }

    public void ShowMenu() {
        this.gameObject.SetActive(true);
    }

    public void ExitGame() {
        Application.Quit();
    }

    public void RestartGame() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level");
    }

    public void ShowAbout() {
        HideMenu();
    }
}
