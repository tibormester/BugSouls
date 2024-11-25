using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditorInternal;
using TMPro;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System;


public class CharacterAnimation : MonoBehaviour
{
    private CharacterMovement charMove;
    private CustomPhysicsBody cusPhysBod;
    private Animator charAnimator;



    [SerializeField]
    private float minWalkingSpeed2 = 1f;
    private float minRunningSpeed2 = 2f;


    private GrappleScript gs;

    [SerializeField]
    private const string oneHandAttackStringPrefix = "1handed combo ";
    private float[] oneHandAttackTimes = new float[] {1.3f, 1.717f, 2.417f};

    // Start is called before the first frame update
    void Start()
    {
        charMove = GetComponent<CharacterMovement>();
        cusPhysBod = GetComponent<CustomPhysicsBody>();
        charAnimator = GetComponentInChildren<Animator>();

        minRunningSpeed2 = (charMove.moveSpeed * 1.1f) * (charMove.moveSpeed * 1.1f);
        gs = GetComponent<GrappleScript>();

    }


    // Update is called once per fram
    private int combo = 1;//1,2,3
    private float expiration = -1f; //counts down seconds until it is below zero
    private bool comboQueued = false;
    void Update()
    {    
        //TODO implement an input buffer
        //If the user clicks, if we arent in cooldown, attack
        //If we are expired, reset the combo, otherwise incrmenet the combo
        if (gs.currentWeapon != null)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //If we aren't stuck in an animation, it should play the next one
                if (ChangeAnimation(oneHandAttackStringPrefix + combo))
                {
                    gs.currentWeapon.ToggleActive(true);
                    expiration = oneHandAttackTimes[combo - 1];
                    combo = combo < 3 ? combo + 1 : 1;
                    //Set the expiration and cooldown timers, maybe add code so this is unique per attack
                    comboQueued = false;
                    return;
                }
                else if (expiration > 0)
                {
                    comboQueued = true;
                }
            }
            else if (comboQueued && expiration < 0)
            {
                if (ChangeAnimation(oneHandAttackStringPrefix + combo))
                {
                    Debug.Log("Queued attacking working");
                    gs.currentWeapon.ToggleActive(true);
                    combo = combo < 3 ? combo + 1 : 1;
                    //Set the expiration and cooldown timers, maybe add code so this is unique per attack
                    expiration = 1f;
                    comboQueued = false;
                    return;
                }
                else
                {
                    Debug.Log("Queued attack can't yet");
                }

            }
        }
        
         
        expiration = expiration < 0 ? -1f : expiration - Time.deltaTime;
        combo = expiration < 0 && !comboQueued ? 1 : combo;

        Debug.Log("Combo: " + combo + ", Expiration: " + expiration + " , Combo Queued: " + comboQueued + ", Anim time: " + oneHandAttackTimes[combo - 1]);
        

        var horizontalVelocity = charMove.GetHorizontalVelocity();
        var verticalVelocity = Vector3.Dot(GetComponent<Rigidbody>().velocity, charMove.transform.up);

        float angle = Vector3.SignedAngle(horizontalVelocity, transform.forward, transform.up);
        
        bool grounded = cusPhysBod.IsGrounded();
        bool jumping = verticalVelocity > 3f;

        // Debug.Log("angle from straight: " + angle);
        
        var speed2 = horizontalVelocity.sqrMagnitude;
        //Are we on the ground?
        if(grounded){
            //Are we trying to jump?
            if(jumping){
                ChangeAnimation("Jump");
            //Are we fast enough to run?
            } else if (speed2 > minRunningSpeed2) {
                ChangeAnimation("Run");
            //Are we fast enough to walk
            } else if (speed2 > minWalkingSpeed2){
                //Which direction are we strafing?
                if (angle < -120 || angle > 120){
                ChangeAnimation("Back Strafe");
                } else if ( angle < -30) {
                    ChangeAnimation("Left Strafe");
                } else if (angle > 30){
                    ChangeAnimation("Right Strafe");
                } else {
                    ChangeAnimation("Walk Forward");
                }
            } else {
                ChangeAnimation("Idle");
            }
        } else {
            ChangeAnimation("Falling");
        }
        
    }

    //Swaps into the new animation only if its not already playing and if its not one of the unCancellable
    public string currentAnimation;
    public static string[] nonCancellable = new string[] {"1handed combo 1", "1handed combo 2","1handed combo 3"};

    public bool ChangeAnimation(string animationName){
        if( currentAnimation == animationName){
            return false;
        }
        if (nonCancellable.Contains(currentAnimation)){
            //non cancellables can only be canceled by other non cancellables after they have played most of their animaiton
            var state = charAnimator.GetCurrentAnimatorStateInfo(0);
            if( nonCancellable.Contains(animationName)){
                if (state.normalizedTime < 0.9f){
                    return false;
                }
            } else{
                if (state.normalizedTime < 0.9f){
                    return false;
                }
            }
            //We are cancelling a weapon animation so turn off the colliders
            gs.currentWeapon.ToggleActive(false);
        }
        charAnimator.CrossFade(animationName, 0.1f,0);
        currentAnimation = animationName;
        return true;
    }
    
}
