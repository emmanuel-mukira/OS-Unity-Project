using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PetersonSimulation : MonoBehaviour
{
    public Button processStartButton;
    public Button processEndButton;
    public GameObject[] processes; // Array of "process" objects representing the balls

    private bool isRunning = false; // To keep track if the simulation is running

    void Start()
    {
        // Ensure the buttons have listeners to start and stop the simulation
        processStartButton.onClick.AddListener(StartSimulation);
        processEndButton.onClick.AddListener(StopSimulation);
    }

    // Start the simulation coroutine
    private void StartSimulation()
    {
        if (!isRunning)
        {
            isRunning = true;
            StartCoroutine(ProcessCoroutine());
        }
    }

    // Stop the simulation coroutine
    private void StopSimulation()
    {
        isRunning = false;
        StopCoroutine(ProcessCoroutine());
        ResetProcesses();
    }

    // Coroutine to simulate Peterson's algorithm
    private IEnumerator ProcessCoroutine()
    {
        int processCount = processes.Length;

        while (isRunning)
        {
            for (int i = 0; i < processCount; i++)
            {
                if (!isRunning) yield break; // Exit if simulation stopped

                // Activate the current process (make it green and move it up)
                SetProcessActive(i, true);

                // Wait for 3 seconds
                yield return new WaitForSeconds(3);

                // Deactivate the current process (reset to white and move back down)
                SetProcessActive(i, false);
            }
        }
    }

    // Method to activate or deactivate a process
    private void SetProcessActive(int index, bool isActive)
    {
        GameObject process = processes[index];
        Renderer renderer = process.GetComponent<Renderer>();

        if (isActive)
        {
            renderer.material.color = Color.green; // Change color to green
            process.transform.position += Vector3.up * 0.5f; // Move up
        }
        else
        {
            renderer.material.color = Color.white; // Change color back to white
            process.transform.position -= Vector3.up * 0.5f; // Move down
        }
    }

    // Method to reset all processes to their original state
    private void ResetProcesses()
    {
        foreach (GameObject process in processes)
        {
            Renderer renderer = process.GetComponent<Renderer>();
            renderer.material.color = Color.white; // Reset color to white

            // Reset position to original (if initial position was changed)
            process.transform.position = new Vector3(
                process.transform.position.x,
                0, // Assume 0 is the original Y position
                process.transform.position.z
            );
        }
    }
}
