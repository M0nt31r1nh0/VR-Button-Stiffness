using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;

[RequireComponent(typeof(ButtonFollowVisual))]
public class XRButtonLogger : MonoBehaviour
{
    [Header("Experiment Settings")]
    [Tooltip("Set this per button (0..8)")]
    public int buttonId = 0;

    private ButtonFollowVisual targetButtonScript;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    private bool pressStarted = false;
    private bool goalReached = false;
    private float pressStartTime = -1f;
    private float maxDepthReached = 0f;
    private Vector3 initialLocalPos;

    private static string csvPath;
    private static bool csvInitialized = false;
    private static int globalTrialIndex = 0;
    private static readonly object fileLock = new object();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticsOnRun()
    {
        csvPath = null;
        csvInitialized = false;
        globalTrialIndex = 0;
    }

    private void Start()
    {
        targetButtonScript = GetComponent<ButtonFollowVisual>();
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

        if (targetButtonScript == null || interactable == null)
        {
            Debug.LogError($"[XRButtonLogger] Missing required components on {name}");
            enabled = false;
            return;
        }

        if (targetButtonScript.visualTarget != null)
            initialLocalPos = targetButtonScript.visualTarget.localPosition;

        interactable.hoverEntered.AddListener(OnHoverEnter);
        
        interactable.selectEntered.AddListener(OnSelectEnter);
        
        interactable.hoverExited.AddListener(OnHoverExit);

        EnsureCsvInitialized();
    }

    private void OnHoverEnter(BaseInteractionEventArgs args)
    {
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor)
        {
            if (!pressStarted)
            {
                pressStarted = true;
                goalReached = false;
                pressStartTime = Time.time;
                maxDepthReached = 0f;
                
                if (targetButtonScript.visualTarget != null)
                    initialLocalPos = targetButtonScript.visualTarget.localPosition;
            }
        }
    }

    private void OnSelectEnter(BaseInteractionEventArgs args)
    {
        if (pressStarted && !goalReached)
        {
            if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor)
            {
                goalReached = true;
                float timeToGoal = Time.time - pressStartTime;
                
                SaveTrial(timeToGoal, maxDepthReached);
            }
        }
    }

    private void OnHoverExit(BaseInteractionEventArgs args)
    {
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor)
        {
            pressStarted = false;
            goalReached = false;
        }
    }

    private void Update()
    {
        if (!pressStarted || targetButtonScript.visualTarget == null) return;

        Vector3 currentPos = targetButtonScript.visualTarget.localPosition;
        Vector3 diff = currentPos - initialLocalPos;
        float currentDepth = Vector3.Project(diff, targetButtonScript.localAxis).magnitude;

        if (currentDepth > maxDepthReached) maxDepthReached = currentDepth;
    }

    private void EnsureCsvInitialized()
    {
        if (csvInitialized) return;

        lock (fileLock)
        {
            if (csvInitialized) return;
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            csvPath = Path.Combine(Application.persistentDataPath, $"buttonLog_XR_{timestamp}.csv");
            File.WriteAllText(csvPath, "Trial,ButtonId,TimeToGoal,MaxDepthReached,PushResistance,DateTime\n");
            csvInitialized = true;
            Debug.Log("[XRLogger] Log file created: " + csvPath);
        }
    }

    private void SaveTrial(float timeToGoal, float depth)
    {
        EnsureCsvInitialized();
        float resistance = targetButtonScript.pushResistance;

        int trialNumber;
        lock (fileLock)
        {
            globalTrialIndex++;
            trialNumber = globalTrialIndex;
            string line = $"{trialNumber},{buttonId},{timeToGoal:F4},{depth:F4},{resistance},{System.DateTime.Now}\n";
            File.AppendAllText(csvPath, line);
        }
        Debug.Log($"[XRLogger] Button {buttonId} LOGGED! Time: {timeToGoal}s");
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEnter);
            interactable.selectEntered.RemoveListener(OnSelectEnter);
            interactable.hoverExited.RemoveListener(OnHoverExit);
        }
    }
}