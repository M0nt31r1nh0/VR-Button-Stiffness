using UnityEngine;
using System.IO;

public class ButtonPressLogger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Trigger cube that defines the goal depth")]
    public Transform triggerTransform;        

    [Header("Experiment Settings")]
    [Tooltip("Tag used by the controller object (capsule collider)")]
    public string controllerTag = "Hand";

    private MotionGainButton motionGainButton;

    private bool pressStarted = false;
    private bool goalReached = false;
    private float pressStartTime = -1f;
    private float goalTime = -1f;

    private int trialIndex = 0;
    private string csvPath;

    private void Start()
    {
        motionGainButton = GetComponentInParent<MotionGainButton>();
        if (motionGainButton == null)
        {
            Debug.LogWarning("[Logger] No MotionGainButton found in parents. MotionGain will be logged as -1.");
        }

        csvPath = Path.Combine(Application.persistentDataPath, "buttonLog.csv");

        if (!File.Exists(csvPath))
        {
            File.WriteAllText(csvPath,
                "Trial,TimeToGoal,ButtonLocalY,MotionGain,DateTime\n");
        }

        Debug.Log("[Logger] Saving data to: " + csvPath);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!pressStarted && collision.collider.CompareTag(controllerTag))
        {
            pressStarted = true;
            goalReached = false;

            pressStartTime = Time.time;
            goalTime = -1f;

            Debug.Log("[Button] Press STARTED (controller touched PushButton)");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!pressStarted) return;           
        if (goalReached) return;             

        if (other.transform == triggerTransform)
        {
            goalReached = true;
            goalTime = Time.time;

            float timeToGoal = goalTime - pressStartTime;
            float buttonLocalY = transform.localPosition.y;

            Debug.Log($"[Button] Reached Trigger! TimeToGoal = {timeToGoal:F3}s, localY = {buttonLocalY:F4}");

            SaveTrial(timeToGoal, buttonLocalY);
            ResetTrialState();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag(controllerTag))
        {
            Debug.Log("[Button] Controller left PushButton surface");
        }
    }

    private void SaveTrial(float timeToGoal, float buttonLocalY)
    {
        trialIndex++;

        float gainValue = (motionGainButton != null) ? motionGainButton.motionGain : -1f;

        string line = $"{trialIndex},{timeToGoal:F4},{buttonLocalY:F4},{gainValue},{System.DateTime.Now}\n";
        File.AppendAllText(csvPath, line);

        Debug.Log("[Logger] Saved trial " + trialIndex + $" (Gain={gainValue})");
    }

    private void ResetTrialState()
    {
        pressStarted = false;
        goalReached = false;
        pressStartTime = -1f;
        goalTime = -1f;
    }
}
