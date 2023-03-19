﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class SSScoreManagerData : ParentScriptData
{
    public int[] listOfInputNodeIDs;
}

public class SCR_ScoreboardManager : MonoBehaviour, ILevelItemInteractive, ISSAcceptsInt, IScriptable, IScriptableData<ParentScriptData>
{
    public static SCR_ScoreboardManager instance;

    [SerializeField] private float timerStartMins = 1f;
    [SerializeField] private float timerStartSeconds = 30f;
    [SerializeField] private GameObject startingTeleport = null;
    [SerializeField] private GameObject[] listOfInputNodes;

    [SerializeField] private MeshRenderer[] renderersToDisableOnPlay;
    [SerializeField] private Collider[] collidersToDisableOnPlay;

    [SerializeField] private Transform lineEndTransform;
    public Transform LineEndTransform
    {
        get { return lineEndTransform; }
    }

    private List<TextMeshPro> targetsLeftTextList = new List<TextMeshPro>();
    private List<TextMeshPro> timerLeftTextList = new List<TextMeshPro>();
    private List<TextMeshPro> winConditionTextList = new List<TextMeshPro>();
    private List<SCR_Teleporter> listOfTeleporterScripts = new List<SCR_Teleporter>();

    private int targetsLeft;
    private int scoreBoardsInScene;
    private bool bGameOver = true;
    private float timerMins;
    private float timerSeconds;
    private string timerMinsZeroString;
    private string timerSecondsZeroString;

    private bool bAddedToInteractiveList;

    void Awake()
    {
        
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        

        //instance = this;
    }

    // Use this for initialization
    void Start()
    {
        timerMins = timerStartMins;
        timerSeconds = timerStartSeconds;
        print("Added to list bool: " + bAddedToInteractiveList);

        if (!bAddedToInteractiveList)
        {
            SCR_LevelEditorManager.instance.AddPrefabsToInteractive(this.gameObject);
            bAddedToInteractiveList = true;
        }

        print("Added to list bool: " + bAddedToInteractiveList);
    }

    void OnDestroy()
    {
        SCR_LevelEditorManager.instance.RemovePrefabsFromInteractive(this.gameObject);
        SCR_LevelEditorManager.instance.RemoveScriptablesToToggleVisibilty(this as IScriptable);
        SCR_ScoreboardManager comp = gameObject.GetComponent<SCR_ScoreboardManager>();
        UnityEngine.Object.Destroy(comp);
        //instance = null;
    }

    public void Visible(bool toggleOn)
    {
        if (toggleOn)
        {
            if (renderersToDisableOnPlay.Length > 0)
            {
                for (int i = 0; i < renderersToDisableOnPlay.Length; i++)
                {
                    renderersToDisableOnPlay[i].enabled = true;
                }
            }

            if (collidersToDisableOnPlay.Length > 0)
            {
                for (int i = 0; i < collidersToDisableOnPlay.Length; i++)
                {
                    collidersToDisableOnPlay[i].enabled = true;
                }
            }
        }
        else
        {
            if (renderersToDisableOnPlay.Length > 0)
            {
                for (int i = 0; i < renderersToDisableOnPlay.Length; i++)
                {
                    renderersToDisableOnPlay[i].enabled = false;
                }
            }

            if (collidersToDisableOnPlay.Length > 0)
            {
                for (int i = 0; i < collidersToDisableOnPlay.Length; i++)
                {
                    collidersToDisableOnPlay[i].enabled = false;
                }
            }
        }
    }

    public void Enable()
    {
        Start();
        bGameOver = false;

        SCR_LevelEditorManager.instance.AddScriptablesToToggleVisibilty(this as IScriptable);

        if (startingTeleport)
        {
            listOfTeleporterScripts.Add(startingTeleport.GetComponent<SCR_Teleporter>());
        }
    }

    public void Disable()
    {
        Reset();
    }

