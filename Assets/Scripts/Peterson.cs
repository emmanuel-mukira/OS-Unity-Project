using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PetersonSimulation : MonoBehaviour
{
    public Button processStartButton;
    public Button processEndButton;
    public GameObject[] processes; // Array of "process" objects representing the balls
    public TextMeshProUGUI statusText; // Reference to the UI Text element for status display

    private bool isRunning = false; // To keep track if the simulation is running

    private bool[] flags; // Boolean flags for each process to signal intent
      private int turn; // Indicates whose turn it is to enter the critical section

      public AudioClip criticalSectionSound;
public AudioClip waitingSound;


    void Start()
    {
        // Initialize flags and turn
    flags = new bool[processes.Length];
    turn = 0;

        // Ensure the buttons have listeners to start and stop the simulation
        processStartButton.onClick.AddListener(StartSimulation);
        processEndButton.onClick.AddListener(StopSimulation);

        // Initialize status text
        UpdateStatusText("Press 'Start' to begin the simulation.");
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
        UpdateStatusText("Simulation stopped.");
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

            // Set flag to true, indicating process i wants to enter the critical section
            flags[i] = true;
            turn = (i + 1) % processCount; // Set turn to the next process

            // Show process is waiting
            UpdateStatusText($"Process {i + 1} is waiting.");
            SetProcessWaiting(i); // Display as waiting (yellow)

            // Play waiting sound
            processes[i].GetComponent<AudioSource>().PlayOneShot(waitingSound);

            // Wait until it’s this process's turn or the other process is not interested
            while (flags[(i + 1) % processCount] && turn == (i + 1) % processCount)
            {
                yield return null; // Yield until the condition is met
            }

            // Enter critical section
            UpdateStatusText($"Process {i + 1} is in the critical section.");
            SetProcessActive(i, true); // Activate process (green for critical section)

            // Play critical section sound
            processes[i].GetComponent<AudioSource>().PlayOneShot(criticalSectionSound);
            yield return new WaitForSeconds(3); // Simulate time in the critical section

            // Exit critical section
            SetProcessActive(i, false); // Deactivate process (reset to white)
            flags[i] = false; // Reset flag indicating process i is done
            UpdateStatusText($"Process {i + 1} has exited the critical section.");

            yield return new WaitForSeconds(1); // Small wait before the next process attempts
        }
    }
}




    // Method to activate or deactivate a process
   private void SetProcessActive(int index, bool inCriticalSection)
{
    GameObject process = processes[index];
    Renderer renderer = process.GetComponent<Renderer>();

    if (inCriticalSection)
    {
        renderer.material.color = Color.green; // Critical section color
        process.transform.position += Vector3.up * 0.5f; // Move up
    }
    else
    {
        renderer.material.color = Color.yellow; // Reset to idle color
        process.transform.position -= Vector3.up * 0.5f; // Move down
    }
}

private void SetProcessWaiting(int index)
{
    GameObject process = processes[index];
    Renderer renderer = process.GetComponent<Renderer>();
    renderer.material.color = Color.yellow; // Waiting color
}


    // Method to reset all processes to their original state
    private void ResetProcesses()
    {
        foreach (GameObject process in processes)
        {
            Renderer renderer = process.GetComponent<Renderer>();
            renderer.material.color = Color.white; // Reset color to white

            // Reset position to original (assuming 0 is the original Y position)
            process.transform.position = new Vector3(
                process.transform.position.x,
                0, 
                process.transform.position.z
            );
        }
    }

    // Method to update the status text
    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
