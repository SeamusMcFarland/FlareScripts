using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonControllerScript : MonoBehaviour
{

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadMainLevel()
    {
        SceneManager.LoadScene("Main Level");
    }
}