    void Reset()
    {
        timerMins = timerStartMins;
        timerSeconds = timerStartSeconds;
        bGameOver = true;

        SCR_LevelEditorManager.instance.RemoveScriptablesToToggleVisibilty(this as IScriptable);

        if (scoreBoardsInScene == 1)
        {
            timerLeftTextList[0].text = "Total Time: " + timerMinsZeroString + timerMins.ToString("F0") + ":" + timerSecondsZeroString + timerSeconds.ToString("F2");
        }
        else if (scoreBoardsInScene > 1)
        {
            for (int i = 0; i < timerLeftTextList.Count; i++)
            {
                timerLeftTextList[i].text = "Total Time: " + timerMinsZeroString + timerMins.ToString("F0") + ":" + timerSecondsZeroString + timerSeconds.ToString("F2");
            }
        }

        if (scoreBoardsInScene == 1)
        {
            targetsLeftTextList[0].text = "Targets Left: 0";
        }
        else if (scoreBoardsInScene > 1)
        {
            for (int i = 0; i < timerLeftTextList.Count; i++)
            {
                targetsLeftTextList[i].text = "Targets Left: 0";
            }
        }

        if (scoreBoardsInScene == 1)
        {
            winConditionTextList[0].text = "Go!";
        }
        else if (scoreBoardsInScene > 1)
        {
            for (int i = 0; i < timerLeftTextList.Count; i++)
            {
                winConditionTextList[i].text = "Go!";
            }
        }

        targetsLeft = 0;
        scoreBoardsInScene = 0;

        targetsLeftTextList.Clear();
        timerLeftTextList.Clear();
        winConditionTextList.Clear();
        listOfTeleporterScripts.Clear();
    }

    public void ConfigureNewScriptable()
    {
        if (listOfInputNodes.Length > 0)
        {
            for (int i = 0; i < listOfInputNodes.Length; i++)
            {
                SCR_SaveSystem.instance.ScriptIDNumber++;
                listOfInputNodes[i].name = "IONode_" + SCR_SaveSystem.instance.ScriptIDNumber;
                listOfInputNodes[i].GetComponent<IInputNode>().InputNodeID = SCR_SaveSystem.instance.ScriptIDNumber;
            }   
        }
    }

    public void ReconfigureScriptableNode(ParentScriptData newData)
    {
        SSScoreManagerData newScoreManagerData = newData as SSScoreManagerData;

        if (listOfInputNodes.Length > 0)
        {
            for (int i = 0; i < listOfInputNodes.Length; i++)
            {
                IInputNode inputNodeScript = listOfInputNodes[i].GetComponent<IInputNode>();
                inputNodeScript.InputNodeID = newScoreManagerData.listOfInputNodeIDs[i];
                inputNodeScript.SetUpInput();
            }
        }
    }

    public void ReconfigureOutputNodes()
    {
        //no nodes
    }

    public void StartDataFlow()
    {
        //no data to pass on
    }
    
    public void ResetNode()
    {
        //no output nodes to reset
    }

    public ParentScriptData GetData()
    {
        SSScoreManagerData newScoreManagerData = new SSScoreManagerData();

        if (listOfInputNodes.Length > 0)
        {
            newScoreManagerData.listOfInputNodeIDs = new int[listOfInputNodes.Length];
            for (int i = 0; i < listOfInputNodes.Length; i++)
            {
                newScoreManagerData.listOfInputNodeIDs[i] = listOfInputNodes[i].GetComponent<IInputNode>().InputNodeID;
            }
        }

        return newScoreManagerData as ParentScriptData;
    }

    public void AcceptInt(int variableOrder, int incomingVariable)
    {
        switch (variableOrder)
        {
            case 0:
                {
                    timerStartMins = incomingVariable;
                    timerMins = timerStartMins;
                    break;
                }
            case 1:
                {
                    timerStartSeconds = incomingVariable;
                    timerSeconds = timerStartSeconds;
                    break;
                }
            default:
                break;
        }

    }

    public void RegisterTarget()
    {
        targetsLeft++;

        if (scoreBoardsInScene == 1)
        {
            targetsLeftTextList[0].text = "Targets Left: " + targetsLeft.ToString();
        }
        else if (scoreBoardsInScene > 1)
        {
            for (int i = 0; i < timerLeftTextList.Count; i++)
            {
                targetsLeftTextList[i].text = "Targets Left: " + targetsLeft.ToString();
            }
        }
    }

