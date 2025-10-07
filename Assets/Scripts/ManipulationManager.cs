using UnityEngine;

namespace DefaultNamespace
{
    public enum ManipulationType
    {
        Move,
        Rotate,
        Scale
    }

    public class ManipulationManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject manipulationGizmoPrefab;

        private GameObject targetObject;
        private GameObject attachedManipulationObject;
        private Transform originalParent;

        /// <summary>
        /// Sets the target GameObject and attaches a manipulation object to it
        /// </summary>
        /// <param name="target">The GameObject to manipulate</param>
        public void SetTarget(GameObject target)
        {
            Debug.Log("Setting Target in ManipulationManager: " + target.name + "");

            // Clean up previous manipulation object
            if (attachedManipulationObject != null)
            {
                ClearTarget();
            }

            targetObject = target;

            if (targetObject != null)
            {
                // Store the original parent
                originalParent = targetObject.transform.parent;

                // Create and attach manipulation object
                if (manipulationGizmoPrefab != null)
                {
                    // Instantiate gizmo at the target's position and rotation
                    attachedManipulationObject = Instantiate(manipulationGizmoPrefab, targetObject.transform.position, targetObject.transform.rotation);

                    // Child the target GameObject to the gizmo
                    targetObject.transform.SetParent(attachedManipulationObject.transform);
                }
            }
        }

        /// <summary>
        /// Clears the current target and removes the manipulation object
        /// </summary>
        public void ClearTarget()
        {
            // Return target to original parent before destroying gizmo
            if (targetObject != null && originalParent != null)
            {
                targetObject.transform.SetParent(originalParent);
            }
            else if (targetObject != null)
            {
                // If there was no original parent, unparent the object
                targetObject.transform.SetParent(null);
            }

            if (attachedManipulationObject != null)
            {
                Destroy(attachedManipulationObject);
                attachedManipulationObject = null;
            }

            targetObject = null;
            originalParent = null;
        }

        /// <summary>
        /// Gets the current target GameObject
        /// </summary>
        public GameObject GetTarget()
        {
            return targetObject;
        }

        /// <summary>
        /// Checks if the gizmo is currently handling input
        /// </summary>
        /// <returns>True if gizmo is actively being manipulated</returns>
        public bool IsGizmoActive()
        {
            if (attachedManipulationObject == null)
                return false;

            MoveGizmo moveGizmo = attachedManipulationObject.GetComponent<MoveGizmo>();
            if (moveGizmo != null)
            {
                return moveGizmo.IsHandlingInput;
            }

            return false;
        }
    }
}