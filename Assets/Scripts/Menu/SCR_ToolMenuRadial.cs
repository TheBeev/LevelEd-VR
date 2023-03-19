using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;
using TMPro;

public class SCR_ToolMenuRadial : MonoBehaviour
{
    public static SCR_ToolMenuRadial instance;
    public Color defaultObjectColour;
    public Color defaultOutlineColour;
    public Color highlightedObjectColour;
    public Color highlightedOutlineColour;
    public Color selectedObjectColour;
    public Color selectedOutlineColour;
    public Color toolBusyPointerColour;
    public Color highlightedGameObjectColour;

    [SerializeField] private VRTK_ControllerEvents.ButtonAlias activationButtonTool = VRTK_ControllerEvents.ButtonAlias.ButtonOnePress;
    [SerializeField] private VRTK_ControllerEvents.ButtonAlias activationButtonEdit = VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress;
    [SerializeField] private VRTK_ControllerEvents.ButtonAlias selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
    [SerializeField] private VRTK_ControllerEvents controllerEvents;
    [SerializeField] private GameObject toolMenuUI;
    [SerializeField] private GameObject editMenuUI;
    [SerializeField] private float menuDistanceFromPointer = 0.05f;
    [SerializeField] private Transform headsetCentre;
    [SerializeField] private TextMeshProUGUI toolText;
    [SerializeField] private GameObject defaultTool;
    [SerializeField] private GameObject defaultMenuItem;

    private bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private GameObject currentTool;

    private RectTransform toolMenuRectTransform;
    private Vector3 overlapBoxExtents;
    private Vector3 centre;
    //private int geometryLayer;

    private PointerStates previousPointerState;
    private int menuItemLayer;
    private bool bToolMenuOpen;
    private bool bEditMenuOpen;
    private bool bSelectionTriggerPressed;
    private RaycastHit pointerHit;
    private IToolMenuItem currentMenuItem;
    private GameObject currentMenuItemGO;
    private GameObject currentToolMenuItem;
    private GameObject currentHighlightedObject;
    private GameObject currentActivePopoutObject;

    private Color previousObjectColour;
    private Color previousObjectOutlineColour;
    private GameObject previousSelectedObject;

    private VRTK_ControllerReference controllerReference;

    private IPointer variablePointer;
    private GameObject variableObject;

    private void Awake()
    {
        if (instance == null)
        {
            //DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        defaultMenuItem.GetComponent<IToolMenuItem>().OnSelected();

        previousSelectedObject = defaultMenuItem;
        previousSelectedObject.GetComponent<Renderer>().material.color = selectedObjectColour;
        //previousSelectedObject.GetComponent<SCR_Outline>().OutlineColor = selectedOutlineColour;
    }

    // Use this for initialization
    void Start()
    {
        //geometryLayer = 1 << 8;
        menuItemLayer = 1 << 9;

        variableObject = GameObject.FindGameObjectWithTag("RightVariable");

        if (variableObject)
        {
            variablePointer = (IPointer)variableObject.GetComponent(typeof(IPointer));
        }
    }

    private void OnEnable()
    {
        controllerEvents.SubscribeToButtonAliasEvent(activationButtonTool, true, DoActivationButtonToolPressed);
        controllerEvents.SubscribeToButtonAliasEvent(activationButtonEdit, true, DoActivationButtonEditPressed);
        controllerEvents.SubscribeToButtonAliasEvent(selectionButton, true, DoActivationButtonSelectionPressed);
        controllerEvents.SubscribeToButtonAliasEvent(selectionButton, false, DoActivationButtonSelectionDepressed);
    }

    private void OnDisable()
    {
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButtonTool, true, DoActivationButtonToolPressed);
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButtonEdit, true, DoActivationButtonEditPressed);
        controllerEvents.UnsubscribeToButtonAliasEvent(selectionButton, true, DoActivationButtonSelectionPressed);
        controllerEvents.UnsubscribeToButtonAliasEvent(selectionButton, false, DoActivationButtonSelectionDepressed);

        if (bEditMenuOpen)
        {
            MenuClose(false);
        }