    public void TargetDestroyed()
    {
        targetsLeft--;

        if (scoreBoardsInScene == 1)
        {
            targetsLeftTextList[0].text = "Targets Left: " + targetsLeft.ToString();
        }
        else if(scoreBoardsInScene > 1)
        {
            for (int i = 0; i < timerLeftTextList.Count; i++)
            {
                targetsLeftTextList[i].text = "Targets Left: " + targetsLeft.ToString();
            }
        }

        if (targetsLeft <= 0)
        {
            CheckWinCondition();
        }
    }

    public void RegisterScoreboard(TextMeshPro targetsLeftText, TextMeshPro timerLeftText, TextMeshPro winConditionText)
    {
        targetsLeftTextList.Add(targetsLeftText);
        timerLeftTextList.Add(timerLeftText);
        winConditionTextList.Add(winConditionText);

        scoreBoardsInScene++;

        if (scoreBoardsInScene == 1 && targetsLeft > 0)
        {
            targetsLeftTextList[0].text = "Targets Left: " + targetsLeft.ToString();
        }
        else if (scoreBoardsInScene > 1 && targetsLeft > 0)
        {
            for (int i = 0; i < timerLeftTextList.Count; i++)
            {
                targetsLeftTextList[i].text = "Targets Left: " + targetsLeft.ToString();
            }
        }
    }

    void CheckWinCondition()
    {
        if (targetsLeft <= 0 && !bGameOver)
        {
            if (scoreBoardsInScene == 1)
            {
                winConditionTextList[0].text = "You Win!";
            }
            else if (scoreBoardsInScene > 1)
            {
                for (int i = 0; i < timerLeftTextList.Count; i++)
                {
                    winConditionTextList[i].text = "You Win!";
                }
            }
        }
        else
        {
            if (scoreBoardsInScene == 1)
            {
                winConditionTextList[0].text = "You Lose!";
            }
            else if (scoreBoardsInScene > 1)
            {
                for (int i = 0; i < timerLeftTextList.Count; i++)
                {
                    winConditionTextList[i].text = "You Lose!";
                }
            }
        }

        bGameOver = true;
    }


    /*
    public bool RegisterStartingTeleporter(GameObject newStartingTeleporter)
    {
        if (startingTeleport == null)
        {
            startingTeleport = newStartingTeleporter;
            listOfTeleporterScripts.Add(newStartingTeleporter.GetComponent<SCR_Teleporter>());
            return true;
        }
        else
        {
            listOfTeleporterScripts.Add(newStartingTeleporter.GetComponent<SCR_Teleporter>());
            return false;
        } 
    }
    */

    public void RegisterTeleporter(GameObject newTeleporter)
    {
        listOfTeleporterScripts.Add(newTeleporter.GetComponent<SCR_Teleporter>());
    }

    public void EnableTeleporterEffects()
    {
        foreach (var item in listOfTeleporterScripts)
        {
            item.TeleporterEffectEnabled(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!bGameOver)
        {

            timerSeconds -= Time.deltaTime;

            if (timerSeconds <= 0 && timerMins >= 1)
            {
                timerMins--;
                timerSeconds = 60;
            }

            if (timerMins <= 0 && timerSeconds <= 0)
            {
                bGameOver = true;
                CheckWinCondition();
            }

            if (timerMins < 10)
            {
                timerMinsZeroString = "0";
            }
            else
            {
                timerMinsZeroString = "";
            }

            if (timerSeconds < 10)
            {
                timerSecondsZeroString = "0";
            }
            else
            {
                timerSecondsZeroString = "";
            }

            if (scoreBoardsInScene == 1)
            {
                timerLeftTextList[0].text = "Total Time: " + timerMinsZeroString + timerMins.ToString("F0") + ":" + timerSecondsZeroString + timerSeconds.ToString("F2");
            }
            else if(scoreBoardsInScene > 1)
            {
                for (int i = 0; i < timerLeftTextList.Count; i++)
                {
                    timerLeftTextList[i].text = "Total Time: " + timerMinsZeroString + timerMins.ToString("F0") + ":" + timerSecondsZeroString + timerSeconds.ToString("F2");
                }
            }

        }
    }
}
