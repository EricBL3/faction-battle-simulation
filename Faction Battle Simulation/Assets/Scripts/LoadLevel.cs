using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    private IEnumerator ChangeScene(string scene)
    {
        yield return new WaitForSecondsRealtime(0.5f);
        SceneManager.LoadScene(scene);
    }

    public void MainMenu()
    {
        StartCoroutine(ChangeScene("MainMenu"));
    }
    
    public void SetupGame()
    {
        StartCoroutine(ChangeScene("SimSetup"));
    }

    public void StartGame()
    {
        StartCoroutine(ChangeScene("SimBattle"));
    }
}
