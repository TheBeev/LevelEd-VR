using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMenuPopout
{
    void ActivatePopout();
    void DeactivatePopout();
    void PopoutSelected(GameObject newToolObject, GameObject popoutItemSelected);
    void PopoutSelected(GameObject newToolObject, GameObject popoutItemSelected, Color newColourToUse);
}
