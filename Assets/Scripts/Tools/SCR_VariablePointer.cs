using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public enum PointerStates { Short, Medium, Infinite, Custom, Variable };

public class SCR_VariablePointer : MonoBehaviour, IPointer {

    [SerializeField] private LineRenderer lineRender;
    
    [SerializeField] private GameObject pointerEnd;
    [SerializeField] private GameObject pointerEndSphere;
    [SerializeField] private GameObject pointerEndGridCrossHair;
    [SerializeField] private GameObject pointerEndCircle;
    [SerializeField] private VRTK_ControllerEvents.ButtonAlias activationButton = VRTK_ControllerEvents.ButtonAlias.TouchpadPress;
    [SerializeField] private VRTK_ControllerEvents controllerEvents;
    [SerializeField] private float shortDistance = 0.01f;
    [SerializeField] private float mediumDistance = 1f;
    [SerializeField] private float pointerEndScaleMultiplier = 1.2f;
    [SerializeField] private float pointerEndScaleDefault = 0.007f;
    [SerializeField] private float defaultPointerWidth = 0.003f;

    [SerializeField] private Transform lineRendererTransform;
    public Transform PointerLineRendererStartTransform
    {
        get { return lineRendererTransform; }
    }

    [SerializeField] private bool bSnappingPointerEndActive;
    [SerializeField] private bool bUsePointerCircle;
    [SerializeField] private bool bVariablePointerActive;
    public bool VariablePointerActive
    {
        get { return bVariablePointerActive; }
        set { bVariablePointerActive = value; }
    }

    private PointerStates pointerState = PointerStates.Short;
        
    private Vector3 pointerEndOriginalScale;
    private Vector3 maximumPointerEndScale;
    private Renderer pointerEndRenderer;
    private Renderer pointerEndCircleRenderer;
    private float distanceScale;
    private int geometryLayer;
    private float customDistance;
    private float variableDistance;
    private PointerStates previousPointerState = PointerStates.Short;
    private bool bPointerDistanceChanging;
    private float pointerDistanceChange = 1.01f;
    private GameObject highlightedObject;
    private Renderer highlightedObjectRenderer;
    private SCR_GroupParent highlightedParentScript;
    private SCR_PrefabData highlightedPrefabParentScript;
    private SCR_PrefabData highlightedPrefabScript;
    private SCR_ObjectData highlightedObjectScript;

    private Color highlightedObjectDefaultColour;
    public Color ObjectOriginalColour
    {
        get { return highlightedObjectDefaultColour; }
        set { highlightedObjectDefaultColour = value; }
    }

    private bool bFreezePointerState;
    public bool FreezePointerState
    {
        get { return bFreezePointerState; }
        set { bFreezePointerState = value; }
    }

    public PointerStates CurrentPointerState
    {
        get { return pointerState; }
    }

    private RaycastHit pointerHit;
    public RaycastHit PointerHit
    {
        get { return pointerHit; }
    }

    private bool bValidRaycastTarget;
    public bool ValidRaycastTarget
    {
        get { return bValidRaycastTarget; }
    }

    private bool bToggleDistance = false;
    public bool ToggleDistance
    {
        get { return bToggleDistance; }
        set { bToggleDistance = value; }
    }

    private Vector3 pointerPosition;
    public Vector3 PointerPosition
    {
        get { return pointerPosition; }
    }

    private Vector3 pointerStartPosition;
    public Vector3 PointerStartPosition
    {
        get { return pointerStartPosition; }
    }

    public GameObject PointerEndGameObject
    {
        get { return pointerEnd; }
    }

    private bool bValidTargetPosition;
    public bool ValidTargetPosition
    {
        get { return bValidTargetPosition; }
    }

    private bool bActive = true;
    public bool Active
    {
        get { return bActive; }
        set { bActive = value; }
    }

    private bool bHighlightingActive;
    public bool HighlightingActive
    {
        get { return bHighlightingActive; }
        set { bHighlightingActive = value; }
    }

    private void OnEnable()
    {
        controllerEvents.SubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
    }

    private void OnDisable()
    {
        controllerEvents.UnsubscribeToButtonAliasEvent(activationButton, true, DoActivationButtonPressed);
    }

