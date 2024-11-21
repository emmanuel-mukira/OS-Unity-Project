using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QueueManager : MonoBehaviour
{
    public Transform doctorPosition;
    public Button fcfsButton;
    public Button srtfButton;
    public Button resetButton;
    public GameObject[] patients;

    public TextMeshProUGUI waitingTimeText;
    public TextMeshProUGUI averageWaitingTimeText;
    public TextMeshProUGUI turnaroundTimeText;
    public TextMeshProUGUI averageTurnaroundTimeText;
    public TextMeshProUGUI patientDoneText;

    public float moveSpeed = 2f;
    private List<Patient> patientQueue = new List<Patient>();
    private Patient currentPatient = null;
    private List<Patient> treatedPatients = new List<Patient>();

    private float totalWaitingTime = 0f;
    private float totalTurnaroundTime = 0f;

    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();

    private void Start()
    {
        // Initialize patient queue and store original positions
        foreach (var patientObj in patients)
        {
            Patient patient = patientObj.GetComponent<Patient>();
            patientQueue.Add(patient);
            originalPositions[patientObj] = patientObj.transform.position;

            Animator animator = patientObj.GetComponent<Animator>();
            animator.SetBool("isWalking", false); // Start idle
        }

        // Assign button click listeners
        fcfsButton.onClick.AddListener(() => StartSimulation("FCFS"));
        srtfButton.onClick.AddListener(() => StartSimulation("SRTF"));
        resetButton.onClick.AddListener(ResetSimulation);

        // Ensure "Patient is done!" text is hidden initially
        patientDoneText.text = "";
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
                patientQueue.Sort((a, b) => a.remainingServiceTime.CompareTo(b.remainingServiceTime)); // SRTF
                currentPatient = patientQueue[0];
                patientQueue.RemoveAt(0);
                StartCoroutine(ProcessPatient(currentPatient));
            }
        }
    }

    private IEnumerator ProcessPatient(Patient patient)
    {
        Animator animator = patient.GetComponent<Animator>();
        animator.SetBool("isWalking", true); // Start walking animation

        Vector3 targetPosition = new Vector3(doctorPosition.position.x, doctorPosition.position.y, patient.transform.position.z);
        while (Vector3.Distance(patient.transform.position, targetPosition) > 0.1f)
        {
            patient.transform.position = Vector3.MoveTowards(patient.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("isWalking", false); // Stop walking animation

        // Simulate service time
        float startTime = Time.time;
        yield return new WaitForSeconds(patient.serviceTime);

        float completionTime = Time.time;

        // Update waiting and turnaround times
        patient.waitingTime = completionTime - patient.arrivalTime - patient.serviceTime;
        patient.turnaroundTime = completionTime - patient.arrivalTime;

        treatedPatients.Add(patient);
        totalWaitingTime += patient.waitingTime;
        totalTurnaroundTime += patient.turnaroundTime;
        UpdateUI();

        // Show "Patient is done!" text
        patientDoneText.text = "Patient is done!";
        yield return new WaitForSeconds(2f);

        patientDoneText.text = "";
        patient.gameObject.SetActive(false);

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
        // Reset patient positions, timings, and animations
        foreach (var patientObj in patients)
        {
            Patient patient = patientObj.GetComponent<Patient>();
            patientObj.transform.position = originalPositions[patientObj]; // Reset position
            patient.remainingServiceTime = patient.serviceTime; // Reset service time
            patient.waitingTime = 0f;
            patient.turnaroundTime = 0f;

            Animator animator = patientObj.GetComponent<Animator>();
            animator.SetBool("isWalking", false); // Reset animation

            patient.gameObject.SetActive(true); // Reactivate patient
        }

        // Clear queues and treated patients
        patientQueue.Clear();
        treatedPatients.Clear();

        // Refill patientQueue in FCFS order (by Arrival Time)
        foreach (var patientObj in patients)
        {
            Patient patient = patientObj.GetComponent<Patient>();
            patientQueue.Add(patient);
        }
        patientQueue.Sort((a, b) => a.arrivalTime.CompareTo(b.arrivalTime));

        // Reset stats
        totalWaitingTime = 0f;
        totalTurnaroundTime = 0f;

        // Reset UI
        waitingTimeText.text = "Waiting Time: -";
        turnaroundTimeText.text = "Turnaround Time: -";
        averageWaitingTimeText.text = "Avg Waiting Time: -";
        averageTurnaroundTimeText.text = "Avg Turnaround Time: -";

        patientDoneText.text = "";
        currentPatient = null;
    }
}
