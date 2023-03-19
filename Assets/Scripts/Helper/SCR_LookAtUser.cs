using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_LookAtUser : MonoBehaviour {

    [SerializeField] private Transform headsetCentre;
    [SerializeField] private bool bOnlyOnEnabled;
    [SerializeField] private bool bRestrictZRotation;

    private void OnEnable()
    {
        transform.LookAt(headsetCentre);

        if (bRestrictZRotation)
        {
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0);
        }
    }

    void LateUpdate ()
    {
        if (!bOnlyOnEnabled)
        {
            transform.LookAt(headsetCentre);
        }        
    }
}
