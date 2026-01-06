using UnityEngine;

public class SimonButton : MonoBehaviour
{
    [Header("Assigned by Manager")]
    [HideInInspector] public int buttonId;

    [Header("Visual Feedback")]
    public Renderer visualRenderer;
    [Range(0f, 1f)] public float offMultiplier = 0.3f;

    [Header("Hardness Settings")]
    [SerializeField] private float resistanceSoft = 1.0f;
    [SerializeField] private float resistanceNormal = 2.0f;
    [SerializeField] private float resistanceHard = 3.0f;

    private SimonGameManager9 manager;

    private bool pressStarted = false;
    private bool goalReached = false;

    private Color baseColor;

    private ButtonFollowVisual visualScript;

    public void Init(SimonGameManager9 gm, int id)
    {
        manager = gm;
        buttonId = id;

        Transform child = transform.Find("Interactable"); 

        if (child != null)
        {
            visualScript = child.GetComponent<ButtonFollowVisual>();
        }
        else
        {
            Debug.LogError($"[SimonButton] Could not find child named 'Interactable' on button {buttonId}!");
        }

        CacheBaseColor();
        TurnOff();
    }

    private void CacheBaseColor()
    {
        if (visualRenderer == null) return;

        var mat = visualRenderer.material;
        if (mat.HasProperty("_BaseColor")) baseColor = mat.GetColor("_BaseColor");
        else if (mat.HasProperty("_Color")) baseColor = mat.GetColor("_Color");
        else baseColor = Color.white;
    }

    public void TurnOn()
    {
        if (visualRenderer == null) return;
        SetColor(baseColor);
    }

    public void TurnOff()
    {
        if (visualRenderer == null) return;
        SetColor(baseColor * offMultiplier);
    }

    private void SetColor(Color c)
    {
        var mat = visualRenderer.material;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
        else if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
    }

    public void SetHardness(SimonGameManager9.HardnessLevel level)
    {
        if (visualScript == null)
        {
            Debug.LogError($"[SimonButton] No ButtonFollowVisual found on button {buttonId}");
            return;
        }

        float newResistance = resistanceNormal;

        switch (level)
        {
            case SimonGameManager9.HardnessLevel.Soft:
                newResistance = resistanceSoft;
                break;
            case SimonGameManager9.HardnessLevel.Normal:
                newResistance = resistanceNormal;
                break;
            case SimonGameManager9.HardnessLevel.Hard:
                newResistance = resistanceHard;
                break;
        }

        visualScript.pushResistance = newResistance;

        Debug.Log($"[SimonButton] Button {buttonId} set to {level} (Resistance: {newResistance})");
    }

    // public void OnCollisionEnter(Collision collision)
    // {
    //     if (!pressStarted && collision.collider.CompareTag(controllerTag))
    //     {
    //         pressStarted = true;
    //         goalReached = false;
    //     }
    // }

    public void OnTriggerEnter()
    {
        // if (!pressStarted) return;
        // if (goalReached) return;
        // if (triggerTransform == null) return;

        // if (other.transform == triggerTransform)
        // {
        goalReached = true;

        manager.OnPhysicalButtonPressed(buttonId);

        // Reset for next press
        pressStarted = false;
        goalReached = false;
        // }
    }
}
