using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowImageOnTouch : MonoBehaviour
{
    [Header("UI Image to Display")]
    public GameObject imageToShow; // Assign your UI Image GameObject here

    [Header("Player Settings")]
    public string playerTag = "Player"; // Tag assigned to your player object

    private bool hasDisplayed = false; // Prevents multiple triggers

    private void OnTriggerEnter(Collider other)
    {
        if (!hasDisplayed && other.CompareTag(playerTag))
        {
            hasDisplayed = true;
            StartCoroutine(DisplayImage());
        }
    }

    private IEnumerator DisplayImage()
    {
        imageToShow.SetActive(true); // Show the image
        yield return new WaitForSeconds(3f); // Wait for 3 seconds
        imageToShow.SetActive(false); // Hide the image
    }
}