    void DoActivationButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (!bFreezePointerState && bVariablePointerActive)
        {
            if (pointerState == PointerStates.Custom)
            {
                pointerState = previousPointerState;
            }

            switch (pointerState)
            {
                case PointerStates.Short:
                    pointerState = PointerStates.Medium;
                    break;
                case PointerStates.Medium:
                    pointerState = PointerStates.Infinite;
                    break;
                case PointerStates.Infinite:
                    pointerState = PointerStates.Short;
                    break;
            }

            if (pointerState == PointerStates.Variable)
            {
                if (variableDistance < mediumDistance)
                {
                    pointerState = PointerStates.Medium;
                }
                else if (variableDistance < 1000f)
                {
                    pointerState = PointerStates.Infinite;
                }
            }

            
        }
    }

    public void SnapPointerState(PointerStates newPointerState)
    {
        pointerState = newPointerState;

        switch (pointerState)
        {
            case PointerStates.Short:
                UpdatePointer(shortDistance);
                break;
            case PointerStates.Medium:
                UpdatePointer(mediumDistance);
                break;
            case PointerStates.Infinite:
                UpdatePointer(1000f);
                break;
        }

    }

    public void LockPointerLength(bool lockStatus)
    {
        if (lockStatus)
        {
            previousPointerState = pointerState;
            customDistance = Vector3.Distance(lineRendererTransform.position, pointerEnd.transform.position);
            variableDistance = customDistance;
            pointerState = PointerStates.Custom;
        }
        else
        {
            if(pointerState == PointerStates.Custom || pointerState == PointerStates.Variable)
            {
                pointerState = PointerStates.Variable;
                //variableDistance = customDistance;
            }
        }
    }

    public void SetLayerMask(int layerMaskIndex)
    {
        geometryLayer = 1 << layerMaskIndex;
    }

    // Use this for initialization
    void Start ()
    {
        geometryLayer = 1 << 8;
        pointerEndOriginalScale = pointerEndSphere.transform.localScale;
        maximumPointerEndScale = pointerEndOriginalScale * 15f;
        pointerEndRenderer = pointerEndSphere.GetComponent<Renderer>();
        pointerEndCircleRenderer = pointerEndCircle.GetComponentInChildren<Renderer>();
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        if (bActive)
        {

            lineRender.SetPosition(0, lineRendererTransform.position);
            lineRender.enabled = true;

            if (bVariablePointerActive)
            {
                if (controllerEvents.GetTouchpadAxis().y > 0.1f)
                {
                    if (pointerState != PointerStates.Variable)
                    {
                        previousPointerState = pointerState;
                        pointerState = PointerStates.Variable;
                    }

                    if (!bPointerDistanceChanging)
                    {
                        variableDistance = distanceScale;
                        bPointerDistanceChanging = true;
                    }

                    if (bPointerDistanceChanging)
                    {
                        pointerDistanceChange = pointerDistanceChange * pointerDistanceChange; //exponential
                        pointerDistanceChange = Mathf.Clamp(pointerDistanceChange, 0f, 2f);
                        variableDistance += ((pointerDistanceChange * 0.025f) * controllerEvents.GetTouchpadAxis().y);
                        variableDistance = Mathf.Clamp(variableDistance, shortDistance, 1000f);
                    }
                }
                else if (controllerEvents.GetTouchpadAxis().y < -0.1f)
                {
                    if (pointerState != PointerStates.Variable)
                    {
                        previousPointerState = pointerState;
                        variableDistance = distanceScale;
                        pointerState = PointerStates.Variable;
                    }

                    if (!bPointerDistanceChanging)
                    {
                        variableDistance = distanceScale;
                        bPointerDistanceChanging = true;
                    }

                    if (bPointerDistanceChanging)
                    {
                        pointerDistanceChange = pointerDistanceChange * pointerDistanceChange; //exponential
                        pointerDistanceChange = Mathf.Clamp(pointerDistanceChange, 0f, 2f);
                        variableDistance -= ((pointerDistanceChange * 0.025f) * -controllerEvents.GetTouchpadAxis().y);
                        variableDistance = Mathf.Clamp(variableDistance, shortDistance, 1000f);
                    }
                }
                else
                {
                    bPointerDistanceChanging = false;
                    pointerDistanceChange = 1.01f;
                }
            }
            
            switch (pointerState)
            {
                case PointerStates.Short:
                    CalculateShortPointer();
                    break;
                case PointerStates.Medium:
                    CalculateMediumPointer();
                    break;
                case PointerStates.Infinite:
                    CalculateInfinitePointer();
                    break;
                case PointerStates.Custom:
                    CalculateCustomPointer();
                    break;
                case PointerStates.Variable:
                    CalculateVariablePointer();
                    break;
            }

            if (bSnappingPointerEndActive)
            {
                if (SCR_GridSnappingOption.instance.SnappingActive == OptionActive.On)
                {
                    pointerEndSphere.SetActive(false);
                    pointerEndGridCrossHair.transform.position = SCR_GridSnappingOption.instance.GetNearestPointOnGrid(pointerEndSphere.transform.position);
                }
                else
                {
                    pointerEndGridCrossHair.transform.position = pointerEndSphere.transform.position;
                }

                pointerEndGridCrossHair.transform.localScale = pointerEndSphere.transform.localScale;
            } 

        }
        else
        {
            lineRender.enabled = false;
            pointerEnd.SetActive(false);
        }
    }

    void CalculateShortPointer()
    {
        if (Physics.Raycast(lineRendererTransform.position, lineRendererTransform.forward, out pointerHit, shortDistance, geometryLayer))
        {
            bValidRaycastTarget = true;

            if (bHighlightingActive)
            {
                HighlightObjects(pointerHit.collider.gameObject);
            }
            else
            {
                RemoveHighlight();
            }

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                pointerEnd.SetActive(true);
                bValidRaycastTarget = true;
                pointerPosition = pointerHit.point;
                distanceScale = Vector3.Distance(lineRendererTransform.position, pointerHit.point);
                if (bUsePointerCircle)
                {
                    pointerEndCircle.SetActive(true);
                    pointerEndCircle.transform.forward = pointerHit.normal;
                }
                //pointerEndSphere.transform.localScale = Vector3.Min(pointerEndOriginalScale * (distanceScale * pointerEndScaleMultiplier), maximumPointerEndScale);
            }
            else
            {
                pointerEndCircle.SetActive(false);
                distanceScale = shortDistance;
            }
        }
        else
        {
            RemoveHighlight();
            pointerEndCircle.SetActive(false);
            distanceScale = shortDistance;
            bValidRaycastTarget = false;
        }

        pointerEndSphere.transform.localScale = pointerEndOriginalScale;
        UpdatePointer(distanceScale);
  
    }

    void CalculateMediumPointer()
    {
        if (Physics.Raycast(lineRendererTransform.position, lineRendererTransform.forward, out pointerHit, mediumDistance, geometryLayer))
        {
            bValidRaycastTarget = true;

            if (bHighlightingActive)
            {
                HighlightObjects(pointerHit.collider.gameObject);
            }
            else
            {
                RemoveHighlight();
            }

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                pointerEnd.SetActive(true);
                bValidRaycastTarget = true;
                pointerPosition = pointerHit.point;
                distanceScale = Vector3.Distance(lineRendererTransform.position, pointerHit.point);
                if (bUsePointerCircle)
                {
                    pointerEndCircle.SetActive(true);
                    pointerEndCircle.transform.forward = pointerHit.normal;
                }
                //pointerEndSphere.transform.localScale = Vector3.Min(pointerEndOriginalScale * (distanceScale * pointerEndScaleMultiplier), maximumPointerEndScale);
            }
            else
            {
                pointerEndCircle.SetActive(false);
                distanceScale = mediumDistance;
            }
        }
        else
        {
            RemoveHighlight();
            distanceScale = mediumDistance;
            pointerEndCircle.SetActive(false);
            bValidRaycastTarget = false;
        }

        pointerEndSphere.transform.localScale = pointerEndOriginalScale;
        UpdatePointer(distanceScale);
    }

    void CalculateInfinitePointer()
    {
        if (Physics.Raycast(lineRendererTransform.position, lineRendererTransform.forward, out pointerHit, Mathf.Infinity, geometryLayer))
        {
            if (bHighlightingActive)
            {
                HighlightObjects(pointerHit.collider.gameObject);
            }
            else
            {
                RemoveHighlight();
            }

            pointerEnd.SetActive(true);
            bValidTargetPosition = true;
            bValidRaycastTarget = true;
            pointerPosition = pointerHit.point;
            distanceScale = Vector3.Distance(lineRendererTransform.position, pointerHit.point);
            UpdatePointer(distanceScale);
            pointerEndSphere.transform.localScale = Vector3.Min(pointerEndOriginalScale * (distanceScale * pointerEndScaleMultiplier), maximumPointerEndScale);
            if (pointerEndSphere.transform.localScale.x <= pointerEndOriginalScale.x)
            {
                pointerEndSphere.transform.localScale = pointerEndOriginalScale;
            }

            if (bUsePointerCircle)
            {
                pointerEndCircle.SetActive(true);
                pointerEndCircle.transform.forward = pointerHit.normal;
            }
        }
        else
        {
            RemoveHighlight();
            bValidTargetPosition = false;
            bValidRaycastTarget = false;
            UpdatePointer(1000f);
            pointerEnd.SetActive(false);
            pointerEndCircle.SetActive(false);
        }
    }

    void CalculateCustomPointer()
    {
        pointerEnd.SetActive(true);
        bValidTargetPosition = true;
        bValidRaycastTarget = true;
        pointerPosition = pointerEnd.transform.position;
        distanceScale = Vector3.Distance(lineRendererTransform.position, pointerPosition);
        UpdatePointer(distanceScale);
    }

    void CalculateVariablePointer()
    {
        if (Physics.Raycast(lineRendererTransform.position, lineRendererTransform.forward, out pointerHit, variableDistance, geometryLayer))
        {
            bValidRaycastTarget = true;

            if (bHighlightingActive)
            {
                HighlightObjects(pointerHit.collider.gameObject);
            }
            else
            {
                RemoveHighlight();
            }
            

            if (SCR_SurfaceSnappingOption.instance.SurfaceSnappingActive == OptionActive.On)
            {
                pointerEnd.SetActive(true);
                bValidRaycastTarget = true;
                pointerPosition = pointerHit.point;
                distanceScale = Vector3.Distance(lineRendererTransform.position, pointerHit.point);
                if (bUsePointerCircle)
                {
                    pointerEndCircle.SetActive(true);
                    pointerEndCircle.transform.forward = pointerHit.normal;
                }
            }
            else
            {
                pointerEndCircle.SetActive(false);
                distanceScale = variableDistance;
            }
        }
        else
        {
            if (!SCR_ToolMenuRadial.instance.ToolBusy())
            {
                RemoveHighlight();
            }
            pointerEndCircle.SetActive(false);
            distanceScale = variableDistance;
            bValidRaycastTarget = false;
        }

        pointerEndSphere.transform.localScale = Vector3.Min(pointerEndOriginalScale * (distanceScale * pointerEndScaleMultiplier), maximumPointerEndScale);
        if (pointerEndSphere.transform.localScale.x <= pointerEndOriginalScale.x)
        {
            pointerEndSphere.transform.localScale = pointerEndOriginalScale;
        }
        //pointerEndSphere.transform.localScale = pointerEndOriginalScale;
        UpdatePointer(distanceScale);
    }

    void UpdatePointer(float pointerLength)
    {
        Vector3 distancePosition = Vector3.zero;

        if (lineRendererTransform)
        {
            pointerStartPosition = lineRendererTransform.position;
            distancePosition = lineRendererTransform.position + (lineRendererTransform.forward * pointerLength);
        }
        
        pointerPosition = distancePosition;

        if (pointerEnd)
        {
            pointerEnd.transform.position = distancePosition;
            pointerEnd.SetActive(true);
        }
        
        //pointerEndSphere.transform.localScale = pointerEndOriginalScale;
        

        if (lineRender)
        {
            lineRender.SetPosition(1, distancePosition);
        }
        
        bValidTargetPosition = true;
    }

    //Code to highlight objects on geometry layer
    void HighlightObjects(GameObject newObject)
    {
        if (bHighlightingActive)
        {
            if (newObject.transform.parent)
            {
                if (newObject.transform.root.gameObject.GetComponent<SCR_GroupParent>() != null)
                {
                    if (highlightedParentScript != newObject.transform.root.gameObject.GetComponent<SCR_GroupParent>())
                    {
                        RemoveHighlight();
                        highlightedObject = newObject;
                        highlightedParentScript = highlightedObject.transform.root.gameObject.GetComponent<SCR_GroupParent>();
                        highlightedParentScript.CheckMaterialCache();
                        highlightedParentScript.CurrentlyHighlighted();
                    } 
                }
                else if (newObject.transform.root.gameObject.GetComponent<SCR_PrefabData>() != null)
                {
                    if (highlightedPrefabParentScript != newObject.transform.root.gameObject.GetComponent<SCR_PrefabData>())
                    {
                        RemoveHighlight();
                        highlightedObject = newObject;
                        highlightedPrefabParentScript = highlightedObject.transform.root.gameObject.GetComponent<SCR_PrefabData>();
                        highlightedPrefabParentScript.CurrentlyHighlighted();
                    }
                }
                else
                {
                    RemoveHighlight();
                }
            }
            else
            {
                if (highlightedObject != newObject)
                {
                    RemoveHighlight();
                    if (newObject.GetComponent<SCR_ObjectData>() != null)
                    {
                        if (highlightedObjectScript != newObject.GetComponent<SCR_ObjectData>())
                        {
                            if (highlightedObjectRenderer != null)
                            {
                                highlightedObjectRenderer.material.color = highlightedObjectDefaultColour;
                            }

                            highlightedObject = newObject;
                            highlightedObjectRenderer = highlightedObject.GetComponent<Renderer>();
                            highlightedObjectScript = highlightedObject.GetComponent<SCR_ObjectData>();
                            highlightedObjectDefaultColour = highlightedObjectRenderer.material.color;
                            highlightedObjectRenderer.material.color = SCR_ToolMenuRadial.instance.highlightedGameObjectColour;
                        }
                        
                    }
                    else if (newObject.GetComponent<SCR_PrefabData>() != null)
                    {
                        if (highlightedPrefabScript != newObject.GetComponent<SCR_PrefabData>())
                        {
                            highlightedObject = newObject;
                            highlightedPrefabScript = highlightedObject.GetComponent<SCR_PrefabData>();
                            highlightedPrefabScript.CurrentlyHighlighted();
                        }
                    }
                    else
                    {
                        RemoveHighlight();
                    }
                }
            }        
        }

        
    }

    public void RemoveHighlight()
    {
        if (highlightedObject != null)
        {
            if (highlightedObject.transform.parent)
            {
                if (highlightedParentScript != null)
                {
                    highlightedParentScript.StopHighlighting();
                }

                if (highlightedPrefabParentScript != null)
                {
                    highlightedPrefabParentScript.StopHighlighting();
                }
            }
            else
            {
                if (highlightedPrefabScript != null)
                {
                    highlightedPrefabScript.StopHighlighting();
                }
                else
                {
                    if (highlightedObjectRenderer)
                    {
                        highlightedObjectRenderer.material.color = highlightedObjectDefaultColour;
                    }
                    
                }
            }

            highlightedObject = null;
            highlightedParentScript = null;
            highlightedPrefabScript = null;
            highlightedObjectScript = null;
            highlightedPrefabParentScript = null;
            highlightedObjectRenderer = null;

        }
    }

    public void SetPointerColour(Color newColour)
    {
        lineRender.startColor = newColour;
        lineRender.endColor = newColour;
        pointerEndRenderer.material.color = newColour;

        Color alphaNewColour = newColour;
        pointerEndCircleRenderer.material.SetColor("_Albedo", alphaNewColour);
        pointerEndCircleRenderer.material.SetColor("_Color", alphaNewColour);
    }

    public void SetPointerColourDefault()
    {
        if (lineRender)
        {
            lineRender.startColor = Color.green;
            lineRender.endColor = Color.green;
        }

        if (pointerEndRenderer)
        {
            pointerEndRenderer.material.color = Color.green;
        }

        if (pointerEndCircleRenderer)
        {
            Color alphaGreen = Color.green;
            pointerEndCircleRenderer.material.SetColor("_Albedo", alphaGreen);
            pointerEndCircleRenderer.material.SetColor("_Color", alphaGreen);
        }
        
    }

    public void SetPointerWidth(float newStartWidth, float newEndWidth)
    {
        lineRender.startWidth = newStartWidth;
        lineRender.endWidth = newEndWidth;
    }

    public void SetPointerWidthDefault()
    {
        lineRender.startWidth = defaultPointerWidth;
        lineRender.endWidth = defaultPointerWidth;
    }

    public void SetPointerEndSize(float newScale)
    {
        pointerEndOriginalScale = new Vector3(newScale, newScale, newScale);
    }

    public void SetPointerEndSizeDefault()
    {
        pointerEndOriginalScale = new Vector3(pointerEndScaleDefault, pointerEndScaleDefault, pointerEndScaleDefault);
        pointerEndSphere.transform.localScale = pointerEndOriginalScale;
    }

    public void AllowHighlighting(bool newBool)
    {

    }
}
