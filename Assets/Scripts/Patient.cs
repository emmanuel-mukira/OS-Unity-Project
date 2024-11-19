using UnityEngine;

public class Patient : MonoBehaviour
{
    public float arrivalTime; // Time when the patient arrives
    public float serviceTime; // Time required to be treated
    public float remainingServiceTime; // Time left for treatment (used in SRTF)
    public float waitingTime; // Time the patient has been waiting
    public float turnaroundTime; // Time from arrival to completion of treatment

     private void Start()
    {
        // Initialize remainingServiceTime with the initial serviceTime
        remainingServiceTime = serviceTime;
    }

    public Animator animator; // Reference to the Animator for animations
}
