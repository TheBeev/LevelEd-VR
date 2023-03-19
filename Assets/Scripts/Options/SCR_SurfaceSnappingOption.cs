using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_SurfaceSnappingOption : MonoBehaviour, IToolOptionMenu, IOnOffToolOption
{
    public static SCR_SurfaceSnappingOption instance;

    [SerializeField] private GameObject[] menuObjects;
    [SerializeField] private TextMeshProUGUI labelObject;
    [SerializeField] private GameObject defaultOption;
    [SerializeField] private OptionActive surfaceSnappingActive = OptionActive.On;

    public OptionActive SurfaceSnappingActive
    {
        get { return surfaceSnappingActive; }
    }

    private OptionActive previousState;

    List<TextMeshProUGUI> menuTextObjects = new List<TextMeshProUGUI>();

    public void DeactivateOption()
    {
        foreach (var item in menuObjects)
        {
            item.layer = 2;
        }

        foreach (var item in menuTextObjects)
        {
            Color tempItemColour = item.color;
            tempItemColour.a = 0.3f;
            item.color = tempItemColour;
        }

        foreach (var item in menuObjects)
        {
            //Color tempItemOutlineColour = item.GetComponent<SCR_Outline>().OutlineColor;
            //tempItemOutlineColour.a = 0.1f;
            //item.GetComponent<SCR_Outline>().OutlineColor = tempItemOutlineColour;

            Color tempItemObjectColour = item.GetComponent<Renderer>().material.color;
            tempItemObjectColour.a = 0.05f;
            item.GetComponent<Renderer>().material.color = tempItemObjectColour;
        }

        Color tempColour = labelObject.color;
        tempColour.a = 0.3f;
        labelObject.color = tempColour;

        surfaceSnappingActive = OptionActive.Off;
    }

    public void ActivateOption()
    {
        foreach (var item in menuObjects)
        {
            item.layer = 11;
        }

        foreach (var item in menuTextObjects)
        {
            item.color = Color.white;
        }

        /*
        foreach (var item in menuObjects)
        {
            Color tempItemOutlineColour = item.GetComponent<SCR_Outline>().OutlineColor;
            tempItemOutlineColour.a = 1.0f;
            item.GetComponent<SCR_Outline>().OutlineColor = tempItemOutlineColour;

            Color tempItemObjectColour = item.GetComponent<Renderer>().material.color;
            tempItemObjectColour.a = SCR_ToolOptions.instance.defaultObjectColour.a;
            item.GetComponent<Renderer>().material.color = tempItemObjectColour;
        }*/

        foreach (var item in menuObjects)
        {
            //item.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolOptions.instance.defaultOutlineColour;
            item.GetComponent<Renderer>().material.color = SCR_ToolOptions.instance.defaultObjectColour;
        }

        if (previousState == OptionActive.On)
        {
            //menuObjects[0].GetComponent<SCR_Outline>().OutlineColor = SCR_ToolOptions.instance.selectedOutlineColour;
            menuObjects[0].GetComponent<Renderer>().material.color = SCR_ToolOptions.instance.selectedObjectColour;
        }
        else
        {
            //menuObjects[1].GetComponent<SCR_Outline>().OutlineColor = SCR_ToolOptions.instance.selectedOutlineColour;
            menuObjects[1].GetComponent<Renderer>().material.color = SCR_ToolOptions.instance.selectedObjectColour;
        }

        surfaceSnappingActive = previousState;

        labelObject.color = Color.white;
    }

    public void ToggleStatus(OptionActive optionActive, GameObject referredObject)
    {
        surfaceSnappingActive = optionActive;
        previousState = optionActive;

        foreach (var item in menuObjects)
        {
            //item.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolOptions.instance.defaultOutlineColour;
            item.GetComponent<Renderer>().material.color = SCR_ToolOptions.instance.defaultObjectColour;
        }

        //referredObject.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolOptions.instance.selectedOutlineColour;
        referredObject.GetComponent<Renderer>().material.color = SCR_ToolOptions.instance.selectedObjectColour;
    }

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        foreach (var item in menuObjects)
        {
            TextMeshProUGUI textItem = item.GetComponentInChildren<TextMeshProUGUI>();
            if (textItem)
            {
                menuTextObjects.Add(textItem);
            }
        }

        previousState = surfaceSnappingActive;

        //defaultOption.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolOptions.instance.selectedOutlineColour;
        //defaultOption.GetComponent<Renderer>().material.color = SCR_ToolOptions.instance.selectedObjectColour;
    }
}
