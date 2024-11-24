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
    private int isJumping = -1;

    private int combo = 0;
    private int cooldown = -1;
    private int expiration = -1;
    void Update()
    {    
        //TODO implement an input buffer
        //If the user clicks, if we arent in cooldown, attack
        //If we are expired, reset the combo, otherwise incrmenet the combo
        if(Input.GetKeyDown(KeyCode.Mouse0)){
            
            if(cooldown < 0){
                if(expiration > 50 || combo > 2){
                    combo = 0;
                }
                charAnimator.SetBool("attacking", true);
                charAnimator.SetInteger("combo", combo);
                combo += 1;

                expiration = 0;
                cooldown = 15;
            } else {
                charAnimator.SetBool("attacking", false);
                expiration++;
                cooldown--;
            }
        } else{
            charAnimator.SetBool("attacking", false);
            expiration++;
            cooldown--;
        }


        //COuld be optimized by cachine some values and only doing the vector operaitons a single time instead of for each direction

        string debug = "null";
        currSpeed = charMove.GetHorizontalVelocity().magnitude;
        float angle = Vector3.Angle(charMove.GetHorizontalVelocity(), charMove.look_direction);

        //Detect if we are moving up and try to jump if we havent already done so in the past few frames
        var verticalVelocity = Vector3.Dot(GetComponent<Rigidbody>().velocity, charMove.transform.up);
        if (verticalVelocity > 1f && isJumping == -1){
            charAnimator.SetBool("isJumping", true);
            isJumping = 0;
        } else{
            charAnimator.SetBool("isJumping", false);
            if(isJumping > 0){
                isJumping++;
            }
            if(isJumping > 80){
                isJumping = -1;
            }
        }
        backwards = angle > 90f + 30f;

        Debug.Log("angle to right: " + Vector3.Angle(charMove.GetHorizontalVelocity(), transform.right));
        strafeRight = Vector3.Angle(charMove.GetHorizontalVelocity(), transform.right) < 30f;
        strafeLeft = Vector3.Angle(charMove.GetHorizontalVelocity(), -transform.right) < 30f;
        if(backwards){
            charAnimator.SetInteger("walkDirection", 2);
        }
        else if (strafeRight){
            charAnimator.SetInteger("walkDirection", 1);
        }
        else if (strafeLeft){
            charAnimator.SetInteger("walkDirection", -1);
        }else{
            charAnimator.SetInteger("walkDirection", 0);
        }

        Debug.Log("Strafing: left: " + strafeLeft + ", Right: " + strafeRight);
        if (charAnimator != null)
        {
            if (currSpeed > minRunningSpeed){
                charAnimator.SetBool("isRunning", true);
                charAnimator.SetBool("isWalking", false);

                debug = "running";
            }else if (currSpeed > minWalkingSpeed){
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
        Debug.Log("direction: " + charAnimator.GetInteger("walkDirection") + " and "+ debug + " at speed of " + currSpeed);
        
    }
    
}
