using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using UnityEngine.UI;

public class SCR_Rotate : MonoBehaviour, ITool {

    private enum ToolStates { Highlighting, Selecting, Rotating };
    private ToolStates currentState = ToolStates.Selecting;

    [SerializeField] private VRTK_ControllerEvents.ButtonAlias activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
    [SerializeField] private VRTK_ControllerEvents controllerEvents;
    [SerializeField] private string toolName;
    [SerializeField] private float rotationMultiplier = 1.0f;
    [SerializeField] private float snapRotationAmount = 15f;

    [Header("Widget Settings")]
    [SerializeField] private GameObject rotationWidgetPrefab;
    [SerializeField] private float widgetMinimumScale = 0.1f;
    [SerializeField] private float widgetMaximumScale = 2.0f;
    [SerializeField] private float widgetScaleMultiplier = 1.1f;


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
    private GameObject tempObjectForRotating;
    private Color objectStartColor;

    private AxisSelected previousAxis;
    private AxisSelected previousWidgetAxis;
    private GameObject widgetParent;
    private GameObject xAxisWidget;
    private GameObject yAxisWidget;
    private GameObject zAxisWidget;
    private SCR_WidgetData widgetData;
    private Transform headsetTransform;
    private float widgetScale;
    private float widgetStartingScale;
    private GameObject tempObjectForHighlighting;
    private bool bAllowShortcut;
    private bool bShortcutInputTriggered;

    private float rotationAmount;
    private Vector3 controllerStartPosition;

    //test stuff
    float previousX;
    float previousY;
    float previousZ;

    private SCR_ToolOptions toolOptions;
    private SCR_GroupParent groupParentScript;

    private void OnEnable()
    {
        SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

        if (toolOptions == null)
        {
            toolOptions = FindObjectOfType<SCR_ToolOptions>();
        }

        if (widgetParent == null)
        {
            widgetParent = Instantiate(rotationWidgetPrefab, transform.position, transform.rotation);
            widgetParent.SetActive(false);
            widgetData = widgetParent.GetComponent<SCR_WidgetData>();
            xAxisWidget = widgetData.widgetChildren[0];
            yAxisWidget = widgetData.widgetChildren[1];
            zAxisWidget = widgetData.widgetChildren[2];
            widgetStartingScale = widgetParent.transform.localScale.x;
        }

        headsetTransform = VRTK_DeviceFinder.HeadsetTransform();

        SCR_ToolOptions.instance.DeactivateOptions();
        SCR_GridSnappingOption.instance.ActivateOption();
        SCR_AxisOption.instance.ActivateOption();
        SCR_AxisOption.instance.AllowAllAxis(false);

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
            case ToolStates.Rotating:
                RotatingObject();
                break;
            default:
                break;
        }

