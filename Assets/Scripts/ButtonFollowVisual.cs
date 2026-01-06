using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ButtonFollowVisual : MonoBehaviour
{
    [Header("Visual Configuration")]
    public Transform visualTarget;
    public Vector3 localAxis;
    public float resetSpeed = 5;
    public float followAngleThreshold = 45;

    [Header("Physics Feel")]
    [Tooltip("Value of 1 is 1:1 movement. Higher values make the button feel 'heavier' by requiring more hand movement.")]
    public float pushResistance = 1.0f;

    private bool freeze = false;
    private Vector3 initialLocalPos;
    private Vector3 offset;
    private Transform pokeAttachTransform;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
    private bool isFollowing = false;

    void Start()
    {
        initialLocalPos = visualTarget.localPosition;

        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        interactable.hoverEntered.AddListener(Follow);
        interactable.hoverExited.AddListener(Reset);
        interactable.selectEntered.AddListener(Freeze);
    }

    public void Follow(BaseInteractionEventArgs hover)
    {
        if (hover.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor interactor = (UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor)hover.interactorObject;

            isFollowing = true;
            pokeAttachTransform = interactor.attachTransform;
            
            offset = visualTarget.position - pokeAttachTransform.position;

            float pokeAngle = Vector3.Angle(offset, visualTarget.TransformDirection(localAxis));

            if (pokeAngle > followAngleThreshold)
            {
                isFollowing = false;
                freeze = true;
            }
        }
    }

    public void Reset(BaseInteractionEventArgs hover)
    {
        if (hover.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor)
        {
            isFollowing = false;
            freeze = false;
        }
    }

    public void Freeze(BaseInteractionEventArgs hover)
    {
        if (hover.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor)
        {
            freeze = true;
        }
    }

    void Update()
    {
        if (freeze)
            return;

        if (isFollowing)
        {
            Vector3 targetWorldPos = pokeAttachTransform.position + offset;
            Vector3 targetLocalPos = visualTarget.parent != null 
                ? visualTarget.parent.InverseTransformPoint(targetWorldPos) 
                : targetWorldPos;
            Vector3 moveDelta = targetLocalPos - initialLocalPos;
            Vector3 projectedDelta = Vector3.Project(moveDelta, localAxis);

            Vector3 weightedDelta = projectedDelta / Mathf.Max(1f, pushResistance);

            visualTarget.localPosition = initialLocalPos + weightedDelta;
        }
        else
        {
            visualTarget.localPosition = Vector3.Lerp(visualTarget.localPosition, initialLocalPos, Time.deltaTime * resetSpeed);
        }
    }
}