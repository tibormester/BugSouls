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
        StartCoroutine(ChangeAnimation()); //start with boss entry first
    }

    IEnumerator ChangeAnimation()
    {
        while (true)
        {
            //only the attacks, not the boss entry or die animation
            int randomAnim = Random.Range(0, 5);

            // set to a random attack
            vineBossanimator.SetInteger("attackselector", randomAnim);

            // Wait for a frame to allow the Animator to process the change
            yield return null;

            // resetting to some number out of bounds so we dont do any weird attack
            vineBossanimator.SetInteger("attackselector", -1);

            yield return new WaitForSeconds(changeInterval);
        }
    }
}
