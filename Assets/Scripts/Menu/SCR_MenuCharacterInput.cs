using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterMenuState
{
    Character,
    Done
}

public class SCR_MenuCharacterInput : MonoBehaviour, ICharacterMenuItem, IHighlightMenuItem
{
    [SerializeField] private CharacterMenuState characterOrSpecial = CharacterMenuState.Character;
    public CharacterMenuState CharacterOrSpecial
    {
        get { return characterOrSpecial; }
    }

    [SerializeField] private string characterValue;
    public string CharacterValue
    {
        get { return characterValue; }
    }

    public void Highlighted()
    {
        GetComponent<Renderer>().material.color = SCR_ToolMenuRadial.instance.highlightedObjectColour;
        //GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.highlightedOutlineColour;
        transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
    }

    public void Unhighlighted()
    {
        GetComponent<Renderer>().material.color = SCR_ToolMenuRadial.instance.defaultObjectColour;
        //GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.defaultOutlineColour;
        transform.localScale = new Vector3(0.075f, 0.075f, 0.075f);
    }

    public void Selected()
    {
        //quick highlight
    }
}