        ShortcutHandling();
    }

    //Supports switching axis from the analogue stick
    void ShortcutHandling()
    {
        if (bAllowShortcut)
        {
            if (controllerEvents.GetTouchpadAxis().x > 0.5f)
            {
                if (!bShortcutInputTriggered)
                {
                    SCR_AxisOption.instance.ShortcutSwapAxisOption(false);
                }
                bShortcutInputTriggered = true;
            }
            else if (controllerEvents.GetTouchpadAxis().x < -0.5f)
            {
                if (!bShortcutInputTriggered)
                {
                    SCR_AxisOption.instance.ShortcutSwapAxisOption(true);
                }
                bShortcutInputTriggered = true;
            }
            else
            {
                bShortcutInputTriggered = false;
            }
        }  
    }

    void SelectingObject()
    {
        if (variablePointer.Active && variablePointer.ValidRaycastTarget && !variablePointer.PointerHit.transform.CompareTag("Floor"))
        {
            bAllowShortcut = true;
            if (variablePointer.PointerHit.transform.parent)
            {
                tempObjectForRotating = variablePointer.PointerHit.transform.root.gameObject;
            }
            else
            {
                if (variablePointer.PointerHit.transform.GetComponent<SCR_PrefabData>() != null)
                {
                    tempObjectForRotating = variablePointer.PointerHit.transform.gameObject;
                    tempObjectForRotating.transform.rotation = variablePointer.PointerHit.transform.gameObject.transform.rotation;
                }
                else
                {
                    if (tempObjectForHighlighting == null)
                    {
                        tempObjectForHighlighting = new GameObject("TempRotation");
                    }
                    tempObjectForRotating = tempObjectForHighlighting;
                    tempObjectForRotating.transform.rotation = variablePointer.PointerHit.transform.gameObject.transform.rotation;
                    tempObjectForRotating.transform.position = variablePointer.PointerHit.transform.gameObject.GetComponent<Renderer>().bounds.center;
                }

            }

            //Deal with the rotation widgets while highlighting so you can see which axis is currently selected
            widgetParent.SetActive(true);
            widgetParent.transform.position = tempObjectForRotating.transform.position;
            widgetParent.transform.rotation = tempObjectForRotating.transform.rotation;
            widgetParent.transform.parent = tempObjectForRotating.transform;

            //Deal with scale of roation widget based on distance
            WidgetScaling();
            WidgetAxisHighlighting();

            if (bActivationButtonPressed && !variablePointer.PointerHit.transform.CompareTag("Floor"))
            {
                bBusy = true;

                variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

                if (variablePointer.PointerHit.transform.parent)
                {
                    if (variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_GroupParent>() != null)
                    {
                        tempObjectForRotating = variablePointer.PointerHit.transform.root.gameObject;
                        objectStartColor = variablePointer.ObjectOriginalColour;
                        variablePointer.HighlightingActive = false;
                        variablePointer.RemoveHighlight();
                        groupParentScript = variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_GroupParent>();
                        groupParentScript.CheckMaterialCache();
                        groupParentScript.CurrentlySelected();
                    }
                    else if(variablePointer.PointerHit.transform.root.gameObject.GetComponent<SCR_PrefabData>() != null)
                    {
                        tempObjectForRotating = variablePointer.PointerHit.transform.root.gameObject;
                        tempObjectForRotating.GetComponent<SCR_PrefabData>().CurrentlySelected();
                    }
                    
                }
                else
                {
                    if (variablePointer.PointerHit.transform.GetComponent<SCR_PrefabData>() != null)
                    {
                        tempObjectForRotating = variablePointer.PointerHit.transform.gameObject;
                        tempObjectForRotating.GetComponent<SCR_PrefabData>().CurrentlySelected();
                    }
                    else if (variablePointer.PointerHit.transform.GetComponent<SCR_ObjectData>() != null)
                    {
                        selectedObject = variablePointer.PointerHit.transform.gameObject;
                        selectedObject.layer = 2;

                        objectStartColor = variablePointer.ObjectOriginalColour;
                        variablePointer.HighlightingActive = false;
                        variablePointer.RemoveHighlight();
                        selectedObject.GetComponent<Renderer>().material.color = Color.red;
                        tempObjectForRotating = new GameObject("TempRotation");
                        tempObjectForRotating.transform.rotation = selectedObject.transform.rotation;
                        tempObjectForRotating.transform.position = selectedObject.GetComponent<Renderer>().bounds.center;
                        selectedObject.transform.parent = tempObjectForRotating.transform;
                    }
                }

                controllerStartPosition = variableObject.transform.position;

                //Deal with the rotation widgets
                widgetParent.SetActive(true);
                widgetParent.transform.position = tempObjectForRotating.transform.position;
                //widgetParent.transform.localScale *= Mathf.Min(selectedObject.GetComponent<Renderer>().bounds.extents.magnitude * 2f, 1.5f);
                
                widgetParent.transform.rotation = tempObjectForRotating.transform.rotation;
                widgetParent.transform.parent = tempObjectForRotating.transform;

                VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);
                bActivationButtonPressed = false;
                currentState = ToolStates.Rotating;
            }

            bActivationButtonPressed = false;
        }
        else
        {
            widgetParent.transform.localScale = Vector3.one;
            widgetParent.transform.parent = null;
            widgetParent.SetActive(false);
            Destroy(tempObjectForHighlighting);
            bActivationButtonPressed = false;
        }
    }

    void RotatingObject()
    {
        float x = 0f;
        float y = 0f;
        float z = 0f;

        AxisSelected currentAxis = SCR_AxisOption.instance.CurrentAxis;

        WidgetAxisHighlighting();
        WidgetScaling();

        if (previousAxis != currentAxis)
        {
            controllerStartPosition = variableObject.transform.position;
            previousAxis = currentAxis;
            rotationAmount = 0f;
            previousX = 0f;
            previousY = 0f;
            previousZ = 0f;
        }
        else
        {
            rotationAmount = (controllerStartPosition.y - variableObject.transform.position.y) * rotationMultiplier;

            switch (currentAxis)
            {
                case AxisSelected.XAxis:
                    x -= rotationAmount;
                    break;
                case AxisSelected.YAxis:
                    y -= rotationAmount;
                    break;
                case AxisSelected.ZAxis:
                    z -= rotationAmount;
                    break;
                default:
                    break;
            }

            if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
            {
                tempObjectForRotating.transform.Rotate(new Vector3(RotationSnap(x) - previousX, RotationSnap(y) - previousY, RotationSnap(z) - previousZ), Space.Self);
                previousX = RotationSnap(x);
                previousY = RotationSnap(y);
                previousZ = RotationSnap(z);
            }
            else
            {
                tempObjectForRotating.transform.Rotate(new Vector3(x - previousX, y - previousY, z - previousZ), Space.Self);
                previousX = x;
                previousY = y;
                previousZ = z;
            }
        }

        if (bActivationButtonPressed)
        {
            bActivationButtonPressed = false;

            widgetParent.transform.parent = null;
            widgetParent.transform.localScale = Vector3.one;
            widgetParent.SetActive(false);

            previousX = 0f;
            previousY = 0f;
            previousZ = 0f;

            if (tempObjectForRotating.GetComponent<SCR_GroupParent>() != null)
            {
                groupParentScript.Deselected();
            }
            else if(tempObjectForRotating.GetComponent<SCR_PrefabData>() != null)
            {
                tempObjectForRotating.GetComponent<SCR_PrefabData>().Deselected();
            }
            else
            {
                selectedObject.layer = 8;
                selectedObject.GetComponent<Renderer>().material.color = objectStartColor;
                selectedObject.transform.parent = null;
                Destroy(tempObjectForRotating);
            }

            VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
            bBusy = false;

            variablePointer.SetPointerColourDefault();

            variablePointer.HighlightingActive = true;

            currentState = ToolStates.Selecting;
        }

    }

    //Deal with scale of roation widget based on distance
    void WidgetScaling()
    {
        Vector3 difference = headsetTransform.position - widgetParent.transform.position;
        float distance = difference.magnitude;
        widgetScale = Mathf.Min(widgetStartingScale * distance * widgetScaleMultiplier, widgetMaximumScale);
        widgetScale = Mathf.Max(widgetScale, widgetMinimumScale);
        widgetParent.transform.localScale = new Vector3(widgetScale, widgetScale, widgetScale);
    }

    //Changes which widget axis torus should be coloured (highlighted)
    void WidgetAxisHighlighting()
    {
        if (previousWidgetAxis != SCR_AxisOption.instance.CurrentAxis)
        {
            //this prevents users from selected the Free axis when in this rotate tool
            if (SCR_AxisOption.instance.CurrentAxis == AxisSelected.None)
            {
                SCR_AxisOption.instance.ShortcutSwapAxisOption(false);
            }

            widgetData.widgetChildrenRenderers[0].material = widgetData.defaultMaterial;
            widgetData.widgetChildrenRenderers[1].material = widgetData.defaultMaterial;
            widgetData.widgetChildrenRenderers[2].material = widgetData.defaultMaterial;

            switch (SCR_AxisOption.instance.CurrentAxis)
            {
                case AxisSelected.XAxis:
                    widgetData.widgetChildrenRenderers[0].material = widgetData.widgetChildrenMaterials[0];
                    previousWidgetAxis = AxisSelected.XAxis;
                    break;
                case AxisSelected.YAxis:
                    widgetData.widgetChildrenRenderers[1].material = widgetData.widgetChildrenMaterials[1];
                    previousWidgetAxis = AxisSelected.YAxis;
                    break;
                case AxisSelected.ZAxis:
                    widgetData.widgetChildrenRenderers[2].material = widgetData.widgetChildrenMaterials[2];
                    previousWidgetAxis = AxisSelected.ZAxis;
                    break;
            }
        }

        
    }

    //Snaps a value to a specific rotation amount (default 15 degrees)
    float RotationSnap(float newRotationAmount)
    {
        int newSnapAmount = Mathf.RoundToInt(newRotationAmount / snapRotationAmount);
        float result = (float)newSnapAmount * snapRotationAmount;
        return result;
    }
}
