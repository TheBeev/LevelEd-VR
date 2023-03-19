using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_AxisOptionItem : MonoBehaviour, IToolOptionMenuItem {

    [SerializeField] private GameObject axisOptionMenuObject;
    [SerializeField] private AxisSelected axisOption;

    private IAxisToolOption axisOptionMenu;

    void Start()
    {
        axisOptionMenu = axisOptionMenuObject.GetComponent<IAxisToolOption>();
    }

    public void Selected()
    {
        axisOptionMenu.SetCurrentAxis(axisOption, gameObject);
    }

}
