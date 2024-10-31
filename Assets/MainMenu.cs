using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadCPUScheduling()
    {
        SceneManager.LoadScene("CPUSchedulingScene");
    }

    public void LoadProcessSynchronization()
    {
        SceneManager.LoadScene("ProcessSynchronizationScene");
    }

    public void LoadMemoryManagement()
    {
        SceneManager.LoadScene("MemoryManagementScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
