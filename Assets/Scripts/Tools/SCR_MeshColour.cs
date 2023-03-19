using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SCR_MeshColour : MonoBehaviour, ITool {

    private enum ToolStates { Colouring,  };
    private ToolStates currentState = ToolStates.Colouring;

    [SerializeField] private VRTK_ControllerEvents.ButtonAlias activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
    [SerializeField] private VRTK_ControllerEvents controllerEvents;
    [SerializeField] private string toolName;

    private Color colourToUse;
    public Color ColourToUse
    {
        get { return colourToUse; }
        set { colourToUse = value; }
    }

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private VRTK_ControllerReference controllerReference;
    private bool bActivationButtonPressed;
    private IPointer variablePointer;
    private GameObject objectToColour;

    private SCR_GroupParent groupParentScript;
    private SCR_ToolOptions toolOptions;

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

    void DoActivationButtonReleased(object sender, ControllerInteractionEventArgs e)
    {

    }

    // Use this for initialization
    void Start()
    {
        GameObject variableObject = GameObject.FindGameObjectWithTag("RightVariable");

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
            case ToolStates.Colouring:
                ColouringObject();
                break;
            default:
                break;
        }
    }

    void ColouringObject()
    {
        if (variablePointer.Active && variablePointer.ValidRaycastTarget)
        {
            if (bActivationButtonPressed)
            {
                bBusy = true;

                variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);


                if (variablePointer.PointerHit.transform.root.GetComponent<SCR_GroupParent>() != null)
                {
                    groupParentScript = variablePointer.PointerHit.transform.root.GetComponent<SCR_GroupParent>();
                    variablePointer.RemoveHighlight();
                    groupParentScript.CheckMaterialCache();
                    objectToColour = variablePointer.PointerHit.transform.gameObject;
                    variablePointer.ObjectOriginalColour = colourToUse;
                    objectToColour.GetComponent<MeshRenderer>().material.color = colourToUse;
                    groupParentScript.UpdateCachedMaterials();

                }
                else if(variablePointer.PointerHit.transform.GetComponent<SCR_ObjectData>() != null)
                {

                    objectToColour = variablePointer.PointerHit.transform.gameObject;
                    variablePointer.ObjectOriginalColour = colourToUse;
                    objectToColour.GetComponent<Renderer>().material.color = colourToUse;

                }

                VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
                bActivationButtonPressed = false;
                bBusy = false;

                variablePointer.SetPointerColourDefault();

                currentState = ToolStates.Colouring;
            }
        }
        else
        {
            bActivationButtonPressed = false;
        }
    }
}
