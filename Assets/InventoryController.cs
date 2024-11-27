using UnityEngine;

public class InventoryController : MonoBehaviour
{
    // Reference to your Scroll View GameObject
    public GameObject scrollView;

    // Tracks whether the Scroll View is currently open
    private bool isOpen = false;

    //reference to camera controller
    private CameraController cameraMovements;

    void Start()
    {
        cameraMovements = FindObjectOfType<CameraController>();
        // Ensure the Scroll View is initially inactive
        if (scrollView != null)
        {
            scrollView.SetActive(false);
        }
        else
        {
            Debug.LogError("Scroll View not assigned in the inspector.");
        }
    }

    void Update()
    {
        // Check if the "I" key was pressed down this frame
        if (Input.GetKeyDown(KeyCode.I))
        {
            
            ToggleInventory();
             if (isOpen ) {
                cameraMovements.allowMovement = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } else {
                if (!isOpen) {
                cameraMovements.allowMovement = true;
                Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }

    void ToggleInventory()
    {
        if (scrollView != null)
        {
            // Toggle the open state
            isOpen = !isOpen;

            // Set the active state of the Scroll View accordingly
            scrollView.SetActive(isOpen);
           
        }
    }
}
