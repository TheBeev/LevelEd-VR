using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SCR_AntennaPointer : MonoBehaviour {

    [SerializeField] private LineRenderer antennaLineRender;
    [SerializeField] private GameObject pointerEnd;
    [SerializeField] private VRTK_ControllerEvents.ButtonAlias activationButton = VRTK_ControllerEvents.ButtonAlias.TouchpadPress;
    [SerializeField] private VRTK_ControllerEvents controllerEvents;

    private bool bToggleDistance = false;
    public bool ToggleDistance
    {
        get { return bToggleDistance; }
        set { bToggleDistance = value; }
    }

    public Vector3 PointerPosition
    {
        get
        {
            return pointerEnd.transform.position;
        }
    }

    public bool ValidTargetPosition
    {
        get
        {
            return true;
        }
    }

    private bool bActive;
    public bool Active
    {
        get { return bActive; }
        set { bActive = value; }
    }

    private void OnEnable()
    {
        controllerEvents.SubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
        bActive = true;
        //controllerEvents.SubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonReleased);
    }

    private void OnDisable()
    {
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
        //controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonReleased);
    }

    void DoActivationButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        antennaLineRender.enabled = !antennaLineRender.enabled;
        pointerEnd.SetActive(!pointerEnd.activeInHierarchy);
        bActive = !bActive;
    }

    void DoActivationButtonReleased(object sender, ControllerInteractionEventArgs e)
    {
        antennaLineRender.enabled = true;
        pointerEnd.SetActive(true);
        bActive = true;
    }

}
