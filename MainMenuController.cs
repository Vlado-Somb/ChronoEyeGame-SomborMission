using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("test"); // Replace with your game scene name
    }

    public void OpenSettings()
    {
        Debug.Log("Settings Menu Opened"); // Replace with actual logic later
    }

    public void OpenCredits()
    {
        Debug.Log("Credits Menu Opened"); // Replace with actual logic later
    }
}
