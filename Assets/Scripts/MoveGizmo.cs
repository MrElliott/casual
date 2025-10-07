using UnityEngine;

public class MoveGizmo : MonoBehaviour
{
    public float handleLength = 1.0f;
    public GameObject xHandlePrefab, yHandlePrefab, zHandlePrefab;

    [SerializeField]
    private LayerMask handleLayer = 0; // Layer to assign to handles

    [SerializeField]
    private float handleOffsetFromBounds = 0.2f; // Additional offset from object bounds

    [SerializeField]
    private float minimumHandleDistance = 1.0f; // Minimum distance for handles from center

    private float currentHandleDistance = 1.0f; // Current calculated handle distance

    [SerializeField]
    private ManipulationType manipulationMode = ManipulationType.Move;

    [SerializeField]
    private float rotationSpeed = 100f; // Degrees per unit of drag

    [SerializeField]
    private float scaleSpeed = 1f; // Scale multiplier per unit of drag

    [SerializeField]
    private float lineThickness = 0.05f; // Thickness of the line handles

    private GameObject xHandle, yHandle, zHandle;
    private GameObject xLine, yLine, zLine; // Line handles
    private GameObject selectedHandle;
    private Camera cam;
    private Vector3 dragStartOffset; // Stores the initial offset
    private Plane dragPlane;         // Plane for dragging
    private Vector3 dragAxis;        // Axis to constrain movement

    private Vector3? initialDragPos = null;
    
    private Collider[] handleColliders; // Store colliders to toggle during dragging

    /// <summary>
    /// Returns true if the gizmo is currently handling input (selected or dragging)
    /// </summary>
    public bool IsHandlingInput => selectedHandle != null;

    void Start()
    {
        cam = Camera.main;

        // Calculate appropriate handle distance based on object bounds
        CalculateHandleDistance();

        // Instantiate handles
        xHandle = Instantiate(xHandlePrefab, transform.position, Quaternion.identity);
        yHandle = Instantiate(yHandlePrefab, transform.position, Quaternion.identity);
        zHandle = Instantiate(zHandlePrefab, transform.position, Quaternion.identity);

        // Configure handle positions and rotations
        SetupHandle(xHandle, Vector3.right, Color.red);
        SetupHandle(yHandle, Vector3.up, Color.green);
        SetupHandle(zHandle, Vector3.forward, Color.blue);

        // Create line handles
        xLine = CreateLineHandle(Vector3.right, currentHandleDistance, Color.red);
        yLine = CreateLineHandle(Vector3.up, currentHandleDistance, Color.green);
        zLine = CreateLineHandle(Vector3.forward, currentHandleDistance, Color.blue);

        // Get all handle colliders (including line handles)
        handleColliders = new Collider[] { 
            xHandle.GetComponent<Collider>(), 
            yHandle.GetComponent<Collider>(), 
            zHandle.GetComponent<Collider>(),
            xLine.GetComponent<Collider>(),
            yLine.GetComponent<Collider>(),
            zLine.GetComponent<Collider>()
        };
    }

    void Update()
    {
        //UpdateGizmoPosition(); // Always align gizmo with parent object
        HandleSelection();
        HandleDragging();
    }

    /// <summary>
    /// Sets the manipulation mode for the gizmo
    /// </summary>
    public void SetManipulationMode(ManipulationType mode)
    {
        Debug.Log("Setting Manipulation Mode to: " + mode);
        manipulationMode = mode;
        UpdateHandleColors();
    }

    /// <summary>
    /// Gets the current manipulation mode
    /// </summary>
    public ManipulationType GetManipulationMode()
    {
        return manipulationMode;
    }

    /// <summary>
    /// Calculates the appropriate handle distance based on the object's bounds
    /// </summary>
    private void CalculateHandleDistance()
    {
        // Look for child objects (the actual target object is a child of this gizmo)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            currentHandleDistance = minimumHandleDistance;
            return;
        }

        // Calculate combined bounds of all child renderers in world space
        Bounds combinedBounds = new Bounds(transform.position, Vector3.zero);
        bool boundsInitialized = false;

