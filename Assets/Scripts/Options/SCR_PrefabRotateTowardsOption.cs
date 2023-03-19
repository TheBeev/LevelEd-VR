using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_PrefabRotateTowardsOption : MonoBehaviour, IToolOptionMenu, IOnOffToolOption
{

    public static SCR_PrefabRotateTowardsOption instance;

    [SerializeField] private GameObject[] menuObjects;
    [SerializeField] private TextMeshProUGUI labelObject;
    [SerializeField] private GameObject defaultOption;
    [SerializeField] private OptionActive prefabRotateTowardsActive = OptionActive.On;

    public OptionActive PrefabRotateTowardsActive
    {
        get { return prefabRotateTowardsActive; }
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
            Color tempItemObjectColour = item.GetComponent<Renderer>().material.color;
            tempItemObjectColour.a = 0.05f;
            item.GetComponent<Renderer>().material.color = tempItemObjectColour;
        }

        Color tempColour = labelObject.color;
        tempColour.a = 0.3f;
        labelObject.color = tempColour;

        //lockToFaceActive = OptionActive.Off;
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


        foreach (var item in menuObjects)
        {
            //Color tempItemOutlineColour = item.GetComponent<SCR_Outline>().OutlineColor;
            //tempItemOutlineColour.a = 1.0f;
            // item.GetComponent<SCR_Outline>().OutlineColor = tempItemOutlineColour;

            Color tempItemObjectColour = item.GetComponent<Renderer>().material.color;
            tempItemObjectColour.a = SCR_ToolOptions.instance.defaultObjectColour.a;
            item.GetComponent<Renderer>().material.color = tempItemObjectColour;
        }

        foreach (var item in menuObjects)
        {
            item.GetComponent<Renderer>().material.color = SCR_ToolOptions.instance.defaultObjectColour;
        }

        if (previousState == OptionActive.On)
        {
            menuObjects[0].GetComponent<Renderer>().material.color = SCR_ToolOptions.instance.selectedObjectColour;
        }
        else
        {
            menuObjects[1].GetComponent<Renderer>().material.color = SCR_ToolOptions.instance.selectedObjectColour;
        }

        prefabRotateTowardsActive = previousState;

        labelObject.color = Color.white;
    }

    public void ToggleStatus(OptionActive optionActive, GameObject referredObject)
    {
        prefabRotateTowardsActive = optionActive;
        previousState = optionActive;

        foreach (var item in menuObjects)
        {
            item.GetComponent<Renderer>().material.color = SCR_ToolOptions.instance.defaultObjectColour;
        }

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

        previousState = prefabRotateTowardsActive;

        //defaultOption.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolOptions.instance.selectedOutlineColour;
        defaultOption.GetComponent<Renderer>().material.color = SCR_ToolOptions.instance.selectedObjectColour;
    }

}
