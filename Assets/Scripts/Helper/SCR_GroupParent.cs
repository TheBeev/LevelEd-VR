using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GroupData
{
    public Color startingMaterialColour;
    public Color startingEmissionColour;
    public MeshRenderer individualObjectRenderer;
}

public class SCR_GroupParent : MonoBehaviour {

    public List<GameObject> groupedObjectList = new List<GameObject>();
    public int ID;
    public bool bMaterialsCached;

    public List<GroupData> currentGroupData = new List<GroupData>();

    public void CheckMaterialCache()
    {
        if (!bMaterialsCached)
        {
            for (int i = 0; i < groupedObjectList.Count; i++)
            {
                if (groupedObjectList[i].GetComponent<MeshFilter>() != null)
                {
                    GroupData newGroupData = new GroupData();
                    newGroupData.individualObjectRenderer = groupedObjectList[i].GetComponent<MeshRenderer>();
                    newGroupData.startingMaterialColour = newGroupData.individualObjectRenderer.material.color;
                    newGroupData.startingEmissionColour = newGroupData.individualObjectRenderer.material.GetColor("_EmissionColor");

                    currentGroupData.Add(newGroupData);
                }
            }

            bMaterialsCached = true;
        }
    }

    public void UpdateCachedMaterials()
    {
        if (!bMaterialsCached)
        {
            CheckMaterialCache();
        }

        for (int i = 0; i < currentGroupData.Count; i++)
        {
            currentGroupData[i].startingMaterialColour = currentGroupData[i].individualObjectRenderer.material.color;
            currentGroupData[i].startingEmissionColour = currentGroupData[i].individualObjectRenderer.material.GetColor("_EmissionColor");
        }
    }

    public void Reset()
    {
        bMaterialsCached = false;
        CheckMaterialCache();
    }

    public void CurrentlySelected()
    {
        for (int i = 0; i < currentGroupData.Count; i++)
        {
            currentGroupData[i].individualObjectRenderer.material.color = Color.red;
            currentGroupData[i].individualObjectRenderer.material.SetColor("_EmissionColor", Color.red);
            currentGroupData[i].individualObjectRenderer.gameObject.layer = 2;
        }
    }

    public void Deselected()
    {
        for (int i = 0; i < currentGroupData.Count; i++)
        {
            currentGroupData[i].individualObjectRenderer.material.color = currentGroupData[i].startingMaterialColour;
            currentGroupData[i].individualObjectRenderer.material.SetColor("_EmissionColor", currentGroupData[i].startingEmissionColour);
            currentGroupData[i].individualObjectRenderer.gameObject.layer = 8;
        }
    }

    public void CurrentlyHighlighted()
    {
        for (int i = 0; i < currentGroupData.Count; i++)
        {
            currentGroupData[i].individualObjectRenderer.material.color = SCR_ToolMenuRadial.instance.highlightedGameObjectColour;
            currentGroupData[i].individualObjectRenderer.material.SetColor("_EmissionColor", SCR_ToolMenuRadial.instance.highlightedGameObjectColour);
        }
    }

    public void StopHighlighting()
    {
        for (int i = 0; i < currentGroupData.Count; i++)
        {
            currentGroupData[i].individualObjectRenderer.material.color = currentGroupData[i].startingMaterialColour;
            currentGroupData[i].individualObjectRenderer.material.SetColor("_EmissionColor", currentGroupData[i].startingEmissionColour);
        }
    }

}
