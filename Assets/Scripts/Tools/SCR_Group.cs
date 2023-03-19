using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SCR_Group : MonoBehaviour, ITool {

    private enum ToolStates { Grouping, Idling };
    private ToolStates currentState = ToolStates.Grouping;

    [SerializeField] private VRTK_ControllerEvents.ButtonAlias activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
    [SerializeField] private VRTK_ControllerEvents controllerEvents;
    [SerializeField] private string toolName;
    [SerializeField] private Color defaultColour;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private Dictionary<GameObject, Color> colourDictionary = new Dictionary<GameObject, Color>();

    private VRTK_ControllerReference controllerReference;
    private bool bActivationButtonPressed;
    private bool bCreatingNewObject;
    private IPointer variablePointer;
    private GameObject selectedObject;
    private GameObject newParentObject;
    private SCR_GroupParent newParentObjectScript;
    private Vector3 objectOffset;
    private Color objectStartColour;
    private Color currentGroupColour;

    //deals with custom highlighting
    private bool bHighlightingActive = true;
    private GameObject highlightedObject;
    private Renderer highlightedObjectRenderer;
    private Color highlightedObjectDefaultColour;

    private void OnEnable()
    {
        SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

        SCR_ToolOptions.instance.DeactivateOptions();

        controllerEvents.SubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
        controllerEvents.SubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonDepressed);
    }

    private void OnDisable()
    {
        bActivationButtonPressed = false;
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonDepressed);
    }

    private void DoActivationButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor)
        {
            if (controllerReference == null)
            {
                controllerReference = e.controllerReference;
            }

            if (!bActivationButtonPressed)
            {
                bActivationButtonPressed = true;
            }
        }
    }

    private void DoActivationButtonDepressed(object sender, ControllerInteractionEventArgs e)
    {
        if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor)
        {
            if (controllerReference == null)
            {
                controllerReference = e.controllerReference;
            }

            if (!bActivationButtonPressed && !SCR_ToolMenuRadial.instance.Busy)
            {
                bActivationButtonPressed = false;
            }
        }
    }

    private void Start()
    {
        GameObject variableObject = GameObject.FindGameObjectWithTag("RightVariable");

        if (variableObject)
        {
            variablePointer = (IPointer)variableObject.GetComponent(typeof(IPointer));
        }

        bCreatingNewObject = true;

    }

    private void Update()
    {
        switch (currentState)
        {
            case ToolStates.Grouping:
                GroupObjects();
                break;
            case ToolStates.Idling:
                ToolIdling();
                break;
            default:
                break;
        }
    }

    private void GroupObjects()
    {
        if (variablePointer.Active && variablePointer.ValidRaycastTarget && variablePointer.PointerHit.transform.parent == null)
        {
            HighlightObjects(variablePointer.PointerHit.collider.gameObject);

            if (bActivationButtonPressed)
            {
                bBusy = true;

                variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

                //create parent first time
                if (bCreatingNewObject)
                {
                    bCreatingNewObject = false;
                    SCR_SaveSystem.instance.GroupNumber++;
                    newParentObject = new GameObject("Group" + SCR_SaveSystem.instance.GroupNumber);
                    newParentObject.transform.localPosition = variablePointer.PointerHit.transform.position;
                    newParentObject.transform.localScale = Vector3.one;
                    newParentObject.AddComponent<SCR_GroupParent>();
                    //currentGroupColour = new Color(Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), 1f);
                }

                newParentObjectScript = newParentObject.GetComponent<SCR_GroupParent>();
                selectedObject = variablePointer.PointerHit.transform.gameObject;
                colourDictionary.Add(selectedObject, highlightedObjectDefaultColour);
                newParentObjectScript.groupedObjectList.Add(selectedObject);

                selectedObject.GetComponent<Renderer>().material.color = Color.red;

                if (newParentObjectScript.groupedObjectList.Count >= 1)
                {
                    ReCenter(newParentObjectScript);
                }

                VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);
                bActivationButtonPressed = false;
                //currentState = ToolStates.Moving;
            }
        }
        else if (variablePointer.Active && !variablePointer.ValidRaycastTarget)
        {
            if (bActivationButtonPressed)
            {
                //complete group and save

                VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
                bActivationButtonPressed = false;
                currentState = ToolStates.Idling;
            }
            RemoveHighlight();
        }
        else if (bBusy && bActivationButtonPressed)
        {
            if (variablePointer.PointerHit.transform.parent != null)
            {
                selectedObject = variablePointer.PointerHit.transform.gameObject;

                if (colourDictionary.ContainsKey(selectedObject))
                {
                    //selectedObject.GetComponent<Renderer>().material.color = objectStartColour;
                    selectedObject.GetComponent<Renderer>().material.color = colourDictionary[selectedObject];
                    selectedObject.transform.parent = null;
                    newParentObjectScript.groupedObjectList.Remove(selectedObject);
                    colourDictionary.Remove(selectedObject);
                    bActivationButtonPressed = false;
                    VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);
                    return;
                }
                /*
                if (newParentObjectScript.groupedObjectList.Contains(selectedObject))
                {
                    
                }
                */
            }
        }
        else
        {
            RemoveHighlight();
            bActivationButtonPressed = false;
        }
    }

    private void ToolIdling()
    {
        
        bCreatingNewObject = true;

        if (newParentObjectScript)
        {
            if (newParentObjectScript.groupedObjectList.Count >= 1)
            {
                print("in idling");
                foreach (var item in newParentObjectScript.groupedObjectList)
                {
                    item.GetComponent<SCR_ObjectData>().parentName = newParentObject.name;
                    item.GetComponent<SCR_ObjectData>().parentID = SCR_SaveSystem.instance.GroupNumber;
                    item.GetComponent<Renderer>().material.color = colourDictionary[item];
                    //item.GetComponent<Renderer>().material.color = currentGroupColour;
                }

                newParentObjectScript.ID = SCR_SaveSystem.instance.GroupNumber;
                SCR_SaveSystem.instance.AddParent(newParentObject);
                newParentObjectScript = null;

            }
            else
            {
                Destroy(newParentObject);
            }
        }

        colourDictionary.Clear();

        bBusy = false;

        variablePointer.SetPointerColourDefault();

        currentState = ToolStates.Grouping;
    }

    private void ReCenter(SCR_GroupParent groupParentScript)
    {
        foreach (var item in groupParentScript.groupedObjectList)
        {
            item.transform.parent = null;
        }

        newParentObject.transform.position = FindCentre(groupParentScript.groupedObjectList);

        foreach(var item in groupParentScript.groupedObjectList)
        {
            item.transform.parent = newParentObject.transform;
        }

    }

    //custom highlighting code
    void HighlightObjects(GameObject newObject)
    {
        if (bHighlightingActive)
        {
            if (highlightedObject != newObject)
            {
                if (highlightedObjectRenderer != null)
                {
                    highlightedObjectRenderer.material.color = highlightedObjectDefaultColour;
                }

                highlightedObject = newObject;
                highlightedObjectRenderer = highlightedObject.GetComponent<Renderer>();
                highlightedObjectDefaultColour = highlightedObjectRenderer.material.color;
                highlightedObjectRenderer.material.color = SCR_ToolMenuRadial.instance.highlightedGameObjectColour;
            }
        }
    }

    void RemoveHighlight()
    {
        if (highlightedObjectRenderer != null)
        {
            if (newParentObjectScript != null)
            {
                if (newParentObjectScript.groupedObjectList.Contains(highlightedObject))
                {
                    highlightedObjectRenderer.material.color = Color.red;
                }
                else
                {
                    highlightedObjectRenderer.material.color = highlightedObjectDefaultColour;
                }
            }
            else
            {
                highlightedObjectRenderer.material.color = highlightedObjectDefaultColour;
            }
            
            highlightedObject = null;
            highlightedObjectRenderer = null;
        }

        
    }

    //Adapated from robertu (https://answers.unity.com/questions/511841/how-to-make-an-object-move-away-from-three-or-more.html)
    private Vector3 FindCentre(List<GameObject> targets)
    {
        if (targets.Count == 0)
        {
            return Vector3.zero;
        }
            
        if (targets.Count == 1)
        {
            return targets[0].transform.position;
        }
            
        var bounds = new Bounds(targets[0].GetComponent<Renderer>().bounds.center, Vector3.zero);

        for (var i = 1; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].GetComponent<Renderer>().bounds.center);
        }
            
        return SCR_GridSnappingOption.instance.GetNearestPointOnGrid(bounds.center);
    }
}
