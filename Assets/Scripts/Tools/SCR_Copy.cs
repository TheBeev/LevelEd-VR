using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SCR_Copy : MonoBehaviour, ITool {

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
    private GameObject objectToCopy;
    private GameObject selectedObject;
    private Vector3 objectOffset;
    private Vector3 testOffset;
    private Vector3 startLocation;
    private Vector3 pointerLocation;
    private Vector3 offsetLocation;
    private Color objectStartColor;

    //temp
    GameObject copiedGeometry;

    private SCR_GroupParent groupParentScript;
    private SCR_GroupParent newParentObjectScript;

    private void OnEnable()
    {
        SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

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
        if (variablePointer.Active && variablePointer.ValidRaycastTarget)
        {
            if (bActivationButtonPressed)
            {
                
                selectedObject = null;

                //is it grouped or not
                if (variablePointer.PointerHit.transform.parent)
                {
                    if (variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_GroupParent>() != null && !variablePointer.PointerHit.collider.CompareTag("CantCopy"))
                    {
                        selectedObject = variablePointer.PointerHit.transform.root.gameObject;
                        groupParentScript = variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_GroupParent>();
                        //objectStartColor = variablePointer.ObjectOriginalColour;

                        //create new group parent
                        SCR_SaveSystem.instance.GroupNumber++;
                        GameObject newParentObject = new GameObject("Group" + SCR_SaveSystem.instance.GroupNumber);
                        newParentObject.transform.position = selectedObject.transform.position;
                        newParentObject.transform.localScale = selectedObject.transform.localScale;
                        newParentObjectScript = newParentObject.AddComponent<SCR_GroupParent>();
                        SCR_SaveSystem.instance.AddParent(newParentObject);
                        newParentObjectScript.ID = SCR_SaveSystem.instance.GroupNumber;

                        foreach (var item in groupParentScript.groupedObjectList)
                        {

                            copiedGeometry = Instantiate(item, item.transform.position, item.transform.rotation);
                            copiedGeometry.transform.parent = null;
                            copiedGeometry.transform.localScale = item.transform.localScale;
                            
                            SCR_SaveSystem.instance.ObjectIDNumber++;
                            copiedGeometry.name = "Geometry" + SCR_SaveSystem.instance.ObjectIDNumber;
                            SCR_SaveSystem.instance.AddGeometry(copiedGeometry);

                            //this isn't necessary because the geometry registers itself with SCR_ObjectData
                            //may need adding back in this ever changes.
                            //newParentObjectScript.groupedObjectList.Add(copiedGeometry);

                            copiedGeometry.GetComponent<SCR_ObjectData>().parentName = newParentObject.name;
                            copiedGeometry.GetComponent<SCR_ObjectData>().parentID = SCR_SaveSystem.instance.GroupNumber;
                            copiedGeometry.GetComponent<SCR_ObjectData>().localScaleAgainstParent = item.transform.localScale;
                            copiedGeometry.GetComponent<SCR_ObjectData>().Start();
                        }

                        //newParentObject.transform.position = FindCentre(newParentObjectScript.groupedObjectList);

                        foreach (var item in newParentObjectScript.groupedObjectList)
                        {
                            //item.transform.parent = newParentObject.transform;
                        }

                        variablePointer.RemoveHighlight();
                        newParentObjectScript.Reset();
                        newParentObjectScript.CurrentlySelected();

                        objectOffset = newParentObject.transform.position - variablePointer.PointerHit.point;

                        startLocation = newParentObject.transform.position;
                        testOffset = startLocation - variablePointer.PointerPosition;
                        
                        selectedObject = newParentObject;

                    }
                    else if (variablePointer.PointerHit.transform.root.gameObject.GetComponent<IScriptable>() != null && !variablePointer.PointerHit.collider.CompareTag("CantCopy"))
                    {
                        objectToCopy = variablePointer.PointerHit.transform.root.gameObject;
                        objectToCopy.GetComponent<SCR_PrefabData>().StopHighlighting();
                        selectedObject = Instantiate(objectToCopy, objectToCopy.transform.position, objectToCopy.transform.rotation);
                        objectOffset = selectedObject.transform.position - variablePointer.PointerHit.point;
                        startLocation = selectedObject.transform.position;
                        testOffset = startLocation - variablePointer.PointerPosition;

                        foreach (Transform trans in selectedObject.GetComponentsInChildren<Transform>(true))
                        {
                            trans.gameObject.layer = 2;
                        }

                        selectedObject.GetComponent<IScriptable>().ResetNode();
                        selectedObject.GetComponent<IScriptable>().ConfigureNewScriptable();

                        selectedObject.GetComponent<SCR_PrefabData>().CurrentlySelected();
                        SCR_SaveSystem.instance.AddScript(selectedObject);

                    }
                    else if (variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_PrefabData>() != null && !variablePointer.PointerHit.collider.CompareTag("CantCopy"))
                    {
                        objectToCopy = variablePointer.PointerHit.transform.root.gameObject;
                        objectToCopy.GetComponent<SCR_PrefabData>().StopHighlighting();
                        selectedObject = Instantiate(objectToCopy, objectToCopy.transform.position, objectToCopy.transform.rotation);
                        objectOffset = selectedObject.transform.position - variablePointer.PointerHit.point;
                        startLocation = selectedObject.transform.position;
                        testOffset = startLocation - variablePointer.PointerPosition;

                        foreach (Transform trans in selectedObject.GetComponentsInChildren<Transform>(true))
                        {
                            trans.gameObject.layer = 2;
                        }

                        selectedObject.GetComponent<SCR_PrefabData>().CurrentlySelected();
                        SCR_SaveSystem.instance.AddPrefab(selectedObject);

                    }
                }
                else
                {
                    if (variablePointer.PointerHit.transform.GetComponent<IScriptable>() != null && !variablePointer.PointerHit.collider.CompareTag("CantCopy"))
                    {

                        objectToCopy = variablePointer.PointerHit.transform.gameObject;
                        selectedObject = Instantiate(objectToCopy, objectToCopy.transform.position, objectToCopy.transform.rotation);
                        selectedObject.GetComponent<SCR_PrefabData>().CurrentlySelected();
                        objectOffset = selectedObject.transform.position - variablePointer.PointerHit.point;
                        startLocation = selectedObject.transform.position;
                        testOffset = startLocation - variablePointer.PointerPosition;
                        selectedObject.layer = 2;

                        SCR_SaveSystem.instance.AddScript(selectedObject);

                    }
                    else if (variablePointer.PointerHit.transform.GetComponent<SCR_PrefabData>() != null && !variablePointer.PointerHit.collider.CompareTag("CantCopy"))
                    {
                        objectToCopy = variablePointer.PointerHit.transform.gameObject;
                        selectedObject = Instantiate(objectToCopy, objectToCopy.transform.position, objectToCopy.transform.rotation);
                        selectedObject.GetComponent<SCR_PrefabData>().CurrentlySelected();
                        objectOffset = selectedObject.transform.position - variablePointer.PointerHit.point;
                        startLocation = selectedObject.transform.position;
                        testOffset = startLocation - variablePointer.PointerPosition;
                        selectedObject.layer = 2;

                        SCR_SaveSystem.instance.AddPrefab(selectedObject);

                    }
                    else if(variablePointer.PointerHit.transform.GetComponent<SCR_ObjectData>() != null && !variablePointer.PointerHit.collider.CompareTag("CantCopy"))
                    {
                        objectToCopy = variablePointer.PointerHit.transform.gameObject;
                        selectedObject = Instantiate(objectToCopy, objectToCopy.transform.position, objectToCopy.transform.rotation);
                        selectedObject.layer = 2;
                        objectStartColor = variablePointer.ObjectOriginalColour;
                        selectedObject.GetComponent<Renderer>().material.color = Color.red;

                        objectOffset = selectedObject.transform.position - variablePointer.PointerHit.point;

                        startLocation = selectedObject.transform.position;
                        testOffset = startLocation - variablePointer.PointerPosition;

                        SCR_SaveSystem.instance.ObjectIDNumber++;
                        selectedObject.name = "Geometry" + SCR_SaveSystem.instance.ObjectIDNumber;
                        SCR_SaveSystem.instance.AddGeometry(selectedObject);

                        
                    }
                }

                

                //SCR_ToolOptions.instance.DeactivateOptions();
                //SCR_GridSnappingOption.instance.ActivateOption();

                if (selectedObject)
                {
                    bBusy = true;
                    variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

                    if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
                    {
                        variablePointer.LockPointerLength(true);
                    }

                    currentState = ToolStates.Moving;
                }

                VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);
                bActivationButtonPressed = false;
                
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
                selectedObject.GetComponent<SCR_GroupParent>().Deselected();
                foreach (Transform trans in selectedObject.GetComponentsInChildren<Transform>(true))
                {
                    trans.gameObject.layer = 8;
                }
            }
            else if(selectedObject.GetComponent<IScriptable>() != null)
            {
                selectedObject.GetComponent<SCR_PrefabData>().Deselected();
                foreach (Transform trans in selectedObject.GetComponentsInChildren<Transform>(true))
                {
                    trans.gameObject.layer = 8;
                }
            }
            else if(selectedObject.GetComponent<SCR_PrefabData>() != null)
            {
                selectedObject.GetComponent<SCR_PrefabData>().Deselected();
                foreach (Transform trans in selectedObject.GetComponentsInChildren<Transform>(true))
                {
                    trans.gameObject.layer = 8;
                }
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

            VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
            bBusy = false;
            //SCR_ToolOptions.instance.DeactivateOptions();
            //SCR_GridSnappingOption.instance.ActivateOption();
            //SCR_SurfaceSnappingOption.instance.ActivateOption();

            variablePointer.SetPointerColourDefault();

            currentState = ToolStates.Selecting;
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

    Vector3 Snap(Vector3 snapNearPoint)
    {
        return SCR_GridSnappingOption.instance.GetNearestPointOnGrid(snapNearPoint);
    }
}
