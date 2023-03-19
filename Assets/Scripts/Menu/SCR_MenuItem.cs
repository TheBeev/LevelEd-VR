using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_MenuItem : MonoBehaviour, IToolMenuItem {

    [SerializeField] private GameObject toolToActivate;
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
    private MaterialPropertyBlock propertyBlock;
    private Renderer currentRend;

    void Start()
    {
        currentRend = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    public void OnSelected()
    {
        if (!toolToActivate.activeSelf)
        {
            if (!currentRend)
            {
                currentRend = GetComponent<Renderer>();
            }

            currentRend.material.color = SCR_ToolMenuRadial.instance.selectedObjectColour;
            //gameObject.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.selectedOutlineColour;
            gameObject.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
            bCurrentlySelected = true;
            toolToActivate.SetActive(true);
        }
    }

    public void Deselected()
    {
        bCurrentlySelected = false;

        currentRend.material.color = SCR_ToolMenuRadial.instance.defaultObjectColour;
        //gameObject.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.defaultOutlineColour;
        toolToActivate.SetActive(false);
    }

    public void Highlighted()
    {
        GetComponent<Renderer>().material.color = SCR_ToolMenuRadial.instance.highlightedObjectColour;

        //currentRend.GetPropertyBlock(propertyBlock);
        //propertyBlock.SetColor("_Color", SCR_ToolMenuRadial.instance.highlightedObjectColour);
        //currentRend.SetPropertyBlock(propertyBlock);

        //GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.highlightedOutlineColour;
        descriptionTextObject.text = descriptionText;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    public void Unhighlighted()
    {

        if (bCurrentlySelected)
        {
            currentRend.material.color = SCR_ToolMenuRadial.instance.selectedObjectColour;
            //currentRend.GetPropertyBlock(propertyBlock);
            //propertyBlock.SetColor("_Color", SCR_ToolMenuRadial.instance.selectedObjectColour);
            //currentRend.SetPropertyBlock(propertyBlock);
        }
        else
        {
            currentRend.material.color = SCR_ToolMenuRadial.instance.defaultObjectColour;
            //currentRend.GetPropertyBlock(propertyBlock);
            //propertyBlock.SetColor("_Color", SCR_ToolMenuRadial.instance.defaultObjectColour);
            //currentRend.SetPropertyBlock(propertyBlock);
        }
        
        //GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.defaultOutlineColour;
        descriptionTextObject.text = "Hover for description";
        transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
    }
}
