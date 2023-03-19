using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SCR_Move : MonoBehaviour, ITool {

    private enum ToolStates { Selecting, Moving };
    private ToolStates currentState = ToolStates.Selecting;

    [SerializeField] private VRTK_ControllerEvents.ButtonAlias activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
    [SerializeField] private VRTK_ControllerEvents controllerEvents;
    [SerializeField] private string toolName;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private VRTK_ControllerReference controllerReference;
    private bool bActivationButtonPressed;
    private IPointer variablePointer;
    private GameObject selectedObject;
    private Vector3 objectOffset;
    private Vector3 testOffset;
    private Vector3 startLocation;
    private Vector3 pointerLocation;
    private Vector3 offsetLocation;
    private Color objectStartColor;
    private SCR_GroupParent groupParentScript;
    

    private SCR_ToolOptions toolOptions;
    private bool bSnap;

    private void OnEnable()
    {
        SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

        if (toolOptions == null)
        {
            toolOptions = FindObjectOfType<SCR_ToolOptions>();
        }

        SCR_ToolOptions.instance.DeactivateOptions();
        SCR_GridSnappingOption.instance.ActivateOption();
        SCR_SurfaceSnappingOption.instance.ActivateOption();

        if (variablePointer != null)
        {
            variablePointer.HighlightingActive = true;
        }
        else
        {
            Start();
            variablePointer.HighlightingActive = true;
        }
        
        controllerEvents.SubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
    }

    private void OnDisable()
    {
        bActivationButtonPressed = false;
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
        variablePointer.HighlightingActive = false;
    }

    void DoActivationButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor)
        {
            if (controllerReference == null)
            {
                controllerReference = e.controllerReference;
            }

            if (!bActivationButtonPressed && !SCR_ToolMenuRadial.instance.Busy)
            {
                bActivationButtonPressed = true;
            }
        }
    }

    // Use this for initialization
    void Start ()
    {
        GameObject variableObject = GameObject.FindGameObjectWithTag("RightVariable");

        if (variableObject)
        {
            variablePointer = (IPointer)variableObject.GetComponent(typeof(IPointer));
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        switch (currentState)
        {
            case ToolStates.Selecting:
                SelectingObject();
                break;
            case ToolStates.Moving:
                MovingObject();
                break;
            default:
                break;
        }
    }

    void SelectingObject()
    {
        if (variablePointer != null)
        {
            if (variablePointer.Active && variablePointer.ValidRaycastTarget)
            {
                if (bActivationButtonPressed)
                {
                    bBusy = true;

                    variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

                    //is it grouped or not
                    if (variablePointer.PointerHit.transform.parent != null)
                    {
                        if (variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_GroupParent>() != null)
                        {
                            selectedObject = variablePointer.PointerHit.transform.root.gameObject;
                            objectOffset = variablePointer.PointerPosition - variablePointer.PointerHit.point;
                            startLocation = selectedObject.transform.position;
                            testOffset = startLocation - variablePointer.PointerPosition;
                            variablePointer.RemoveHighlight();
                            groupParentScript = variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_GroupParent>();
                            groupParentScript.CheckMaterialCache();
                            groupParentScript.CurrentlySelected();
                        }
                        else if(variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_PrefabData>() != null)
                        {
                            selectedObject = variablePointer.PointerHit.transform.root.gameObject;
                            objectOffset = variablePointer.PointerPosition - variablePointer.PointerHit.point;
                            startLocation = selectedObject.transform.position;
                            testOffset = startLocation - variablePointer.PointerPosition;

                            selectedObject.GetComponent<SCR_PrefabData>().CurrentlySelected();
                        }
                    }
                    else
                    {
                        if (variablePointer.PointerHit.transform.GetComponent<SCR_PrefabData>() != null)
                        {
                            selectedObject = variablePointer.PointerHit.transform.gameObject;
                            objectOffset = variablePointer.PointerPosition - variablePointer.PointerHit.point;
                            startLocation = selectedObject.transform.position;
                            testOffset = startLocation - variablePointer.PointerPosition;

                            selectedObject.GetComponent<SCR_PrefabData>().CurrentlySelected();
                        }
                        else if(variablePointer.PointerHit.transform.GetComponent<SCR_ObjectData>() != null)
                        {
                            selectedObject = variablePointer.PointerHit.transform.gameObject;
                            objectOffset = variablePointer.PointerPosition - selectedObject.transform.position;
                            startLocation = selectedObject.transform.position;
                            testOffset = startLocation - variablePointer.PointerPosition;
                            selectedObject.layer = 2;
                            objectStartColor = selectedObject.GetComponent<Renderer>().material.color;
                            selectedObject.GetComponent<Renderer>().material.color = Color.red;
                        }
                    }

                    if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
                    {
                        variablePointer.LockPointerLength(true);
                    }

                    VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);
                    bActivationButtonPressed = false;
                    currentState = ToolStates.Moving;
                }
            }
            else
            {
                bActivationButtonPressed = false;
            }
        }
    }

    void MovingObject()
    {

        if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
        {
            pointerLocation = variablePointer.PointerPosition;

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                pointerLocation = pointerLocation + testOffset;
                offsetLocation = pointerLocation;
                selectedObject.transform.position = Snap(offsetLocation);
                
            }
            else
            {
                selectedObject.transform.position = Snap(variablePointer.PointerPosition);
            }
        }
        else
        {
            pointerLocation = variablePointer.PointerPosition;

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                pointerLocation = pointerLocation + testOffset;
                offsetLocation = pointerLocation;
                selectedObject.transform.position = offsetLocation;
            }
            else
            {
                selectedObject.transform.position = variablePointer.PointerPosition + objectOffset;
            }
            
        }

        if (bActivationButtonPressed)
        {
            bActivationButtonPressed = false;
            if (selectedObject.GetComponent<SCR_GroupParent>() != null)
            {
                groupParentScript.Deselected();
            }
            else if(selectedObject.GetComponent<SCR_PrefabData>() != null)
            {
                selectedObject.GetComponent<SCR_PrefabData>().Deselected();
            }
            else
            {
                selectedObject.GetComponent<Renderer>().material.color = objectStartColor;
                selectedObject.layer = 8;
            }

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                variablePointer.LockPointerLength(false);
            }

            variablePointer.SetPointerColourDefault();

            VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
            bBusy = false;
            currentState = ToolStates.Selecting;
        }

    }

    Vector3 Snap(Vector3 snapNearPoint)
    {
        return SCR_GridSnappingOption.instance.GetNearestPointOnGrid(snapNearPoint);
    }
}
