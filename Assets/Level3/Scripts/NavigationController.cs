using UnityEngine;
using UnityEngine.SceneManagement;

// Buttons that change scenes (Lab 8, Exercise #24). Attached to the Canvas.
public class NavigationController : MonoBehaviour
{
    // Play Again: load this level from the start
    public void onClickRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Continue: load the next scene in Build Settings
    public void onClickNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void onClickQuit()
    {
        Application.Quit();
    }
}
