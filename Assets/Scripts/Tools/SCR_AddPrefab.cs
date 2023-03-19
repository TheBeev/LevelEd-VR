using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SCR_AddPrefab : MonoBehaviour, ITool {

    private enum ToolStates { Placing, Adding };
    private ToolStates currentState = ToolStates.Placing;

    [SerializeField] private VRTK_ControllerEvents.ButtonAlias activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
    [SerializeField] private VRTK_ControllerEvents controllerEvents;
    [SerializeField] private VRTK_ControllerEvents controllerEventsLeft;
    [SerializeField] private string toolName;
    [SerializeField] private Material guidePrefabMaterial;

    [Header("Shortcuts")]
    [SerializeField] private bool bAllowShortcuts;
    [SerializeField] private float rotationSpeedMultiplier;
    [SerializeField] private float scaleSpeedMultiplier;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private GameObject prefabToSpawn;
    public GameObject PrefabToSpawn
    {
        get { return prefabToSpawn; }
        set { prefabToSpawn = value; }
    }

    private VRTK_ControllerReference controllerReference;
    private bool bActivationButtonPressed;
    private IPointer variablePointer;
    private Vector3 startLocation;
    private Vector3 pointerLocation;
    private Vector3 offsetLocation;
    private SCR_PrefabData prefabDataScript;
    private GameObject newlyCreatedPrefab;
    private GameObject guidePrefab;
    private string previousName;

    private bool bSnap;

    private void OnEnable()
    {
        SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

        SCR_ToolOptions.instance.DeactivateOptions();
        SCR_GridSnappingOption.instance.ActivateOption();
        SCR_SurfaceSnappingOption.instance.ActivateOption();
        SCR_PrefabRotateTowardsOption.instance.ActivateOption();

        SpawnGuidePrefab();

        controllerEvents.SubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
    }

    private void OnDisable()
    {
        bActivationButtonPressed = false;
        Destroy(guidePrefab);
        guidePrefab = null;
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
    void Update ()
    {
        switch (currentState)
        {
            case ToolStates.Placing:
                PlacingObject();
                break;
            case ToolStates.Adding:
                AddingObject();
                break;
            default:
                break;
        }

        if (guidePrefab != null)
        {

            if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
            {
                guidePrefab.transform.position = Snap(variablePointer.PointerPosition);
            }
            else
            {
                guidePrefab.transform.position = variablePointer.PointerPosition;
            }

            if (SCR_PrefabRotateTowardsOption.instance.PrefabRotateTowardsActive == OptionActive.On)
            {
                guidePrefab.transform.LookAt(new Vector3(VRTK_DeviceFinder.HeadsetTransform().transform.position.x, guidePrefab.transform.position.y, VRTK_DeviceFinder.HeadsetTransform().transform.position.z));
            }

            if (SCR_ToolMenuRadial.instance.Busy && guidePrefab.activeInHierarchy)
            {
                guidePrefab.SetActive(false);
            }
            else if (!SCR_ToolMenuRadial.instance.Busy && !guidePrefab.activeInHierarchy)
            {
                guidePrefab.SetActive(true);
            }
        }
    }

    //proxy prefab is used as a guide so users can see what they're spawning and where it's going. 
    void SpawnGuidePrefab()
    {
        if (variablePointer == null)
        {
            Start();
        }

        if (prefabToSpawn.tag == "CantCopy")
        {
            GameObject objectToCheck = GameObject.Find(prefabToSpawn.name + "(Clone)");
            if (objectToCheck)
            {
                return;
            }
        }

        variablePointer.SnapPointerState(PointerStates.Medium);

        guidePrefab = Instantiate(prefabToSpawn, variablePointer.PointerPosition, prefabToSpawn.transform.rotation);

        SCR_PrefabData prefabDataScript = guidePrefab.GetComponent<SCR_PrefabData>();

        //this deals with objects that must be unique, like game managers.
        if (guidePrefab.tag == "CantCopy")
        {
            previousName = guidePrefab.name;
            guidePrefab.tag = "Untagged";
            guidePrefab.name = "GuidePrefab";

            if (prefabDataScript)
            {
                prefabDataScript.CurrentlyGuiding(guidePrefabMaterial, false);
            }
        }
        else
        {
            if (prefabDataScript)
            {
                prefabDataScript.CurrentlyGuiding(guidePrefabMaterial, true);
            }
        }
    }

    void PlacingObject()
    {
        if (variablePointer != null)
        {
            if (variablePointer.Active)
            {
                //deals with shortcuts for rotating and scaling whilst placing the prefab
                if (bAllowShortcuts)
                {
                    if (SCR_PrefabRotateTowardsOption.instance.PrefabRotateTowardsActive == OptionActive.Off)
                    {
                        float rotationAmount = 0f;

                        if (controllerEvents.GetTouchpadAxis().x > 0.8f)
                        {
                            rotationAmount = controllerEvents.GetTouchpadAxis().x;
                        }
                        else if (controllerEvents.GetTouchpadAxis().x < -0.8f)
                        {
                            rotationAmount = controllerEvents.GetTouchpadAxis().x;
                        }

                        if (guidePrefab != null)
                        {
                            guidePrefab.transform.Rotate(new Vector3(0f, rotationAmount * rotationSpeedMultiplier, 0f), Space.Self);
                        }
                    }

                    float scaleAmount = 0f;

                    if (controllerEventsLeft.GetTouchpadAxis().x > 0.8f)
                    {
                        scaleAmount = controllerEventsLeft.GetTouchpadAxis().x;
                    }
                    else if (controllerEventsLeft.GetTouchpadAxis().x < -0.8f)
                    {
                        scaleAmount = controllerEventsLeft.GetTouchpadAxis().x;
                    }

                    scaleAmount *= scaleSpeedMultiplier;

                    if (guidePrefab != null)
                    {
                        guidePrefab.transform.localScale = new Vector3(Mathf.Max(guidePrefab.transform.localScale.x + scaleAmount, 0.05f), Mathf.Max(guidePrefab.transform.localScale.y + scaleAmount, 0.05f), Mathf.Max(guidePrefab.transform.localScale.z + scaleAmount, 0.05f));
                    }

                }
                

                if (bActivationButtonPressed)
                {
                    if (prefabToSpawn.tag == "CantCopy")
                    {
                        GameObject objectToCheck = GameObject.Find(prefabToSpawn.name + "(Clone)");
                        if (objectToCheck)
                        {
                            bActivationButtonPressed = false;
                            return;
                        }
                    }

                    bBusy = true;
                    bActivationButtonPressed = false;
                    Vector3 locationToPlace;

                    if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
                    {
                        locationToPlace = Snap(variablePointer.PointerPosition);
                    }
                    else
                    {
                        locationToPlace = variablePointer.PointerPosition;
                    }

                    if (prefabToSpawn.tag == "CantCopy")
                    {
                        guidePrefab.GetComponent<SCR_PrefabData>().StopGuiding();
                        newlyCreatedPrefab = guidePrefab;
                        newlyCreatedPrefab.name = previousName;
                        guidePrefab = null;
                    }
                    else
                    {
                        newlyCreatedPrefab = Instantiate(prefabToSpawn, locationToPlace, guidePrefab.transform.rotation);
                        newlyCreatedPrefab.transform.localScale = guidePrefab.transform.localScale;
                    }

                    if (SCR_PrefabRotateTowardsOption.instance.PrefabRotateTowardsActive == OptionActive.On)
                    {
                        newlyCreatedPrefab.transform.LookAt(new Vector3(VRTK_DeviceFinder.HeadsetTransform().transform.position.x, newlyCreatedPrefab.transform.position.y, VRTK_DeviceFinder.HeadsetTransform().transform.position.z));
                    }

                    VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
                    currentState = ToolStates.Adding;
                }
            }
        }
    }

    void AddingObject()
    {
        if (newlyCreatedPrefab.transform.root.GetComponent<IScriptable>() != null)
        {
            newlyCreatedPrefab.transform.root.GetComponent<IScriptable>().ConfigureNewScriptable();
            SCR_SaveSystem.instance.AddScript(newlyCreatedPrefab);
        }
        else
        {
            SCR_SaveSystem.instance.AddPrefab(newlyCreatedPrefab);
        }
        
        bBusy = false;
        currentState = ToolStates.Placing;
    }

    Vector3 Snap(Vector3 snapNearPoint)
    {
        return SCR_GridSnappingOption.instance.GetNearestPointOnGrid(snapNearPoint);
    }
}
