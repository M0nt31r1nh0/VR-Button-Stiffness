using UnityEngine;
using System;

public class MotionGainButton : MonoBehaviour
{
    public enum HandSide { Left, Right }

    [Serializable]
    public class ButtonEntry
    {
        public string name;
        public Transform buttonVisual;

        [HideInInspector] public Vector3 startLocalPos;
        [HideInInspector] public float normalizedPress;
        [HideInInspector] public bool isAtGoal;
    }

    [Header("Buttons (size = 9)")]
    public ButtonEntry[] buttons = new ButtonEntry[9];

    [Header("Hands")]
    public Transform leftHand;
    public Transform rightHand;

    [Header("Press Settings")]
    public float maxVisualPressDistance = 0.02f;
    [Range(0f, 1f)] public float goalPressFraction = 0.8f;
    [Range(0.1f, 5f)] public float motionGain = 1.0f;

    [Header("Trial")]
    public bool trialActive;

    private float trialStartTime;
    private float leftHandStartY;
    private float rightHandStartY;

    // buttonIndex, handUsed, timeToGoal
    public event Action<int, HandSide, float> OnButtonGoalReached;

    public void BeginTrial()
    {
        trialActive = true;
        trialStartTime = Time.time;

        if (leftHand != null)
            leftHandStartY = transform.InverseTransformPoint(leftHand.position).y;

        if (rightHand != null)
            rightHandStartY = transform.InverseTransformPoint(rightHand.position).y;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i]?.buttonVisual == null) continue;

            buttons[i].startLocalPos = buttons[i].buttonVisual.localPosition;
            buttons[i].normalizedPress = 0f;
            buttons[i].isAtGoal = false;
        }
    }

    public void EndTrial()
    {
        trialActive = false;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i]?.buttonVisual == null) continue;

            buttons[i].buttonVisual.localPosition = buttons[i].startLocalPos;
            buttons[i].normalizedPress = 0f;
            buttons[i].isAtGoal = false;
        }
    }

    void Update()
    {
        if (!trialActive) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            var b = buttons[i];
            if (b?.buttonVisual == null) continue;

            float leftDelta = 0f;
            float rightDelta = 0f;

            if (leftHand != null)
            {
                float y = transform.InverseTransformPoint(leftHand.position).y;
                leftDelta = Mathf.Max(0f, leftHandStartY - y);
            }

            if (rightHand != null)
            {
                float y = transform.InverseTransformPoint(rightHand.position).y;
                rightDelta = Mathf.Max(0f, rightHandStartY - y);
            }

            HandSide activeHand = (rightDelta > leftDelta) ? HandSide.Right : HandSide.Left;
            float pressDelta = Mathf.Max(leftDelta, rightDelta);

            float visualPress = Mathf.Clamp(pressDelta, 0f, maxVisualPressDistance);

            b.buttonVisual.localPosition =
                b.startLocalPos + Vector3.down * visualPress;

            b.normalizedPress = visualPress / maxVisualPressDistance;

            if (!b.isAtGoal && b.normalizedPress >= goalPressFraction)
            {
                b.isAtGoal = true;

                float timeToGoal = Time.time - trialStartTime;

                Debug.Log(
                    $"[Button] {i} ({b.name}) reached goal | Hand={activeHand} | Time={timeToGoal:F3}s"
                );

                OnButtonGoalReached?.Invoke(i, activeHand, timeToGoal);
            }
        }
    }
}
