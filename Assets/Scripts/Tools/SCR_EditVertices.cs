using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class VertexGroups
{
    public List<Vector3> vertexPosition = new List<Vector3>();
    public List<int> vertexIndex = new List<int>();
}

public class SCR_EditVertices : MonoBehaviour, ITool {

    public static SCR_EditVertices instance;

    private enum ToolStates { SelectingObject, ChoosingVertex, MovingVertex };
    private ToolStates currentState = ToolStates.SelectingObject;

    [SerializeField] private VRTK_ControllerEvents.ButtonAlias activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
    [SerializeField] private VRTK_ControllerEvents controllerEvents;
    [SerializeField] private string toolName;
    [SerializeField] private Material mat;
    [SerializeField] private GameObject vertexHelperPrefab;
    [SerializeField] private float vertexHelperStartingScale;
    [SerializeField] private float vertexHelperMaxScale;
    [SerializeField] private float vertexHelperScaleMultiplier;
    [SerializeField] private bool bScaleBasedOnIndividualVertexHelper;
    [SerializeField] private Color meshEditingColour = Color.red;
    [SerializeField] private Color vertexHelperColour = Color.red;
    [SerializeField] private Color vertexHelperSelectedColour = Color.blue;
    [SerializeField] private Color vertexHelperHighlightedColour = Color.blue;

    bool bBusy;
    public bool Busy
    {
        get { return bBusy; }
    }

    private VRTK_ControllerReference controllerReference;
    private bool bActivationButtonPressed;
    private IPointer variablePointer;

    private GameObject selectedObject;
    private GameObject selectedHelperObject;
    private GameObject highlightedHelperObject;
    private SCR_VertexHelper selectedHelperScript;
    private Color objectStartColor;
    private MeshFilter currentMeshFilter;
    private Mesh currentMesh;
    private MeshCollider currentMeshCollider;
    [SerializeField] private float vertexHelperCurrentScale;
    private Transform headsetTransform;

    //list of vertices from the mesh
    private List<Vector3> vertexList = new List<Vector3>();

    //list of indexes of vertices that have already been grouped
    private List<int> vertexIndexMatched = new List<int>();

    //list of vertex groups that include their positions and their index.
    //this is used to move all the vertices at once.
    private List<VertexGroups> vertexGroupList = new List<VertexGroups>();

    [SerializeField] private List<GameObject> vertexHelperList = new List<GameObject>();

    private Vector3[] verticesOfMesh;

    private Vector3 startLocation;
    private Vector3 offsetLocation;

    private void OnEnable()
    {
        SCR_ToolMenuRadial.instance.ToolChanged(gameObject, toolName);

        SCR_ToolOptions.instance.DeactivateOptions();
        SCR_GridSnappingOption.instance.ActivateOption();
        SCR_SurfaceSnappingOption.instance.ActivateOption();

        headsetTransform = VRTK_DeviceFinder.HeadsetTransform();

        if (variablePointer != null)
        {
            variablePointer.HighlightingActive = true;
        }
        else
        {
            Start();
            variablePointer.HighlightingActive = true;
        }

        controllerEvents.SubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
    }

    private void OnDisable()
    {
        bActivationButtonPressed = false;
        variablePointer.HighlightingActive = false;
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
    }

