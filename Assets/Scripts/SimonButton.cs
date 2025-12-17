using UnityEngine;

public class SimonButton : MonoBehaviour
{
    [Header("Assigned by Manager")]
    [HideInInspector] public int buttonId;

    [Header("Press Detection")]
    public Transform triggerTransform;
    public string controllerTag = "Hand";

    [Header("Visual Feedback")]
    public Renderer visualRenderer;
    [Range(0f, 1f)] public float offMultiplier = 0.3f;

    private SimonGameManager9 manager;

    private bool pressStarted = false;
    private bool goalReached = false;

    private Color baseColor;

    public void Init(SimonGameManager9 gm, int id)
    {
        manager = gm;
        buttonId = id;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (!pressStarted && collision.collider.CompareTag(controllerTag))
        {
            pressStarted = true;
            goalReached = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!pressStarted) return;
        if (goalReached) return;
        if (triggerTransform == null) return;

        if (other.transform == triggerTransform)
        {
            goalReached = true;

            manager?.OnPhysicalButtonPressed(buttonId);

            // Reset for next press
            pressStarted = false;
            goalReached = false;
        }
    }
}
