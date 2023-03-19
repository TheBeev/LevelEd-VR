using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SCR_Grab : MonoBehaviour, ITool
{
    private enum ToolStates { Selecting, Grabbing };
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
    private GameObject variableObject;
    private GameObject selectedObject;
    private Color objectStartColor;
    private SCR_GroupParent groupParentScript;

    private SCR_ToolOptions toolOptions;
    private bool bSnap;

    private void OnEnable()
    {
        SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

        SCR_ToolOptions.instance.DeactivateOptions();
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
        variablePointer.HighlightingActive = false;
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
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
    void Start()
    {
        variableObject = GameObject.FindGameObjectWithTag("RightVariable");

        if (variableObject)
        {
            variablePointer = (IPointer)variableObject.GetComponent(typeof(IPointer));
        }

    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case ToolStates.Selecting:
                SelectingObject();
                break;
            case ToolStates.Grabbing:
                MovingObject();
                break;
            default:
                break;
        }
    }

    void SelectingObject()
    {
        if (variablePointer.Active && variablePointer.ValidRaycastTarget)
        {
            if (bActivationButtonPressed)
            {
                bBusy = true;

                variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

                if (variablePointer.PointerHit.transform.parent)
                {
                    if (variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_GroupParent>() != null)
                    {
                        selectedObject = variablePointer.PointerHit.transform.root.gameObject;;
                        objectStartColor = variablePointer.ObjectOriginalColour;

                        groupParentScript = variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_GroupParent>();
                        groupParentScript.CheckMaterialCache();
                        groupParentScript.CurrentlySelected();
                    }
                    else if(variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_PrefabData>() != null)
                    {
                        selectedObject = variablePointer.PointerHit.transform.root.gameObject;
                        selectedObject.GetComponent<SCR_PrefabData>().CurrentlySelected();
                    }
                }
                else
                {
                    if (variablePointer.PointerHit.transform.GetComponent<SCR_PrefabData>() != null)
                    {
                        selectedObject = variablePointer.PointerHit.transform.gameObject;
                        selectedObject.GetComponent<SCR_PrefabData>().CurrentlySelected();
                    }
                    else if(variablePointer.PointerHit.transform.GetComponent<SCR_ObjectData>() != null)
                    {
                        selectedObject = variablePointer.PointerHit.transform.gameObject;
                        selectedObject.layer = 2;
                        objectStartColor = variablePointer.ObjectOriginalColour;
                        selectedObject.GetComponent<Renderer>().material.color = Color.red;
                    }
                }

                if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
                {
                    variablePointer.LockPointerLength(true);
                }

                selectedObject.transform.parent = variablePointer.PointerEndGameObject.transform;

                VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);
                bActivationButtonPressed = false;
                currentState = ToolStates.Grabbing;
            }
        }
        else
        {
            bActivationButtonPressed = false;
        }
    }

    void MovingObject()
    {

        if (bActivationButtonPressed)
        {
            bActivationButtonPressed = false;

            if (selectedObject.GetComponent<SCR_GroupParent>() != null)
            {
                selectedObject.GetComponent<SCR_GroupParent>().Deselected();
            }
            else if(selectedObject.GetComponent<SCR_PrefabData>() != null)
            {
                selectedObject.GetComponent<SCR_PrefabData>().Deselected();
            }
            else
            {
                selectedObject.layer = 8;
                selectedObject.GetComponent<Renderer>().material.color = objectStartColor;
            }

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                variablePointer.LockPointerLength(false);
            }

            selectedObject.transform.parent = null;

            VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
            bBusy = false;

            variablePointer.SetPointerColourDefault();

            currentState = ToolStates.Selecting;
        }

    }
}
