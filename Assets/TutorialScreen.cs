using UnityEngine;
using UnityEngine.UI;

public class TutorialScreen : MonoBehaviour
{
    // Assign your images in the Inspector
    public Sprite[] imagesToShow; // Array of images to display

    private Image uiImage;         // Reference to the UI Image component
    private int currentImageIndex; // Index of the current image

    void Start()
    {
        // Find the UI Image component on this GameObject
        uiImage = GetComponent<Image>();

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
        if (Input.GetMouseButtonDown(0))
        {
            ShowNextImage();
        }
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