        if (bToolMenuOpen)
        {
            MenuClose(true);
        }
    }

    void DoActivationButtonSelectionPressed(object sender, ControllerInteractionEventArgs e)
    {
        bSelectionTriggerPressed = true;

        if (currentHighlightedObject)
        {

            if (previousSelectedObject)
            {
                if (previousSelectedObject != currentHighlightedObject)
                {
                    previousSelectedObject.SetActive(true);
                    previousSelectedObject.GetComponent<IToolMenuItem>().Deselected();
                    //previousSelectedObject.SetActive(false);
                    previousSelectedObject = currentHighlightedObject;
                }
            }

            currentHighlightedObject.GetComponent<IToolMenuItem>().OnSelected();
            VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 2.0f);

        }

        previousObjectColour = defaultObjectColour;
        previousObjectOutlineColour = defaultOutlineColour;
        

        if (bToolMenuOpen)
        {
            if (currentHighlightedObject)
            {
                if (currentHighlightedObject.GetComponent<IToolMenuItem>().CloseMenuOnSelection)
                {
                    MenuClose(true);
                }
            }
            else
            {
                
            }  
        }
        else if(bEditMenuOpen)
        {
            MenuClose(false);
        }

        currentHighlightedObject = null;

    }

    void DoActivationButtonToolPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (bToolMenuOpen)
        {
            MenuClose(true);
            bBusy = false;
        }
        else if(bEditMenuOpen)
        {
            MenuClose(false);
            MenuOpen(true, e);
        }
        else
        {
            MenuOpen(true, e);
        }  
    }

    void DoActivationButtonEditPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (bEditMenuOpen)
        {
            MenuClose(false);
            bBusy = false;
        }
        else if(bToolMenuOpen)
        {
            MenuClose(true);
            MenuOpen(false, e);
        }
        else
        {
            MenuOpen(false, e);
        }    
    }
    
    void DoActivationButtonSelectionDepressed(object sender, ControllerInteractionEventArgs e)
    {
        bBusy = false;
    }

    void MenuOpen(bool bToolMenu, ControllerInteractionEventArgs e)
    {
        if (!bToolMenuOpen && !bEditMenuOpen)
        {
            if (controllerReference == null)
            {
                controllerReference = e.controllerReference;
            }

            bBusy = true;

            if (!currentTool.GetComponent<ITool>().Busy)
            {
                previousPointerState = variablePointer.CurrentPointerState;
                variablePointer.FreezePointerState = true;
                variablePointer.SnapPointerState(PointerStates.Short);
                VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f);


                if (bToolMenu)
                {
                    toolMenuUI.transform.position = variablePointer.PointerEndGameObject.transform.position + (variablePointer.PointerEndGameObject.transform.forward * menuDistanceFromPointer);
                    toolMenuUI.transform.rotation = variablePointer.PointerEndGameObject.transform.rotation;
                    toolMenuUI.transform.LookAt(headsetCentre);
                    toolMenuUI.SetActive(true);
                    bToolMenuOpen = true;
                }
                else
                {
                    editMenuUI.transform.position = variablePointer.PointerEndGameObject.transform.position + (variablePointer.PointerEndGameObject.transform.forward * menuDistanceFromPointer);
                    editMenuUI.transform.rotation = variablePointer.PointerEndGameObject.transform.rotation;
                    editMenuUI.transform.LookAt(headsetCentre);
                    editMenuUI.SetActive(true);
                    bEditMenuOpen = true;
                }
            }
        }
    }

    public void TempCloseMenu()
    {
        if (bToolMenuOpen || bEditMenuOpen)
        {
            if (toolMenuUI)
            {
                toolMenuUI.SetActive(false);
            }

            bToolMenuOpen = false;

            if (editMenuUI)
            {
                editMenuUI.SetActive(false);
            }

            bEditMenuOpen = false;

            bBusy = false;

            if (variableObject)
            {
                variablePointer.SnapPointerState(previousPointerState);
                variablePointer.FreezePointerState = false;
            }
        }
        
    }

    void MenuClose(bool bToolMenu)
    {
        if (bToolMenu)
        {
            if (toolMenuUI)
            {
                toolMenuUI.SetActive(false);
            }
            
            bToolMenuOpen = false;
        }
        else
        {
            if (editMenuUI)
            {
                editMenuUI.SetActive(false);
            }
            
            bEditMenuOpen = false;
        }

        if (variableObject)
        {
            variablePointer.SnapPointerState(previousPointerState);
            variablePointer.FreezePointerState = false;
        }
        
 
    }

    public void ToolChanged(GameObject newTool, string toolName)
    {
        if (currentTool && currentTool != newTool)
        {
            currentTool.SetActive(false);
        }

        currentTool = newTool;
        toolText.text = toolName;
    }
    public bool ToolBusy()
    {
        return currentTool.GetComponent<ITool>().Busy;
    }

    // Update is called once per frame
    void Update()
    {
        if (bToolMenuOpen || bEditMenuOpen)
        {
            if (Physics.Raycast(variablePointer.PointerLineRendererStartTransform.position, variablePointer.PointerLineRendererStartTransform.forward, out pointerHit, 10.0f, menuItemLayer))
            {
                if (currentHighlightedObject != pointerHit.transform.gameObject)
                {
                    if (currentHighlightedObject)
                    {
                        currentHighlightedObject.GetComponent<IToolMenuItem>().Unhighlighted();
                    }

                    currentHighlightedObject = pointerHit.transform.gameObject;

                    //deals with hiding and revealing popout options
                    if (currentHighlightedObject.GetComponent<IMenuPopout>() != null)
                    {
                        currentHighlightedObject.GetComponent<IMenuPopout>().ActivatePopout();
                        currentActivePopoutObject = currentHighlightedObject;
                    }
                    else
                    {
                        if (currentActivePopoutObject != null)
                        {
                            if (currentHighlightedObject.GetComponent<IMenuPopoutItem>() == null)
                            {
                                currentActivePopoutObject.GetComponent<IMenuPopout>().DeactivatePopout();
                            }  
                        }
                    }

                    currentHighlightedObject.GetComponent<IToolMenuItem>().Highlighted();
                    VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f);
                }
            }
            else
            {
                if (currentHighlightedObject)
                {
                    currentHighlightedObject.GetComponent<IToolMenuItem>().Unhighlighted();
                    currentHighlightedObject = null;
                }
            }
        }
    }
}
