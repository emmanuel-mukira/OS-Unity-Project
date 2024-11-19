using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QueueManager : MonoBehaviour
{
    public Transform doctorPosition; // The doctor's position
    public Button fcfsButton; // Button for FCFS scheduling
    public Button srtfButton; // Button for SRTF scheduling
    public Button resetButton; // Button to reset the simulation
    public GameObject[] patients; // Array of patient GameObjects

    // TextMeshPro elements for displaying results
    public TextMeshProUGUI waitingTimeText;
    public TextMeshProUGUI averageWaitingTimeText;
    public TextMeshProUGUI turnaroundTimeText;
    public TextMeshProUGUI averageTurnaroundTimeText;

    public float moveSpeed = 2f; // Speed at which patients move
    private List<Patient> patientQueue = new List<Patient>();
    private Patient currentPatient = null;
    private List<Patient> treatedPatients = new List<Patient>();

    private float totalWaitingTime = 0f;
    private float totalTurnaroundTime = 0f;

    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>(); // Store original positions

    private void Start()
    {
        // Initialize patient queue and store their original positions
        foreach (var patientObj in patients)
        {
            Patient patient = patientObj.GetComponent<Patient>();
            patientQueue.Add(patient);
            originalPositions[patientObj] = patientObj.transform.position; // Save original positions

            // Ensure all patients start idle
            Animator animator = patientObj.GetComponent<Animator>();
            animator.SetBool("isWalking", false);
        }

        // Assign button click listeners
        fcfsButton.onClick.AddListener(() => StartSimulation("FCFS"));
        srtfButton.onClick.AddListener(() => StartSimulation("SRTF"));
        resetButton.onClick.AddListener(ResetSimulation);
    }

    public void StartSimulation(string algorithm)
    {
        if (currentPatient != null) return; // Only process one patient at a time

        if (algorithm == "FCFS")
        {
            if (patientQueue.Count > 0)
            {
                currentPatient = patientQueue[0];
                patientQueue.RemoveAt(0);
                StartCoroutine(ProcessPatient(currentPatient));
            }
        }
        else if (algorithm == "SRTF")
        {
            if (patientQueue.Count > 0)
            {
                // Sort patients by remaining burst time (Shortest Remaining Time First)
                patientQueue.Sort((a, b) => a.remainingServiceTime.CompareTo(b.remainingServiceTime));
                currentPatient = patientQueue[0];
                patientQueue.RemoveAt(0);
                StartCoroutine(ProcessPatient(currentPatient));
            }
        }
    }

    private IEnumerator ProcessPatient(Patient patient)
    {
        Animator animator = patient.GetComponent<Animator>();
        animator.SetBool("isWalking", true); // Trigger walk animation

        // Move the patient towards the doctor position
        Vector3 targetPosition = new Vector3(doctorPosition.position.x, doctorPosition.position.y, patient.transform.position.z);
        while (Vector3.Distance(patient.transform.position, targetPosition) > 0.1f)
        {
            patient.transform.position = Vector3.MoveTowards(patient.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("isWalking", false); // Stop walking animation

        // Simulate service time delay (doctor treating the patient)
        float startTime = Time.time;
        yield return new WaitForSeconds(patient.serviceTime);

        float completionTime = Time.time;

        // Calculate waiting and turnaround times
        patient.waitingTime = completionTime - patient.arrivalTime - patient.serviceTime;
        patient.turnaroundTime = completionTime - patient.arrivalTime;

        // Update statistics
        treatedPatients.Add(patient);
        totalWaitingTime += patient.waitingTime;
        totalTurnaroundTime += patient.turnaroundTime;
        UpdateUI();

        // Mark the current patient as treated
        currentPatient = null;
    }

    private void UpdateUI()
    {
        if (treatedPatients.Count > 0)
        {
            averageWaitingTimeText.text = $"Avg Waiting Time: {totalWaitingTime / treatedPatients.Count:F2}s";
            averageTurnaroundTimeText.text = $"Avg Turnaround Time: {totalTurnaroundTime / treatedPatients.Count:F2}s";
        }

        if (currentPatient != null)
        {
            waitingTimeText.text = $"Waiting Time: {currentPatient.waitingTime:F2}s";
            turnaroundTimeText.text = $"Turnaround Time: {currentPatient.turnaroundTime:F2}s";
        }
        else
        {
            waitingTimeText.text = "Waiting Time: -";
            turnaroundTimeText.text = "Turnaround Time: -";
        }
    }

    public void ResetSimulation()
    {
        // Reset patient positions and clear treated patients
        foreach (var patientObj in patients)
        {
            patientObj.transform.position = originalPositions[patientObj];
            Patient patient = patientObj.GetComponent<Patient>();
            patientQueue.Add(patient); // Add back to queue
            patient.remainingServiceTime = patient.serviceTime; // Reset remaining service time
            patient.waitingTime = 0f;
            patient.turnaroundTime = 0f;

            Animator animator = patientObj.GetComponent<Animator>();
            animator.SetBool("isWalking", false); // Ensure idle state
        }

        treatedPatients.Clear();
        patientQueue.Sort((a, b) => a.arrivalTime.CompareTo(b.arrivalTime)); // Restore FCFS order

        // Reset statistics
        totalWaitingTime = 0f;
        totalTurnaroundTime = 0f;

        // Reset UI
        waitingTimeText.text = "Waiting Time: -";
        turnaroundTimeText.text = "Turnaround Time: -";
        averageWaitingTimeText.text = "Avg Waiting Time: -";
        averageTurnaroundTimeText.text = "Avg Turnaround Time: -";

        currentPatient = null;
    }
}
