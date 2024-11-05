using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueManager : MonoBehaviour
{
    public GameObject patientPrefab; // The prefab for the Patient
    public Transform waitingArea;    // The area where patients will queue
    public Transform treatmentArea;  // The area where patients are treated
    public float patientSpacing = 1.5f; // Spacing between patients in the queue

    private Queue<GameObject> patientQueue = new Queue<GameObject>(); // Queue to store patients

    void Start()
    {
        // Initial setup or spawning of patients can be done here
    }

    void Update()
    {
        // Update can be used to spawn patients periodically or based on user input
        // Example: Press 'P' to add a patient
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddPatientToQueue();
        }

        // If there's a patient in the queue and the treatment area is empty, start treatment
        if (patientQueue.Count > 0 && treatmentArea.childCount == 0)
        {
            StartTreatment();
        }
    }

    // Adds a new patient to the queue
    public void AddPatientToQueue()
    {
        GameObject newPatient = Instantiate(patientPrefab, waitingArea.position, Quaternion.identity);
        
        // Position the patient in the queue based on its position in the list
        Vector3 newPosition = waitingArea.position + Vector3.right * (patientQueue.Count * patientSpacing);
        newPatient.transform.position = newPosition;

        patientQueue.Enqueue(newPatient);
    }

    // Moves the first patient in the queue to the treatment area
    public void StartTreatment()
    {

        
        if (patientQueue.Count == 0) return;

        // Dequeue the first patient
        GameObject patientToTreat = patientQueue.Dequeue();

        // Move patient to treatment area
        patientToTreat.transform.position = treatmentArea.position;

        // Parent the patient under the treatment area for organization
        patientToTreat.transform.SetParent(treatmentArea);

        // Optional: Access Patient script to start treatment
        Patient patientScript = patientToTreat.GetComponent<Patient>();
        if (patientScript != null)
        {
            patientScript.StartTreatment();
        }

        // Re-position remaining patients in the queue
        RepositionQueue();
    }

    // Repositions the queue so patients shift forward when one is moved to treatment
    private void RepositionQueue()
    {
        int index = 0;
        foreach (GameObject patient in patientQueue)
        {
            Vector3 newPosition = waitingArea.position + Vector3.right * (index * patientSpacing);
            patient.transform.position = newPosition;
            index++;
        }
    }
}
