using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PrefabData
{
    public Color startingMaterialColour;
    public Color startingEmissionColour;
    public MeshRenderer individualObjectRenderer;
}

public class SCR_PrefabData : MonoBehaviour {

    [SerializeField] private List<GameObject> listOfChildren = new List<GameObject>();
    public int prefabID = 0;

    private List<PrefabData> currentPrefabData = new List<PrefabData>();

	// Use this for initialization
	void Start ()
    {
        for (int i = 0; i < listOfChildren.Count; i++)
        {
            if (listOfChildren[i].GetComponent<MeshFilter>() != null)
            {
                PrefabData newPrefabData = new PrefabData();
                newPrefabData.individualObjectRenderer = listOfChildren[i].GetComponent<MeshRenderer>();
                newPrefabData.startingMaterialColour = newPrefabData.individualObjectRenderer.material.color;
                newPrefabData.startingEmissionColour = newPrefabData.individualObjectRenderer.material.GetColor("_EmissionColor");

                currentPrefabData.Add(newPrefabData);
            }
        }
	}

    public void CurrentlySelected()
    {
        for (int i = 0; i < currentPrefabData.Count; i++)
        {
            currentPrefabData[i].individualObjectRenderer.material.color = Color.red;
            currentPrefabData[i].individualObjectRenderer.material.SetColor("_EmissionColor", Color.red);
            currentPrefabData[i].individualObjectRenderer.gameObject.layer = 2;
        }
    }

    public void Deselected()
    {
        for (int i = 0; i < currentPrefabData.Count; i++)
        {
            currentPrefabData[i].individualObjectRenderer.material.color = currentPrefabData[i].startingMaterialColour;
            currentPrefabData[i].individualObjectRenderer.material.SetColor("_EmissionColor", currentPrefabData[i].startingEmissionColour);
            currentPrefabData[i].individualObjectRenderer.gameObject.layer = 8;
        }
    }

    public void CurrentlyHighlighted()
    {
        for (int i = 0; i < currentPrefabData.Count; i++)
        {
            currentPrefabData[i].individualObjectRenderer.material.color = SCR_ToolMenuRadial.instance.highlightedGameObjectColour;
            currentPrefabData[i].individualObjectRenderer.material.SetColor("_EmissionColor", SCR_ToolMenuRadial.instance.highlightedGameObjectColour);
        }
    }

    public void StopHighlighting()
    {
        for (int i = 0; i < currentPrefabData.Count; i++)
        {
            currentPrefabData[i].individualObjectRenderer.material.color = currentPrefabData[i].startingMaterialColour;
            currentPrefabData[i].individualObjectRenderer.material.SetColor("_EmissionColor", currentPrefabData[i].startingEmissionColour);
        }
    }

    public void CurrentlyGuiding(Material newMaterial, bool bChangeMaterial)
    {
        Start();

        for (int i = 0; i < currentPrefabData.Count; i++)
        {
            if (bChangeMaterial)
            {
                currentPrefabData[i].individualObjectRenderer.material = newMaterial;
            }
            
            gameObject.layer = 0; 
        }

        Transform[] listOfChildTransforms = GetComponentsInChildren<Transform>();

        foreach (Transform item in listOfChildTransforms)
        {
            item.gameObject.layer = 0;
        }
    }

    public void StopGuiding()
    {
        gameObject.layer = 8;

        Transform[] listOfChildTransforms = GetComponentsInChildren<Transform>();

        foreach (Transform item in listOfChildTransforms)
        {
            item.gameObject.layer = 8;
        }
    }

}
