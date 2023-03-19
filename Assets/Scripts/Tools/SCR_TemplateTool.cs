using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SCR_TemplateTool : MonoBehaviour, ITool {

    private enum ToolStates { State1, State2 };
    private ToolStates currentState = ToolStates.State1;

    [SerializeField] private VRTK_ControllerEvents.ButtonAlias activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
    [SerializeField] private VRTK_ControllerEvents controllerEvents;
    [SerializeField] private string toolName;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private void OnEnable()
    {
        //set this as the current tool
        //enable/disable grid snapping, surface snapping, etc.
        //get a reference to the variable pointer
        //subscribe to the appropriate controller events
    }

    private void OnDisable()
    {
        //unsubscribe from controller events
    }

    void DoActivationButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor)
        {
            //deal with button press
        }
    }

    // Use this for initialization
    void Start()
    {
        //get a reference to the variable pointer object
    }

    // Call the current function for the give state of the tool. Could be more than two.
    void Update()
    {
        switch (currentState)
        {
            case ToolStates.State1:
                State1Code();
                break;
            case ToolStates.State2:
                State2Code();
                break;
            default:
                break;
        }
    }

    void State1Code()
    {
        //Add code here that should run during this tool state
    }

    void State2Code()
    {
        //Add code here that should run during this tool state
    }
}
