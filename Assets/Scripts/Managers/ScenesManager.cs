// ScenesManagers.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ScenesManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {

    }
    public void CloseApp()
    {
        Application.Quit();
        Debug.Log("Application QUIT");
    }
    public void PauseGame()
    {
        Time.timeScale = 0;
        Debug.Log("Application PAUSE 0");
    }
    public void UnPauseGame()
    {
        Time.timeScale = 1.0f;
        Debug.Log("Application PAUSE 1");
    }
    public void PlayGame()
    {
        Time.timeScale = 1.0f;
        Debug.Log("Application PLAY");
    }
    public void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1.0f;
    }
    public void LoadLevel1()
    {
        SceneManager.LoadScene("LV1");
        Time.timeScale = 1;
    }
    public void LoadLevel2()
    {
        SceneManager.LoadScene("LV2");
        Time.timeScale = 1;
    }
    public void LoadLevel3()
    {
        SceneManager.LoadScene("LV3");
        Time.timeScale = 1;
    }
}