using UnityEngine;

public class MoveGizmo : MonoBehaviour
{
    public float handleLength = 1.0f;
    public GameObject xHandlePrefab, yHandlePrefab, zHandlePrefab;

    private GameObject xHandle, yHandle, zHandle;
    private GameObject selectedHandle;
    private Camera cam;
    private Vector3 dragStartOffset; // Stores the initial offset
    private Plane dragPlane;         // Plane for dragging
    private Vector3 dragAxis;        // Axis to constrain movement

    private Vector3? initialDragPos = null;
    
    private Collider[] handleColliders; // Store colliders to toggle during dragging

    void Start()
    {
        cam = Camera.main;

        // Instantiate handles
        xHandle = Instantiate(xHandlePrefab, transform.position, Quaternion.identity);
        yHandle = Instantiate(yHandlePrefab, transform.position, Quaternion.identity);
        zHandle = Instantiate(zHandlePrefab, transform.position, Quaternion.identity);

        // Configure handle positions and rotations
        SetupHandle(xHandle, Vector3.right, Color.red);
        SetupHandle(yHandle, Vector3.up, Color.green);
        SetupHandle(zHandle, Vector3.forward, Color.blue);

        // Get all handle colliders
        handleColliders = new Collider[] { xHandle.GetComponent<Collider>(), yHandle.GetComponent<Collider>(), zHandle.GetComponent<Collider>() };
    }

    void Update()
    {
        //UpdateGizmoPosition(); // Always align gizmo with parent object
        HandleSelection();
        HandleDragging();
    }

    private void SetupHandle(GameObject handle, Vector3 direction, Color color)
    {
        handle.transform.SetParent(transform);
        handle.transform.localPosition = direction * handleLength;
        handle.transform.localRotation = Quaternion.LookRotation(direction);

        Renderer renderer = handle.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        handle.tag = "Handle"; // Tag the handle for raycast detection
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

                    // Determine the axis for dragging and create a plane perpendicular to the camera
                    if (selectedHandle == xHandle) dragAxis = transform.right;
                    if (selectedHandle == yHandle) dragAxis = transform.up;
                    if (selectedHandle == zHandle) dragAxis = transform.forward;

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

                // Calculate the position along the drag axis
                float axisMovement = Vector3.Dot(hitPoint - lastHitPoint, dragAxis);

                // Apply movement along the selected axis only
                Vector3 newPosition = transform.position + dragAxis * axisMovement;

                // Lock the position to the drag plane
                transform.position = Vector3.Project(newPosition, dragAxis) + Vector3.ProjectOnPlane(transform.position, dragAxis);

                lastHitPoint = hitPoint;
            }
        }
    }



    private void ToggleHandleColliders(bool enabled)
    {
        foreach (var collider in handleColliders)
        {
            collider.enabled = enabled;
        }
    }
}
