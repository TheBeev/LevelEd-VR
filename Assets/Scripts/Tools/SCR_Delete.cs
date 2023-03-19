using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SCR_Delete : MonoBehaviour, ITool {

    private enum ToolStates { Deleting,  };
    private ToolStates currentState = ToolStates.Deleting;

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
    private GameObject objectToDelete;

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
            case ToolStates.Deleting:
                DeletingObject();
                break;
            default:
                break;
        }
    }

    void DeletingObject()
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
                        StartCoroutine(DeleteGroupDelayed(variablePointer.PointerHit.transform.root.gameObject));
                    }
                    else if (variablePointer.PointerHit.transform.root.gameObject.GetComponent<IScriptable>() != null)
                    {
                        objectToDelete = variablePointer.PointerHit.transform.root.gameObject;
                        objectToDelete.GetComponent<SCR_PrefabData>().CurrentlySelected();
                        SCR_SaveSystem.instance.RemoveScript(objectToDelete);
                        Destroy(objectToDelete, 0.1f);
                    }
                    else if (variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_PrefabData>() != null)
                    {
                        if (variablePointer.PointerHit.transform.parent != null)
                        {
                            objectToDelete = variablePointer.PointerHit.transform.root.gameObject;
                            objectToDelete.GetComponent<SCR_PrefabData>().CurrentlySelected();
                            SCR_SaveSystem.instance.RemovePrefab(objectToDelete);
                            Destroy(objectToDelete, 0.1f);
                        }
                    }
                }
                else
                {
                    if (variablePointer.PointerHit.transform.GetComponent<IScriptable>() != null)
                    {
                        objectToDelete = variablePointer.PointerHit.transform.gameObject;
                        objectToDelete.GetComponent<SCR_PrefabData>().CurrentlySelected();
                        SCR_SaveSystem.instance.RemoveScript(objectToDelete);
                        Destroy(objectToDelete, 0.1f);
                    }
                    else if (variablePointer.PointerHit.transform.GetComponent<SCR_PrefabData>() != null)
                    {
                        objectToDelete = variablePointer.PointerHit.transform.gameObject;
                        objectToDelete.GetComponent<SCR_PrefabData>().CurrentlySelected();
                        SCR_SaveSystem.instance.RemovePrefab(objectToDelete);
                        Destroy(objectToDelete, 0.1f);
                    }
                    else if(variablePointer.PointerHit.transform.GetComponent<SCR_ObjectData>() != null)
                    {
                        objectToDelete = variablePointer.PointerHit.transform.gameObject;
                        objectToDelete.GetComponent<Renderer>().material.color = Color.red;
                        SCR_SaveSystem.instance.RemoveGeometry(objectToDelete);
                        Destroy(objectToDelete, 0.1f);
                    }
                }

                VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
                bActivationButtonPressed = false;
                bBusy = false;

                variablePointer.SetPointerColourDefault();

                currentState = ToolStates.Deleting;
            }
        }
        else
        {
            bActivationButtonPressed = false;
        }
    }

    IEnumerator DeleteGroupDelayed(GameObject parentObjectToDelete)
    {
        groupParentScript = parentObjectToDelete.GetComponent<SCR_GroupParent>();

        foreach (var item in groupParentScript.groupedObjectList)
        {
            item.GetComponent<Renderer>().material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var item in groupParentScript.groupedObjectList)
        {
            SCR_SaveSystem.instance.RemoveGeometry(item);
            Destroy(item.gameObject);
        }

        SCR_SaveSystem.instance.RemoveParent(parentObjectToDelete);
        Destroy(parentObjectToDelete);

    }
}
