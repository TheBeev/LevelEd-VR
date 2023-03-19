using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_MenuModePopout : MonoBehaviour, IToolMenuItem, IMenuPopout
{
    

    [SerializeField] private List<GameObject> popoutObjects = new List<GameObject>();
    [SerializeField] private GameObject defaultMenuItemObject;
    [SerializeField] private bool bModelIcon;
    [SerializeField] private MeshFilter iconMeshFilter;
    [SerializeField] private GameObject toolModeTextObject;
    [SerializeField] private GameObject toolDescriptionTextObject;
    [SerializeField] private TextMeshProUGUI labelText; //tool currently set for non-popout menu item

    [SerializeField] private bool bCloseMenuOnSelection = true;
    public bool CloseMenuOnSelection
    {
        get { return bCloseMenuOnSelection; }
    }

    private GameObject currentMenuModeObject; //tool currently set for non-popout menu item
    public GameObject ToolToActivate
    {
        get { return currentMenuModeObject; }
    }

    private GameObject popoutMenuItemSelected;
    private bool bPopoutActive;


    public void OnSelected()
    {
        //if (!currentMenuModeObject.activeSelf)
        //{
            //gameObject.GetComponent<Renderer>().material.color = SCR_ToolMenuRadial.instance.selectedObjectColour;
            //gameObject.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.selectedOutlineColour;
            //gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            //currentMenuModeObject.SetActive(true);
        //}
    }

    public void Deselected()
    {

        //gameObject.GetComponent<Renderer>().material.color = SCR_ToolMenuRadial.instance.defaultObjectColour;
        //gameObject.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.defaultOutlineColour;
        //currentMenuModeObject.SetActive(false);

    }

    public void PopoutSelected(GameObject newMenuModeObject, GameObject newPopoutMenuItemSelected)
    {
        popoutMenuItemSelected = newPopoutMenuItemSelected;

        currentMenuModeObject.SetActive(false);
        currentMenuModeObject = newMenuModeObject;
        currentMenuModeObject.SetActive(true);

        if (bModelIcon)
        {
            iconMeshFilter.mesh = popoutMenuItemSelected.GetComponent<IMenuPopoutItem>().ModelIcon;
        }
        else
        {
            labelText.text = popoutMenuItemSelected.GetComponent<IMenuPopoutItem>().OptionUIName;
        }

        //OnSelected();
        DeactivatePopout();
    }

    public void PopoutSelected(GameObject newMenuModeObject, GameObject newPopoutMenuItemSelected, Color newColour)
    {

    }

    void OnDisable()
    {
        DeactivatePopout();
    }

    public void ActivatePopout()
    {
        if (!bPopoutActive)
        {
            bPopoutActive = true;

            labelText.enabled = false;

            if (toolModeTextObject)
            {
                toolModeTextObject.SetActive(false);
            }

            if (toolDescriptionTextObject)
            {
                toolDescriptionTextObject.SetActive(false);
            }

            foreach (var item in popoutObjects)
            {
                item.SetActive(true);
            }
        }
    }

    public void DeactivatePopout()
    {
        bPopoutActive = false;

        labelText.enabled = true;

        if (toolModeTextObject)
        {
            toolModeTextObject.SetActive(true);
        }

        if (toolDescriptionTextObject)
        {
            toolDescriptionTextObject.SetActive(true);
        }

        foreach (var item in popoutObjects)
        {
            item.SetActive(false);
        }

    }

    // Use this for initialization
    void Start()
    {

        if (bModelIcon)
        {
            iconMeshFilter.mesh = defaultMenuItemObject.GetComponent<IMenuPopoutItem>().ModelIcon;
        }
        else
        {
            labelText.text = defaultMenuItemObject.GetComponent<IMenuPopoutItem>().OptionUIName;
        }
        

        currentMenuModeObject = defaultMenuItemObject.GetComponent<IMenuPopoutItem>().ToolToActivate;
    }

    public void Highlighted()
    {
        GetComponent<Renderer>().material.color = SCR_ToolMenuRadial.instance.highlightedObjectColour;
        //GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.highlightedOutlineColour;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    public void Unhighlighted()
    {
        GetComponent<Renderer>().material.color = SCR_ToolMenuRadial.instance.defaultObjectColour;
        //GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.defaultOutlineColour;
        transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
    }

}
