using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadCPUScheduling()
    {
        SceneManager.LoadScene("CPUScheduling");
    }

    public void LoadProcessSynchronization()
    {
        SceneManager.LoadScene("ProcessSynchronization");
    }

    public void LoadMemoryManagement()
    {
        SceneManager.LoadScene("MemoryManagement");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
