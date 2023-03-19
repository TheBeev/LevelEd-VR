﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class SCR_ToolMenu : MonoBehaviour {

    public static SCR_ToolMenu instance;

    [SerializeField] private VRTK_ControllerEvents.ButtonAlias activationButton = VRTK_ControllerEvents.ButtonAlias.ButtonOnePress;
    [SerializeField] private VRTK_ControllerEvents controllerEvents;
    [SerializeField] private GameObject toolMenuUI;
    [SerializeField] private Transform headsetCentre;

    private List<Collider> objectsHiddenByMenu = new List<Collider>();

    private GameObject currentTool;
    //private RectTransform toolMenuRectTransform;
    //private Vector3 overlapBoxExtents;
    private Vector3 centre;
    private int geometryLayer;
    private bool bMenuOpen;

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        
        controllerEvents.SubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
        controllerEvents.SubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonReleased);
    }

    private void OnDisable()
    {
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, false, DoActivationButtonReleased);
    }

    void DoActivationButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (!currentTool.GetComponent<ITool>().Busy)
        {
            toolMenuUI.transform.position = headsetCentre.position + (headsetCentre.transform.forward * 1f);
            centre = (headsetCentre.position + toolMenuUI.transform.position) / 2f;
            Collider[] colliders = Physics.OverlapBox(centre, new Vector3(0.25f, 0.25f, 0.5f), toolMenuUI.transform.rotation, geometryLayer);
            objectsHiddenByMenu.AddRange(colliders);
            foreach (var item in objectsHiddenByMenu)
            {
                item.gameObject.SetActive(false);
            }
            toolMenuUI.SetActive(true);
            bMenuOpen = true;
        }
    }

    void DoActivationButtonReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (bMenuOpen)
        {
            toolMenuUI.SetActive(false);
            foreach (var item in objectsHiddenByMenu)
            {
                item.gameObject.SetActive(true);
            }
            objectsHiddenByMenu.Clear();
            bMenuOpen = false;
        }  
    }

    public void ToolChanged(GameObject newTool)
    {
        if (currentTool)
        {
            currentTool.SetActive(false);
        }
        currentTool = newTool;
    }

	// Use this for initialization
	void Start ()
    {
        geometryLayer = 1 << 8;
        //toolMenuRectTransform = toolMenuUI.GetComponent<RectTransform>();
        //overlapBoxExtents = new Vector3((toolMenuRectTransform.rect.width / toolMenuRectTransform.localScale.x) * 0.5f, (toolMenuRectTransform.rect.height / toolMenuRectTransform.localScale.y) * 0.5f, 0.5f);
	}

    // Update is called once per frame
    void Update ()
    {
		
	}

}
