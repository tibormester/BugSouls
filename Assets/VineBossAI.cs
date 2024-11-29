using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineBossAI : MonoBehaviour
{
   private Animator vineBossanimator;
    public float changeInterval = 5f;

    void Start()
    {
        vineBossanimator = GetComponent<Animator>();
        StartCoroutine(ChangeAnimation());
    }

    IEnumerator ChangeAnimation()
    {
        while (true)
        {
            // Generate a random integer between 0 and 4 (inclusive)
            int randomAnim = Random.Range(0, 5);

            // Set the AnimIndex parameter to trigger the transition
            vineBossanimator.SetInteger("attackselector", randomAnim);

            // Wait for a frame to allow the Animator to process the change
            yield return null;

            // Reset the parameter to prevent unintended transitions
            vineBossanimator.SetInteger("attackselector", -1);

            // Wait for the specified interval before changing again
            yield return new WaitForSeconds(changeInterval);
        }
    }
}
