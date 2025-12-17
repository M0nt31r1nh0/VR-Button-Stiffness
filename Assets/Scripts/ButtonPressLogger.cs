using UnityEngine;
using System.IO;

public class ButtonPressLogger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Trigger cube that defines the goal depth")]
    public Transform triggerTransform;

    [Tooltip("The transform that actually moves down when pressed (recommended)")]
    public Transform buttonVisual;

    [Header("Experiment Settings")]
    [Tooltip("Tag used by the controller object (capsule collider)")]
    public string controllerTag = "Hand";

    [Tooltip("Set this per button (0..8)")]
    public int buttonId = 0;

    private MotionGainButton motionGainButton;

    private bool pressStarted = false;
    private bool goalReached = false;
    private float pressStartTime = -1f;
    private float minButtonLocalY;

    // ---- Shared across ALL buttons (one file per run) ----
    private static string csvPath;
    private static bool csvInitialized = false;
    private static int globalTrialIndex = 0;
    private static readonly object fileLock = new object();

    // Ensures a clean start each time Play/Build runs (even if domain reload settings vary)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticsOnRun()
    {
        csvPath = null;
        csvInitialized = false;
        globalTrialIndex = 0;
    }

    private void Start()
    {
        motionGainButton = GetComponentInParent<MotionGainButton>(); // ok if present; otherwise gain logs as -1

        EnsureCsvInitialized();

        if (buttonVisual == null)
            Debug.LogWarning($"[Logger:{name}] buttonVisual not assigned. Falling back to this transform for Y logging.");

        if (triggerTransform == null)
            Debug.LogWarning($"[Logger:{name}] triggerTransform not assigned. Trigger detection will not work.");
    }

    private void EnsureCsvInitialized()
    {
        if (csvInitialized) return;

        lock (fileLock)
        {
            if (csvInitialized) return;

            // One file per RUN (milliseconds avoids collisions)
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            csvPath = Path.Combine(Application.persistentDataPath, $"buttonLog_{timestamp}.csv");

            File.WriteAllText(csvPath, "Trial,ButtonId,TimeToGoal,MinButtonLocalY,MotionGain,DateTime\n");

            csvInitialized = true;
            Debug.Log("[Logger] Saving ONE NEW CSV for this run to: " + csvPath);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!pressStarted && collision.collider.CompareTag(controllerTag))
        {
            pressStarted = true;
            goalReached = false;
            pressStartTime = Time.time;

            // initialize min with current value at press start
            minButtonLocalY = (buttonVisual != null) ? buttonVisual.localPosition.y : transform.localPosition.y;
        }
    }

    private void Update()
    {
        if (!pressStarted || goalReached) return;

        float y = (buttonVisual != null) ? buttonVisual.localPosition.y : transform.localPosition.y;
        if (y < minButtonLocalY) minButtonLocalY = y;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!pressStarted) return;
        if (goalReached) return;
        if (triggerTransform == null) return;

        if (other.transform == triggerTransform)
        {
            goalReached = true;

            float timeToGoal = Time.time - pressStartTime;
            SaveTrial(timeToGoal, minButtonLocalY);
            ResetTrialState();
        }
    }

    private void SaveTrial(float timeToGoal, float minY)
    {
        EnsureCsvInitialized();

        float gainValue = (motionGainButton != null) ? motionGainButton.motionGain : -1f;

        int trialNumber;
        lock (fileLock)
        {
            globalTrialIndex++;
            trialNumber = globalTrialIndex;

            string line = $"{trialNumber},{buttonId},{timeToGoal:F4},{minY:F4},{gainValue},{System.DateTime.Now}\n";
            File.AppendAllText(csvPath, line);
        }
    }

    private void ResetTrialState()
    {
        pressStarted = false;
        goalReached = false;
        pressStartTime = -1f;
    }
}
