using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VRTK;

public enum AxisSelected { XAxis, YAxis, ZAxis, None };

public class SCR_AxisOption : MonoBehaviour, IToolOptionMenu, IAxisToolOption {

    public static SCR_AxisOption instance;

    [SerializeField] private VRTK_ControllerEvents controllerEvents;
    [SerializeField] private GameObject[] menuObjects;
    [SerializeField] private TextMeshProUGUI labelObject;
    [SerializeField] private TextMeshProUGUI axisText;
    [SerializeField] private GameObject defaultOption;

    [SerializeField] private AxisSelected currentAxis = AxisSelected.None;
    public AxisSelected CurrentAxis
    {
        get { return currentAxis; }
    }

    List<TextMeshProUGUI> menuTextObjects = new List<TextMeshProUGUI>();

    private bool bAllowAllAxisShortcut;

    public void SetCurrentAxis(AxisSelected newAxisSelected, GameObject referredObject)
    {
        currentAxis = newAxisSelected;

        SetAxisText();

        foreach (var item in menuObjects)
        {
            //item.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolOptions.instance.defaultOutlineColour;
            item.GetComponent<Renderer>().material.color = SCR_ToolOptions.instance.defaultObjectColour;
        }

        //referredObject.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolOptions.instance.selectedOutlineColour;
        referredObject.GetComponent<Renderer>().material.color = SCR_ToolOptions.instance.selectedObjectColour;
    }

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

        axisText.enabled = false;
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
            //item.GetComponent<SCR_Outline>().OutlineColor = tempItemOutlineColour;

            Color tempItemObjectColour = item.GetComponent<Renderer>().material.color;
            tempItemObjectColour.a = SCR_ToolOptions.instance.defaultObjectColour.a;
            item.GetComponent<Renderer>().material.color = tempItemObjectColour;
        }

        labelObject.color = Color.white;

        SetAxisText();

        axisText.enabled = true;
    }

    public void AllowAllAxis(bool bAllowAllAxis)
    {
        bAllowAllAxisShortcut = bAllowAllAxis;
    }

    void SetAxisText()
    {
        switch (currentAxis)
        {
            case AxisSelected.XAxis:
                axisText.text = "Axis: X";
                break;
            case AxisSelected.YAxis:
                axisText.text = "Axis: Y";
                break;
            case AxisSelected.ZAxis:
                axisText.text = "Axis: Z";
                break;
            case AxisSelected.None:
                axisText.text = "Axis: None";
                break;
            default:
                break;
        }
    }

    public void ShortcutSwapAxisOption(bool bLeft)
    {
        switch (currentAxis)
        {
            case AxisSelected.XAxis:
                if (bLeft)
                {
                    if (bAllowAllAxisShortcut)
                    {
                        SetCurrentAxis(AxisSelected.None, menuObjects[0]);
                    }
                    else
                    {
                        SetCurrentAxis(AxisSelected.ZAxis, menuObjects[3]);
                    }
                }
                else
                {
                    SetCurrentAxis(AxisSelected.YAxis, menuObjects[2]);
                }
                break;
            case AxisSelected.YAxis:
                if (bLeft)
                {
                    SetCurrentAxis(AxisSelected.XAxis, menuObjects[1]);
                }
                else
                {
                    SetCurrentAxis(AxisSelected.ZAxis, menuObjects[3]);
                }
                break;
            case AxisSelected.ZAxis:
                if (bLeft)
                {
                    SetCurrentAxis(AxisSelected.YAxis, menuObjects[2]);
                }
                else
                {
                    if (bAllowAllAxisShortcut)
                    {
                        SetCurrentAxis(AxisSelected.None, menuObjects[0]);
                    }
                    else
                    {
                        SetCurrentAxis(AxisSelected.XAxis, menuObjects[1]);
                    }
                }
                break;
            case AxisSelected.None:
                if (bLeft)
                {
                    SetCurrentAxis(AxisSelected.ZAxis, menuObjects[3]);
                }
                else
                {
                    SetCurrentAxis(AxisSelected.XAxis, menuObjects[1]);
                }
                break;
        }
    }

    // Use this for initialization
    void Start ()
    {
        foreach (var item in menuObjects)
        {
            TextMeshProUGUI textItem = item.GetComponentInChildren<TextMeshProUGUI>();
            if(textItem)
            {
                menuTextObjects.Add(textItem);
            }
        }

        //efaultOption.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolOptions.instance.selectedOutlineColour;
        defaultOption.GetComponent<Renderer>().material.color = SCR_ToolOptions.instance.selectedObjectColour;

    }

    void Awake()
    {
        instance = this;
    }
	
}
