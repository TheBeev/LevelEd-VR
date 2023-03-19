using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SCR_Ungroup : MonoBehaviour, ITool {

    private enum ToolStates { Ungrouping, Idling };
    private ToolStates currentState = ToolStates.Ungrouping;

    [SerializeField] private VRTK_ControllerEvents.ButtonAlias activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
    [SerializeField] private VRTK_ControllerEvents controllerEvents;
    [SerializeField] private string toolName;
    [SerializeField] private Color defaultColour;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private VRTK_ControllerReference controllerReference;
    private bool bActivationButtonPressed;
    private IPointer variablePointer;

    private void OnEnable()
    {
        SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);
        SCR_ToolOptions.instance.DeactivateOptions();

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
        controllerEvents.SubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonDepressed);
    }

    private void OnDisable()
    {
        bActivationButtonPressed = false;
        variablePointer.HighlightingActive = false;
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonDepressed);
    }

    private void DoActivationButtonPressed(object sender, ControllerInteractionEventArgs e)
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

    private void DoActivationButtonDepressed(object sender, ControllerInteractionEventArgs e)
    {
        if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor)
        {
            if (bActivationButtonPressed)
            {
                bActivationButtonPressed = false;
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
            case ToolStates.Ungrouping:
                UngroupObjects();
                break;
            case ToolStates.Idling:
                //ToolIdling();
                break;
            default:
                break;
        }
    }

    private void UngroupObjects()
    {
        if (variablePointer.Active && variablePointer.ValidRaycastTarget /*&& variablePointer.PointerHit.transform.parent != null*/)
        {
            if (variablePointer.PointerHit.transform.parent)
            {
                if (variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_GroupParent>() != null)
                {
                    if (bActivationButtonPressed && !bBusy)
                    {
                        bBusy = true;

                        variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

                        GameObject oldGroupParentObject = variablePointer.PointerHit.transform.parent.gameObject;
                        SCR_GroupParent oldGroupParentScript = oldGroupParentObject.GetComponent<SCR_GroupParent>();

                        if (oldGroupParentScript)
                        {
                            foreach (var item in oldGroupParentScript.groupedObjectList)
                            {
                                item.GetComponent<SCR_ObjectData>().parentID = 0;
                                item.GetComponent<SCR_ObjectData>().parentName = null;
                                item.transform.parent = null;
                                item.GetComponent<Renderer>().material.color = Color.red;
                            }

                            StartCoroutine(DelayedUngroup(oldGroupParentObject));
                        }
                    }
                }
            }
        }
        else
        {
            bActivationButtonPressed = false;
        }
    }

    IEnumerator DelayedUngroup(GameObject oldParentObjectToDelete)
    {
        variablePointer.HighlightingActive = false;
        variablePointer.RemoveHighlight();

        SCR_GroupParent groupParentScript = oldParentObjectToDelete.GetComponent<SCR_GroupParent>();

        foreach (var item in groupParentScript.groupedObjectList)
        {
            item.GetComponent<Renderer>().material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        groupParentScript.Deselected();

        groupParentScript.groupedObjectList.Clear();

        SCR_SaveSystem.instance.RemoveParent(oldParentObjectToDelete);
        Destroy(oldParentObjectToDelete);

        VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
        bBusy = false;

        variablePointer.HighlightingActive = true;
        variablePointer.SetPointerColourDefault();

    }

}
