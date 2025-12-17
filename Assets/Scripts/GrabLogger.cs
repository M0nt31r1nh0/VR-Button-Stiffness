using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class XRGrabLogger : MonoBehaviour
{
    private XRGrabInteractable grab;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        if (grab == null)
        {
            grab = GetComponent<XRGrabInteractable>();
        }
        grab.selectEntered.AddListener(OnSelectEntered);
        grab.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnSelectEntered);
        grab.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (VRLogger.Instance != null)
        {
            // Log grab event using this object's transform
            VRLogger.Instance.LogEvent("Grab_" + gameObject.name, transform);
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (VRLogger.Instance != null)
        {
            // Log release event
            VRLogger.Instance.LogEvent("Release_" + gameObject.name, transform);
        }
    }
}
