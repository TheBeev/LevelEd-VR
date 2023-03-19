using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.Highlighters;

public class SCR_TestCreateCube : MonoBehaviour {

    public VRTK_DestinationMarker pointer;
    public Color hoverColor = Color.cyan;
    public Color selectColor = Color.yellow;
    public bool logEnterEvent = true;
    public bool logHoverEvent = false;
    public bool logExitEvent = true;
    public bool logSetEvent = true;

    [SerializeField] private GameObject spawnObject;

    protected virtual void OnEnable()
    {
        pointer = (pointer == null ? GetComponent<VRTK_DestinationMarker>() : pointer);

        if (pointer != null)
        {
            pointer.DestinationMarkerEnter += DestinationMarkerEnter;
            pointer.DestinationMarkerHover += DestinationMarkerHover;
            pointer.DestinationMarkerExit += DestinationMarkerExit;
            pointer.DestinationMarkerSet += DestinationMarkerSet;
        }
        else
        {
            VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_FROM_GAMEOBJECT, "VRTKExample_PointerObjectHighlighterActivator", "VRTK_DestinationMarker", "the Controller Alias"));
        }
    }

    protected virtual void OnDisable()
    {
        if (pointer != null)
        {
            pointer.DestinationMarkerEnter -= DestinationMarkerEnter;
            pointer.DestinationMarkerHover -= DestinationMarkerHover;
            pointer.DestinationMarkerExit -= DestinationMarkerExit;
            pointer.DestinationMarkerSet -= DestinationMarkerSet;
        }
    }

    protected virtual void DestinationMarkerEnter(object sender, DestinationMarkerEventArgs e)
    {
        ToggleHighlight(e.target, hoverColor, e, false);
        if (logEnterEvent)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "POINTER ENTER", e.target, e.raycastHit, e.distance, e.destinationPosition);
        }
    }

    private void DestinationMarkerHover(object sender, DestinationMarkerEventArgs e)
    {
        if (logHoverEvent)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "POINTER HOVER", e.target, e.raycastHit, e.distance, e.destinationPosition);
        }
    }

    protected virtual void DestinationMarkerExit(object sender, DestinationMarkerEventArgs e)
    {
        ToggleHighlight(e.target, Color.clear, e, false);
        if (logExitEvent)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "POINTER EXIT", e.target, e.raycastHit, e.distance, e.destinationPosition);
        }
    }

    protected virtual void DestinationMarkerSet(object sender, DestinationMarkerEventArgs e)
    {
        ToggleHighlight(e.target, selectColor, e, true);
        if (logSetEvent)
        {
            DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "POINTER SET", e.target, e.raycastHit, e.distance, e.destinationPosition);
        }
    }

    protected virtual void ToggleHighlight(Transform target, Color color, DestinationMarkerEventArgs e, bool triggered)
    {
        VRTK_BaseHighlighter highligher = (target != null ? target.GetComponentInChildren<VRTK_BaseHighlighter>() : null);
        if (highligher != null)
        {
            highligher.Initialise();
            if (color != Color.clear)
            {
                highligher.Highlight(color);
            }
            else
            {
                highligher.Unhighlight();
            }
        }
        else if(triggered)
        {
            Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);
            print(offset);
            Instantiate(spawnObject, e.destinationPosition + offset, spawnObject.transform.rotation);
        }
    }

    protected virtual void DebugLogger(uint index, string action, Transform target, RaycastHit raycastHit, float distance, Vector3 tipPosition)
    {
        string targetName = (target ? target.name : "<NO VALID TARGET>");
        string colliderName = (raycastHit.collider ? raycastHit.collider.name : "<NO VALID COLLIDER>");
        VRTK_Logger.Info("Controller on index '" + index + "' is " + action + " at a distance of " + distance + " on object named [" + targetName + "] on the collider named [" + colliderName + "] - the pointer tip position is/was: " + tipPosition);
    }
}

