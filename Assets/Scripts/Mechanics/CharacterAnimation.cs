using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditorInternal;
using TMPro;


public class CharacterAnimation : MonoBehaviour
{
    private CharacterMovement charMove;
    private CustomPhysicsBody cusPhysBod;
    private Animator charAnimator;

    private float currSpeed;
    private bool backwards;
    private bool strafeLeft;
    private bool strafeRight;


    [SerializeField]
    private float minWalkingSpeed = 1f;
    private float minRunningSpeed = 2f;



    // Start is called before the first frame update
    void Start()
    {
        charMove = GetComponent<CharacterMovement>();
        cusPhysBod = GetComponent<CustomPhysicsBody>();
        charAnimator = GetComponentInChildren<Animator>();

        minRunningSpeed = charMove.moveSpeed * 1.1f;
        backwards = false;
        currSpeed = 0f;
    }


    // Update is called once per frame
    void Update()
    {
        //COuld be optimized by cachine some values and only doing the vector operaitons a single time instead of for each direction

        string debug = "null";
        float angle = Vector3.Angle(charMove.GetHorizontalVelocity(), charMove.look_direction);

        currSpeed = charMove.GetHorizontalVelocity().magnitude;

        Debug.Log("Horizontal velocity: " + charMove.GetHorizontalVelocity().ToString());

        var verticalVelocity = Vector3.Dot(GetComponent<Rigidbody>().velocity, charMove.transform.up);
        if (verticalVelocity > 1f){
            charAnimator.SetBool("isJumping", true);
        } else{
            charAnimator.SetBool("isJumping", false);
        }
        backwards = angle > 90f + 30f;


        strafeRight = Vector3.Angle(charMove.GetHorizontalVelocity(), transform.right) < 30f;
        strafeLeft = Vector3.Angle(charMove.GetHorizontalVelocity(), -transform.right) < 30f;
        if(backwards){
            charAnimator.SetInteger("direction", 2);
        }
        else if (strafeRight){
            charAnimator.SetInteger("direction", 1);
        }
        else if (strafeLeft){
            charAnimator.SetInteger("direction", -1);
        }else{
            charAnimator.SetInteger("direction", 0);
        }

        Debug.Log("Strafing: left: " + strafeLeft + ", Right: " + strafeRight);
        if (charAnimator != null)
        {
            if (currSpeed > minRunningSpeed){
                charAnimator.SetBool("isRunning", true);
                charAnimator.SetBool("isWalking", false);

                debug = "running";
            }
            else if (currSpeed > minWalkingSpeed){
                charAnimator.SetBool("isWalking", true);
                charAnimator.SetBool("isRunning", false);

                debug = "walking";
            }else{
                charAnimator.SetBool("isRunning", false);
                charAnimator.SetBool("isWalking", false);

                debug = "idle";
            }
            
        }
        if (!cusPhysBod.IsGrounded()){
            charAnimator.SetBool("isGrounded", false);
        } else{
            charAnimator.SetBool("isGrounded", true);
        }
        Debug.Log("Min Running Speed: " + minRunningSpeed + " and "+ debug + " at speed of " + currSpeed);
        
    }
    
}