    void DoActivationButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.LevelEditor)
        {
            if (controllerReference == null)
            {
                controllerReference = e.controllerReference;
            }

            if (!bActivationButtonPressed && !SCR_ToolMenuRadial.instance.Busy)
            {
                bActivationButtonPressed = true;
            }
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        GameObject variableObject = GameObject.FindGameObjectWithTag("RightVariable");

        if (variableObject)
        {
            variablePointer = (IPointer)variableObject.GetComponent(typeof(IPointer));
        }
    }

    // Update is called once per frame
    void Update ()
    {
        switch (currentState)
        {
            case ToolStates.SelectingObject:
                SelectingObject();
                break;
            case ToolStates.ChoosingVertex:
                ChoosingVertex();
                break;
            case ToolStates.MovingVertex:
                MovingVertex();
                break;
            default:
                break;
        }
    }

    void SelectingObject()
    {
        if (variablePointer != null)
        {
            if (variablePointer.Active && variablePointer.ValidRaycastTarget)
            {
                if (bActivationButtonPressed)
                {
                    if (variablePointer.PointerHit.transform.gameObject.GetComponent<MeshCollider>())
                    {
                        bBusy = true;

                        variablePointer.SetPointerColour(SCR_ToolMenuRadial.instance.toolBusyPointerColour);

                        selectedObject = variablePointer.PointerHit.transform.gameObject;
                        //selectedObject.layer = 2;
                        objectStartColor = variablePointer.ObjectOriginalColour;
                        currentMeshFilter = selectedObject.GetComponent<MeshFilter>();
                        currentMesh = currentMeshFilter.mesh;
                        selectedObject.GetComponent<Renderer>().material.color = meshEditingColour;
                        currentMeshCollider = selectedObject.GetComponent<MeshCollider>();

                        VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 0.5f, 0.2f, 0.2f);

                        bActivationButtonPressed = false;

                        GenerateVertexHelpers();

                        variablePointer.SetLayerMask(10);

                        currentState = ToolStates.ChoosingVertex;
                    }
                    else
                    {
                        bActivationButtonPressed = false;
                    }

                }
            }
            else
            {
                bActivationButtonPressed = false;
            }
        }
    }

    void ChoosingVertex()
    {
        UpdateHelperScales();

        if (variablePointer != null)
        { 
            if (bActivationButtonPressed)
            {
                if (variablePointer.Active && variablePointer.ValidRaycastTarget)
                {
                    selectedHelperObject = variablePointer.PointerHit.collider.gameObject;
                    selectedHelperObject.GetComponent<Renderer>().material.color = vertexHelperSelectedColour;
                    selectedHelperObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", vertexHelperSelectedColour);
                    selectedHelperScript = selectedHelperObject.GetComponent<SCR_VertexHelper>();
                    selectedHelperObject.layer = 2;
                    startLocation = selectedObject.transform.position;

                    if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
                    {
                        variablePointer.LockPointerLength(true);
                    }

                    currentState = ToolStates.MovingVertex;
                }
                else
                {

                    for (int i = 0; i < vertexHelperList.Count; i++)
                    {
                        Destroy(vertexHelperList[i]);
                    }

                    vertexHelperList.Clear();
                    vertexList.Clear();
                    vertexIndexMatched.Clear();
                    vertexGroupList.Clear();


                    variablePointer.SetLayerMask(8);

                    if (selectedObject.transform.root.GetComponent<SCR_GroupParent>() != null)
                    {
                        selectedObject.transform.root.GetComponent<SCR_GroupParent>().Deselected();
                    }
                    else
                    {
                        selectedObject.GetComponent<Renderer>().material.color = objectStartColor;
                    }

                    bBusy = false;

                    variablePointer.SetPointerColourDefault();

                    currentState = ToolStates.SelectingObject;
                }

                bActivationButtonPressed = false;
            }

            if (variablePointer.Active && variablePointer.ValidRaycastTarget)
            {
                if (variablePointer.PointerHit.collider.gameObject != highlightedHelperObject)
                {
                    highlightedHelperObject = variablePointer.PointerHit.collider.gameObject;
                    highlightedHelperObject.GetComponent<Renderer>().material.color = vertexHelperHighlightedColour;
                    highlightedHelperObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", vertexHelperHighlightedColour);
                    highlightedHelperObject.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
                }
            }
            else
            {
                if (highlightedHelperObject != null)
                {
                    highlightedHelperObject.transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
                    highlightedHelperObject.GetComponent<Renderer>().material.color = vertexHelperColour;
                    highlightedHelperObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", vertexHelperColour);
                    highlightedHelperObject = null;
                }
            }

        }

        
    }

    void MovingVertex()
    {
        UpdateHelperScales();

        if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
        {
            selectedHelperObject.transform.position = Snap(variablePointer.PointerPosition);
        }
        else
        {
            selectedHelperObject.transform.position = variablePointer.PointerPosition;
            
        }

        offsetLocation = selectedHelperObject.transform.position;

        offsetLocation = selectedObject.transform.InverseTransformPoint(offsetLocation);

        UpdateVertexPositions(selectedHelperScript.vertexIndexList, offsetLocation);

        if (bActivationButtonPressed)
        {
            bActivationButtonPressed = false;
            currentMeshCollider.sharedMesh = currentMesh;
            highlightedHelperObject.transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
            highlightedHelperObject.GetComponent<Renderer>().material.color = vertexHelperColour;
            highlightedHelperObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", vertexHelperColour);
            highlightedHelperObject = null;
            selectedHelperObject.layer = 10;

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                variablePointer.LockPointerLength(false);
            }

            VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f, 0.7f, 0.7f);
            currentState = ToolStates.ChoosingVertex;
        }

    }

    void GenerateVertexHelpers()
    {
        currentMesh.GetVertices(vertexList);

        for (int i = 0; i < vertexList.Count; i++)
        {
            Vector3 currentVertexPosition = vertexList[i];
            VertexGroups vg = new VertexGroups();
            int k = 0;
            for (int j = 0; j < vertexList.Count; j++)
            {
                if (vertexList[j] == currentVertexPosition && (!vertexIndexMatched.Contains(j)))
                {
                    vg.vertexPosition.Add(selectedObject.transform.TransformPoint(vertexList[j]));
                    vg.vertexIndex.Add(j);
                    vertexIndexMatched.Add(j);
                    k++;
                }
            }

            if (k >= 1)
            {
                vertexGroupList.Add(vg);
            }
        }

        foreach (var item in vertexGroupList)
        {
            GameObject goHelper = (GameObject)Instantiate(vertexHelperPrefab, item.vertexPosition[0], Quaternion.identity) as GameObject;
            vertexHelperList.Add(goHelper);
            SCR_VertexHelper vertexHelperScript = goHelper.GetComponent<SCR_VertexHelper>();

            for (int i = 0; i < item.vertexIndex.Count; i++)
            {
                vertexHelperScript.vertexIndexList.Add(item.vertexIndex[i]);
            }

        }

        verticesOfMesh = new Vector3[currentMesh.vertices.Length];
    }

    public void UpdateVertexPositions(List<int> verticesToUpdate, Vector3 newPosition)
    {

        verticesOfMesh = currentMesh.vertices;

        for (int i = 0; i < verticesToUpdate.Count; i++)
        {
            verticesOfMesh[verticesToUpdate[i]] = newPosition;
        }

        currentMesh.vertices = verticesOfMesh;

        currentMesh.RecalculateBounds();
        currentMesh.RecalculateNormals();

        currentMeshFilter.mesh = currentMesh;
    }

    private void UpdateHelperScales()
    {
        if (bScaleBasedOnIndividualVertexHelper)
        {
            foreach (var item in vertexHelperList)
            {
                Vector3 difference = headsetTransform.position - item.transform.position;
                float distance = difference.magnitude;

                vertexHelperCurrentScale = Mathf.Min(vertexHelperStartingScale * distance * vertexHelperScaleMultiplier, vertexHelperMaxScale);
                vertexHelperCurrentScale = Mathf.Max(vertexHelperCurrentScale, vertexHelperStartingScale);
                item.transform.localScale = new Vector3(vertexHelperCurrentScale, vertexHelperCurrentScale, vertexHelperCurrentScale);
            }
        }
        else
        {
            Vector3 difference = headsetTransform.position - selectedObject.transform.position;
            float distance = difference.magnitude;

            vertexHelperCurrentScale = Mathf.Min(vertexHelperStartingScale * distance * vertexHelperScaleMultiplier, vertexHelperMaxScale);
            vertexHelperCurrentScale = Mathf.Max(vertexHelperCurrentScale, vertexHelperStartingScale);

            foreach (var item in vertexHelperList)
            {
                item.transform.localScale = new Vector3(vertexHelperCurrentScale, vertexHelperCurrentScale, vertexHelperCurrentScale);
            }
        }
        
    }

    Vector3 Snap(Vector3 snapNearPoint)
    {
        return SCR_GridSnappingOption.instance.GetNearestPointOnGrid(snapNearPoint);
    }
}
