using UnityEngine;
using System;

public class MotionGainButton : MonoBehaviour
{
    [Header("References")]
    public Transform buttonVisual;      
    public Transform handTransform;     

    [Header("Press Settings")]
    public float maxVisualPressDistance = 0.02f; 
    [Range(0.1f, 5f)]
    public float motionGain = 1.0f;     
    [Range(0f, 1f)]
    public float goalPressFraction = 0.8f; 

    [Header("Debug / Output")]
    [Range(0f, 1f)]
    public float normalizedPress;       
    public bool isAtGoal;

    public bool trialActive = false;
    private float trialStartTime;
    private float goalReachedTime;

    private float handStartLocalY;
    private Vector3 buttonStartLocalPos;

    public event Action OnGoalReached;  
    public void BeginTrial(Transform hand)
    {
        handTransform = hand;
        trialActive = true;
        isAtGoal = false;

        trialStartTime = Time.time;
        goalReachedTime = 0f;

        buttonStartLocalPos = buttonVisual.localPosition;

        handStartLocalY = transform.InverseTransformPoint(handTransform.position).y;
    }

    public void EndTrial()
    {
        trialActive = false;
        handTransform = null;

        buttonVisual.localPosition = buttonStartLocalPos;
        normalizedPress = 0f;
        isAtGoal = false;
    }

    void Update()
    {
        if (!trialActive || handTransform == null)
            return;

        float currentHandLocalY = transform.InverseTransformPoint(handTransform.position).y;
        float physicalDelta = handStartLocalY - currentHandLocalY;  

        if (physicalDelta < 0f) physicalDelta = 0f;

        float visualDelta = physicalDelta * motionGain;

        float clampedVisual = Mathf.Clamp(visualDelta, 0f, maxVisualPressDistance);

        buttonVisual.localPosition = buttonStartLocalPos + new Vector3(0f, -clampedVisual, 0f);

        normalizedPress = clampedVisual / maxVisualPressDistance;

        if (!isAtGoal && normalizedPress >= goalPressFraction)
        {
            isAtGoal = true;
            goalReachedTime = Time.time;
            float timeToGoal = goalReachedTime - trialStartTime;

            Debug.Log($"[Button] Goal reached. Gain={motionGain}, TimeToGoal={timeToGoal:F3}s");

            OnGoalReached?.Invoke();
        }
    }
}
