using UnityEngine;
using UnityEngine.UI;

public class TutorialScreen : MonoBehaviour
{
    // Assign this in the Inspector or via code
    public Sprite imageToShow; // The image to display
    private Image uiImage;     // Reference to the UI Image component

    void Start()
    {
        // Find the UI Image component on this GameObject
        uiImage = GetComponent<Image>();

        if (uiImage == null)
        {
            Debug.LogError("No Image component found on this GameObject!");
            return;
        }

        // Assign the sprite to the Image component
        if (imageToShow != null)
        {
            uiImage.sprite = imageToShow;
            uiImage.enabled = true; // Ensure the image is enabled
        }
        else
        {
            Debug.LogError("No image has been assigned to display.");
        }
    }
}

