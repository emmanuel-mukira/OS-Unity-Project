using System.Collections.Generic;
using UnityEngine;
using TMPro; // Ensure you have this if you're using TextMeshPro

public class QueueManager : MonoBehaviour
{
    public Transform waitingArea;
    public Transform treatmentArea;
    public GameObject patientPrefab;

    // UI Text elements
    public TextMeshProUGUI currentPatientWaitingTimeText;
    public TextMeshProUGUI currentPatientTurnaroundTimeText;
    public TextMeshProUGUI averageWaitingTimeText;
    public TextMeshProUGUI averageTurnaroundTimeText;

    private Queue<Patient> patientQueue = new Queue<Patient>();
    private float totalWaitingTime = 0f;
    private float totalTurnaroundTime = 0f;
    private int patientsServed = 0;

    void Start()
    {
        // Initialize the queue or add initial patients if needed
    }

    public void AddPatientToQueue(float arrivalTime, float serviceTime)
    {
        GameObject patientObj = Instantiate(patientPrefab, waitingArea.position, Quaternion.identity);
        Patient patient = patientObj.GetComponent<Patient>();
        patient.arrivalTime = arrivalTime;
        patient.serviceTime = serviceTime;
        patientQueue.Enqueue(patient);
    }

    void Update()
    {
        ProcessFCFS(); // Call this method to manage the queue
    }

    private void ProcessFCFS()
    {
        if (patientQueue.Count > 0)
        {
            Patient currentPatient = patientQueue.Peek();
            if (!currentPatient.isServed)
            {
                // Move patient to treatment area
                currentPatient.transform.position = treatmentArea.position;
                currentPatient.isServed = true;

                // Calculate waiting and turnaround times
                currentPatient.waitingTime = Time.time - currentPatient.arrivalTime;
                currentPatient.turnaroundTime = currentPatient.waitingTime + currentPatient.serviceTime;

                // Update total times and served count
                totalWaitingTime += currentPatient.waitingTime;
                totalTurnaroundTime += currentPatient.turnaroundTime;
                patientsServed++;

                // Update UI
                UpdateUI(currentPatient.waitingTime, currentPatient.turnaroundTime);
                
                // Dequeue the patient
                patientQueue.Dequeue(); // Serve next patient
            }
        }
    }

    private void UpdateUI(float waitingTime, float turnaroundTime)
    {
        currentPatientWaitingTimeText.text = "Current Patient Waiting Time: " + waitingTime.ToString("F2") + "s";
        currentPatientTurnaroundTimeText.text = "Current Patient Turnaround Time: " + turnaroundTime.ToString("F2") + "s";

        // Calculate averages
        if (patientsServed > 0)
        {
            float averageWaitingTime = totalWaitingTime / patientsServed;
            float averageTurnaroundTime = totalTurnaroundTime / patientsServed;

            averageWaitingTimeText.text = "Average Waiting Time: " + averageWaitingTime.ToString("F2") + "s";
            averageTurnaroundTimeText.text = "Average Turnaround Time: " + averageTurnaroundTime.ToString("F2") + "s";
        }
    }
}