        foreach (Renderer renderer in renderers)
        {
            // Skip if this renderer belongs to a handle
            if (renderer.gameObject.name.Contains("Handle"))
                continue;

            if (!boundsInitialized)
            {
                combinedBounds = renderer.bounds;
                boundsInitialized = true;
            }
            else
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }
        }

        if (boundsInitialized)
        {
            // Calculate the maximum distance from gizmo center to bounds edge
            Vector3 gizmoCenter = transform.position;

            // Get the 8 corners of the bounds
            Vector3 min = combinedBounds.min;
            Vector3 max = combinedBounds.max;

            // Calculate distance from gizmo center to furthest corner in local space
            float maxDistance = 0f;

            // Check all 8 corners of the bounding box
            Vector3[] corners = new Vector3[]
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };

            foreach (Vector3 corner in corners)
            {
                float distance = Vector3.Distance(gizmoCenter, corner);
                maxDistance = Mathf.Max(maxDistance, distance);
            }

            currentHandleDistance = Mathf.Max(maxDistance + handleOffsetFromBounds, minimumHandleDistance);

            Debug.Log($"Calculated handle distance: {currentHandleDistance} (max distance to corner: {maxDistance}, bounds size: {combinedBounds.size})");
        }
        else
        {
            currentHandleDistance = minimumHandleDistance;
            Debug.Log($"No bounds found, using minimum handle distance: {minimumHandleDistance}");
        }
    }

    private void SetupHandle(GameObject handle, Vector3 direction, Color color)
    {
        handle.transform.SetParent(transform);
        handle.transform.localPosition = direction * currentHandleDistance;
        handle.transform.localRotation = Quaternion.LookRotation(direction);

        Renderer renderer = handle.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        handle.tag = "Handle"; // Tag the handle for raycast detection

        // Set the layer for the handle
        if (handleLayer != 0)
        {
            int layer = (int)Mathf.Log(handleLayer.value, 2);
            handle.layer = layer;

            // Also set layer for all child objects
            foreach (Transform child in handle.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = layer;
            }
        }
    }

    /// <summary>
    /// Creates a line handle programmatically that extends from the center to the specified direction
    /// </summary>
    private GameObject CreateLineHandle(Vector3 direction, float length, Color color)
    {
        // Create a cylinder primitive as the line
        GameObject lineHandle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        lineHandle.transform.SetParent(transform);
        lineHandle.name = $"LineHandle_{direction}";

        // Position the line at the midpoint between center and handle
        lineHandle.transform.localPosition = direction * (length / 2f);

        // Rotate to point in the correct direction
        lineHandle.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);

        // Scale the line: length along the direction, thickness for width/depth
        lineHandle.transform.localScale = new Vector3(lineThickness, length / 2f, lineThickness);

        // Create a new material for the line
        Renderer renderer = lineHandle.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Create a new material instance to avoid shared material issues
            Material lineMaterial = new Material(Shader.Find("Standard"));
            lineMaterial.color = color;
            renderer.material = lineMaterial;
        }

        // Tag as handle
        lineHandle.tag = "Handle";

        // Set layer
        if (handleLayer != 0)
        {
            int layer = (int)Mathf.Log(handleLayer.value, 2);
            lineHandle.layer = layer;

            foreach (Transform child in lineHandle.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = layer;
            }
        }

        return lineHandle;
    }

    private void UpdateGizmoPosition()
    {
        // Ensure the gizmo always follows the parent object's position
        xHandle.transform.position = transform.position + transform.right * handleLength;
        yHandle.transform.position = transform.position + transform.up * handleLength;
        zHandle.transform.position = transform.position + transform.forward * handleLength;
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Handle"))
                {
                    selectedHandle = hit.collider.gameObject;

                    // Disable all handle colliders to prevent raycast interference
                    ToggleHandleColliders(false);

                    // Determine the axis for dragging based on which handle or line was clicked
                    if (selectedHandle == xHandle || selectedHandle == xLine) 
                        dragAxis = transform.right;
                    else if (selectedHandle == yHandle || selectedHandle == yLine) 
                        dragAxis = transform.up;
                    else if (selectedHandle == zHandle || selectedHandle == zLine) 
                        dragAxis = transform.forward;

                    dragPlane = new Plane(-cam.transform.forward, transform.position);

                    // Calculate the initial offset
                    if (dragPlane.Raycast(ray, out float enter))
                    {
                        Vector3 hitPoint = ray.GetPoint(enter);
                        dragStartOffset = Vector3.Project(transform.position - hitPoint, dragAxis);
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) // Release click
        {
            selectedHandle = null;
            
            lastHitPoint = Vector3.zero;

            // Re-enable handle colliders after dragging
            ToggleHandleColliders(true);
        }
    }

    Vector3 lastHitPoint = Vector3.zero;
    
    private void HandleDragging()
    {
        if (selectedHandle != null && Input.GetMouseButton(0)) // Dragging
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // Perform dragging along the selected axis
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                
                if(lastHitPoint == Vector3.zero)
                    lastHitPoint = hitPoint;

                // Calculate the movement along the drag axis
                float axisMovement = Vector3.Dot(hitPoint - lastHitPoint, dragAxis);

                // Apply transformation based on manipulation mode
                switch (manipulationMode)
                {
                    case ManipulationType.Move:
                        HandleMove(axisMovement);
                        break;
                    case ManipulationType.Rotate:
                        HandleRotate(axisMovement);
                        break;
                    case ManipulationType.Scale:
                        HandleScale(axisMovement);
                        break;
                }

                lastHitPoint = hitPoint;
            }
        }
    }

    private void HandleMove(float axisMovement)
    {
        // Apply movement along the selected axis only
        Vector3 newPosition = transform.position + dragAxis * axisMovement;

        // Lock the position to the drag plane
        transform.position = Vector3.Project(newPosition, dragAxis) + Vector3.ProjectOnPlane(transform.position, dragAxis);
    }

    private void HandleRotate(float axisMovement)
    {
        // Rotate around the drag axis
        float rotationAmount = axisMovement * rotationSpeed;
        transform.Rotate(dragAxis, rotationAmount, Space.World);
    }

    private void HandleScale(float axisMovement)
    {
        // Scale along the drag axis
        float scaleAmount = 1f + (axisMovement * scaleSpeed);

        // Get current local scale
        Vector3 currentScale = transform.localScale;

        // Determine which axis to scale
        Vector3 scaleChange = Vector3.one;
        if (dragAxis == transform.right)
            scaleChange = new Vector3(scaleAmount, 1f, 1f);
        else if (dragAxis == transform.up)
            scaleChange = new Vector3(1f, scaleAmount, 1f);
        else if (dragAxis == transform.forward)
            scaleChange = new Vector3(1f, 1f, scaleAmount);

        // Apply scale
        transform.localScale = Vector3.Scale(currentScale, scaleChange);

        // Clamp scale to prevent negative or zero values
        transform.localScale = new Vector3(
            Mathf.Max(0.01f, transform.localScale.x),
            Mathf.Max(0.01f, transform.localScale.y),
            Mathf.Max(0.01f, transform.localScale.z)
        );
    }

    private void UpdateHandleColors()
    {
        // Update handle colors based on manipulation mode
        Color xColor = Color.red;
        Color yColor = Color.green;
        Color zColor = Color.blue;

        // Optionally modify colors based on mode for visual feedback
        switch (manipulationMode)
        {
            case ManipulationType.Rotate:
                xColor = new Color(1f, 0.5f, 0.5f); // Lighter red
                yColor = new Color(0.5f, 1f, 0.5f); // Lighter green
                zColor = new Color(0.5f, 0.5f, 1f); // Lighter blue
                break;
            case ManipulationType.Scale:
                xColor = new Color(1f, 0.8f, 0f); // Orange-red
                yColor = new Color(0.8f, 1f, 0f); // Yellow-green
                zColor = new Color(0f, 0.8f, 1f); // Cyan-blue
                break;
        }

        if (xHandle != null)
        {
            Renderer xRenderer = xHandle.GetComponent<Renderer>();
            if (xRenderer != null) xRenderer.material.color = xColor;
        }

        if (yHandle != null)
        {
            Renderer yRenderer = yHandle.GetComponent<Renderer>();
            if (yRenderer != null) yRenderer.material.color = yColor;
        }

        if (zHandle != null)
        {
            Renderer zRenderer = zHandle.GetComponent<Renderer>();
            if (zRenderer != null) zRenderer.material.color = zColor;
        }

        // Update line handle colors
        if (xLine != null)
        {
            Renderer xLineRenderer = xLine.GetComponent<Renderer>();
            if (xLineRenderer != null) xLineRenderer.material.color = xColor;
        }

        if (yLine != null)
        {
            Renderer yLineRenderer = yLine.GetComponent<Renderer>();
            if (yLineRenderer != null) yLineRenderer.material.color = yColor;
        }

        if (zLine != null)
        {
            Renderer zLineRenderer = zLine.GetComponent<Renderer>();
            if (zLineRenderer != null) zLineRenderer.material.color = zColor;
        }
    }



    private void ToggleHandleColliders(bool enabled)
    {
        foreach (Collider collider in handleColliders)
        {
            collider.enabled = enabled;
        }
    }
}
