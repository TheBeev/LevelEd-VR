using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_OnOffOptionItem : MonoBehaviour, IToolOptionMenuItem
{
    [SerializeField] private GameObject optionMenuObject;
    [SerializeField] private OptionActive optionActive;

    private IOnOffToolOption optionMenu;

    void Start()
    {
        optionMenu = optionMenuObject.GetComponent<IOnOffToolOption>();
    }

    public void Selected()
    {
        optionMenu.ToggleStatus(optionActive, gameObject);
    }
}
