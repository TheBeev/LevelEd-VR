using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SCR_SimpleTeleport : MonoBehaviour {

    [SerializeField] private VRTK_ControllerEvents.ButtonAlias activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
    //[SerializeField] private VRTK_ControllerEvents.ButtonAlias secondaryActivationButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
    [SerializeField] private VRTK_ControllerEvents controllerEvents;
    [SerializeField] private GameObject teleporterEffectPrefab;
    [SerializeField] private Color teleportPointerColour;
    [SerializeField] private float pointerStartWidth = 0.02f;
    [SerializeField] private float pointerEndWidth = 0.02f;
    [SerializeField] private float maxTeleportDistance = 1000f;

    private VRTK_HeadsetFade headsetFadeScript;

    private Transform playerTransform;
    private Transform headsetTransform;
    private Vector3 headsetOffset;
    private Vector3 objectOffset; //based on normal of object raycast hits
    private Vector3 newTeleportPosition;
    private WaitForSeconds fadeDelay = new WaitForSeconds(0.25f);
    private bool bCurrentlyTeleporting;

    private VRTK_ControllerReference controllerReference;
    private bool bActivationButtonPressed;
    private bool bTeleportActivated;
    private IPointer variablePointer;
    private GameObject teleportEffectGameObject;

    private void OnEnable()
    {
        //SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

        //SCR_ToolOptions.instance.DeactivateOptions();
        //SCR_GridSnappingOption.instance.ActivateOption();
        //SCR_SurfaceSnappingOption.instance.ActivateOption();
        
        controllerEvents.SubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
        //controllerEvents.SubscribeToButtonAliasEvent(secondaryActivationButton, true, DoSecondaryActivationButtonPressed);
        controllerEvents.SubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonReleased);
    }

    private void OnDisable()
    {
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonReleased);

        if (variablePointer != null)
        {
            variablePointer.VariablePointerActive = false;
            variablePointer.SnapPointerState(PointerStates.Short);
            variablePointer.SetPointerColourDefault();
            variablePointer.SetPointerWidthDefault();
            variablePointer.SetPointerEndSizeDefault();
        }

        if (teleportEffectGameObject)
        {
            teleportEffectGameObject.SetActive(false);
        }

        bTeleportActivated = false;
        bCurrentlyTeleporting = false;
        bActivationButtonPressed = false;
    }

    void DoSecondaryActivationButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        
    }

    void DoActivationButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor && !SCR_ToolOptions.instance.OptionsMenuOpen)
        {
            if (controllerReference == null)
            {
                controllerReference = e.controllerReference;
            }

            if (!bActivationButtonPressed && !SCR_ToolMenuRadial.instance.Busy)
            {
                bActivationButtonPressed = true;

                if (teleportEffectGameObject == null)
                {
                    teleportEffectGameObject = Instantiate(teleporterEffectPrefab, variablePointer.PointerEndGameObject.transform.position, teleporterEffectPrefab.transform.rotation);
                }
                else
                {
                    teleportEffectGameObject.SetActive(true);
                }

                variablePointer.SetPointerColour(teleportPointerColour);
                variablePointer.SetPointerEndSize(0.1f);
                variablePointer.SetPointerWidth(pointerStartWidth, pointerEndWidth);
                variablePointer.VariablePointerActive = true;
                variablePointer.SnapPointerState(PointerStates.Medium);
            }
        }
    }

    void DoActivationButtonReleased(object sender, ControllerInteractionEventArgs e)
    {
        bActivationButtonPressed = false;
        

        if (bTeleportActivated && !SCR_ToolOptions.instance.OptionsMenuOpen)
        {
            if (!bCurrentlyTeleporting)
            {
                StartCoroutine(TeleportHereRoutine());
            }
        }
        else
        {
            variablePointer.VariablePointerActive = false;
            variablePointer.SnapPointerState(PointerStates.Short);

            if (teleportEffectGameObject == null)
            {
                teleportEffectGameObject = Instantiate(teleporterEffectPrefab, variablePointer.PointerEndGameObject.transform.position, teleporterEffectPrefab.transform.rotation);
            }
            else
            {
                teleportEffectGameObject.SetActive(false);
            }

            variablePointer.SetPointerColourDefault();
            variablePointer.SetPointerWidthDefault();
            variablePointer.SetPointerEndSizeDefault();
            bTeleportActivated = false;
            bCurrentlyTeleporting = false;
        }

    }

    IEnumerator TeleportHereRoutine()
    {
        bTeleportActivated = false;
        bCurrentlyTeleporting = true;
        headsetFadeScript.Fade(Color.black, 0.25f);

        SCR_ToolMenuRadial.instance.TempCloseMenu();
        SCR_ToolOptions.instance.TempOptionsMenuClose();

        if (headsetTransform == null)
        {
            headsetTransform = VRTK_DeviceFinder.HeadsetTransform();
        }

        if (playerTransform == null)
        {
            playerTransform = VRTK_DeviceFinder.PlayAreaTransform();
        }

        objectOffset = new Vector3(variablePointer.PointerHit.normal.x * 1f, 0f, variablePointer.PointerHit.normal.z * 1f);

        headsetOffset = new Vector3(playerTransform.position.x - headsetTransform.position.x, 0f, playerTransform.position.z - headsetTransform.position.z);

        newTeleportPosition = variablePointer.PointerPosition + headsetOffset + objectOffset;

        yield return fadeDelay;

        if (Vector3.Distance(playerTransform.localPosition, newTeleportPosition) <= maxTeleportDistance)
        {
            playerTransform.localPosition = newTeleportPosition;
        }

        //playerTransform.localRotation = variablePointer.PointerEndGameObject.transform.rotation;
        headsetFadeScript.Unfade(0.25f);

        variablePointer.VariablePointerActive = false;
        variablePointer.SnapPointerState(PointerStates.Short);
        teleportEffectGameObject.SetActive(false);
        variablePointer.SetPointerColourDefault();
        variablePointer.SetPointerWidthDefault();
        variablePointer.SetPointerEndSizeDefault();

        bCurrentlyTeleporting = false;
    }

    // Use this for initialization
    void Start ()
    {
        playerTransform = VRTK_DeviceFinder.PlayAreaTransform();
        headsetTransform = VRTK_DeviceFinder.HeadsetTransform();
        headsetFadeScript = GetComponent<VRTK_HeadsetFade>();

        GameObject variableObject = GameObject.FindGameObjectWithTag("LeftVariable");

        if (variableObject)
        {
            variablePointer = (IPointer)variableObject.GetComponent(typeof(IPointer));
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (bActivationButtonPressed)
        {
            bTeleportActivated = true;

            teleportEffectGameObject.transform.position = variablePointer.PointerEndGameObject.transform.position;
            /*
            if (controllerEvents.GetTouchpadAxis().y > 0.5f)
            {
                
            }
            */
        }
        else
        {
            bTeleportActivated = false;
        }

        if (SCR_ToolOptions.instance.OptionsMenuOpen)
        {
            bTeleportActivated = false;
            bActivationButtonPressed = false;
            variablePointer.VariablePointerActive = false;
            variablePointer.SnapPointerState(PointerStates.Short);

            if (teleportEffectGameObject)
            {
                teleportEffectGameObject.SetActive(false);
            }

            variablePointer.SetPointerColourDefault();
            variablePointer.SetPointerWidthDefault();
            variablePointer.SetPointerEndSizeDefault();
            bTeleportActivated = false;
            bCurrentlyTeleporting = false;
        }
	}
}
