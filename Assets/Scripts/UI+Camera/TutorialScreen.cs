using UnityEngine;
using UnityEngine.UI;
using System.Collections; 

public class TutorialScreen : MonoBehaviour
{
    public Sprite[] imagesToShow; // Array of images to display

    private Image uiImage;         // Reference to the UI Image component
    private int currentImageIndex; // Index of the current image

    private GrappleScript playerGrappleScript;
    private bool isWaiting = false; // Flag to prevent multiple coroutines

    void Start()
    {
        uiImage = GetComponent<Image>();
        
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            // Get the GrappleScript component from the player GameObject
            playerGrappleScript = player.GetComponent<GrappleScript>();

            if (playerGrappleScript == null)
            {
                Debug.LogError("Can't find the GrappleScript component on the player GameObject.");
            }
        }
        else
        {
            Debug.LogError("Player GameObject not found. Make sure it is tagged 'Player'.");
        }

        if (uiImage == null)
        {
            Debug.LogError("No Image component found on this GameObject!");
            return;
        }

        // Initialize the first image
        if (imagesToShow != null && imagesToShow.Length > 0)
        {
            currentImageIndex = 0;
            uiImage.sprite = imagesToShow[currentImageIndex];
            uiImage.enabled = true; // Ensure the image is enabled
        }
        else
        {
            Debug.LogError("No images have been assigned to display.");
        }
    }

    void Update()
    {
        // Check for left mouse button click
        if (currentImageIndex == 0 && Input.GetMouseButtonDown(0)) //start
        {
            ShowNextImage();
        }
        else if (currentImageIndex == 1 && Input.GetKeyDown(KeyCode.W)) // Moved forward
        {
            if (!isWaiting) // Prevent multiple coroutines
            {
                StartCoroutine(WaitAndShowNextImage(3f));
            }
        }
        else if (currentImageIndex == 2 && Input.GetKeyDown(KeyCode.Space)) //jump
        {
            if (!isWaiting)
            {
                StartCoroutine(WaitAndShowNextImage(2f));
            }
        }
        else if (currentImageIndex == 3 && playerGrappleScript.currentWeapon != null) //equip weapon
        {
        
                Debug.Log("Player has a weapon equipped.");
                if (!isWaiting)
            {
                StartCoroutine(WaitAndShowNextImage(1f));
            }
            
        }
        else if (currentImageIndex == 4 && Input.GetMouseButtonDown(0)) //attack
        {
            if (!isWaiting)
            {
                StartCoroutine(WaitAndShowNextImage(2f));
            }
        }
        else if (currentImageIndex == 5 && playerGrappleScript.grappling) //grapple
        {
            if (!isWaiting)
            {
                StartCoroutine(WaitAndShowNextImage(1f));
            }
        }
    }

    // Coroutine to wait and then show the next image
    IEnumerator WaitAndShowNextImage(float waitTime)
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        ShowNextImage();
        isWaiting = false;
    }

    void ShowNextImage()
    {
        currentImageIndex++;

        if (currentImageIndex >= imagesToShow.Length)
        {
            uiImage.enabled = false;
            // Stop updating after the last image
            return;
        }

        uiImage.sprite = imagesToShow[currentImageIndex];
    }
}