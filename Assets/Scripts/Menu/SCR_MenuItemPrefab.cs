using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_MenuItemPrefab : MonoBehaviour, IToolMenuItem {

    [SerializeField] private GameObject toolToActivate;
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private TextMeshProUGUI descriptionTextObject;
    [SerializeField] private string descriptionText;

    [SerializeField] private bool bCloseMenuOnSelection = true;
    public bool CloseMenuOnSelection
    {
        get { return bCloseMenuOnSelection; }
    }

    public GameObject ToolToActivate
    {
        get { return toolToActivate; }
    }

    private bool bCurrentlySelected;

    public void OnSelected()
    {
        if (!toolToActivate.activeSelf)
        {
            gameObject.GetComponent<Renderer>().material.color = SCR_ToolMenuRadial.instance.selectedObjectColour;
            //gameObject.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.selectedOutlineColour;
            gameObject.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
            toolToActivate.GetComponent<SCR_AddPrefab>().PrefabToSpawn = prefabToSpawn;
            toolToActivate.SetActive(true);
            bCurrentlySelected = true;
        }
    }

    public void Deselected()
    {
        //if (toolToActivate.activeSelf)
        //{
            bCurrentlySelected = false;
            gameObject.GetComponent<Renderer>().material.color = SCR_ToolMenuRadial.instance.defaultObjectColour;
            //gameObject.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.defaultOutlineColour;
            toolToActivate.SetActive(false);
        //}
    }

    public void Highlighted()
    {
        GetComponent<Renderer>().material.color = SCR_ToolMenuRadial.instance.highlightedObjectColour;
        //GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.highlightedOutlineColour;
        descriptionTextObject.text = descriptionText;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    public void Unhighlighted()
    {
        if (bCurrentlySelected)
        {
            gameObject.GetComponent<Renderer>().material.color = SCR_ToolMenuRadial.instance.selectedObjectColour;
        }
        else
        {
            GetComponent<Renderer>().material.color = SCR_ToolMenuRadial.instance.defaultObjectColour;
        }
        
        //GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.defaultOutlineColour;
        descriptionTextObject.text = "Hover for description";
        transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
    }
}
