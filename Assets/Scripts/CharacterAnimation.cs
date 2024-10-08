using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private CharacterMovement charMove;
    private CustomPhysicsBody cusPhysBod;
    private Animator charAnimator;
    private bool walking;
    private bool running;
    private bool jumping;


    [SerializeField]
    private float minWalkingSpeed = 1f;
    [SerializeField]
    private float minRunningSpeed = 2f;


    private float currSpeed;

    // Start is called before the first frame update
    void Start()
    {
        

        charMove = GetComponent<CharacterMovement>();
        cusPhysBod = GetComponent<CustomPhysicsBody>();
        charAnimator = GetComponentInChildren<Animator>();

        minRunningSpeed = charMove.moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        string debug = "null";

        float currSpeed = charMove.GetHorizontalVelocity().magnitude;

        if (charAnimator != null)
        {
            if (currSpeed > minRunningSpeed)
            {
                charAnimator.SetBool("isRunning", true);
                //charAnimator.SetBool("isWalking", false);
                debug = "running";
            }
            else if (currSpeed > minWalkingSpeed)
            {
                charAnimator.SetBool("isWalking", true);
                //charAnimator.SetBool("isRunning", false);
                debug = "walking";
            }
            else
            {
                charAnimator.SetBool("isRunning", false);
                charAnimator.SetBool("isWalking", false);
                debug = "idle";
            }
        }

        Debug.Log(debug);
        
    }
}
