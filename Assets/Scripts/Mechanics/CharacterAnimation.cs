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

    // Start is called before the first frame update
    void Start()
    {
        charMove = GetComponent<CharacterMovement>();
        cusPhysBod = GetComponent<CustomPhysicsBody>();
        charAnimator = GetComponentInChildren<Animator>();

        minRunningSpeed2 = (charMove.moveSpeed * 1.1f) * (charMove.moveSpeed * 1.1f);
        gs = GetComponent<GrappleScript>();

        UpdateAnimData();
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
            string weaponType = gs.currentWeapon.type.ToString();
            //Detect if theres an input or an attack
            if (Input.GetKeyDown(KeyCode.Mouse0) || comboQueued){
                //clear the queue
                comboQueued = false;
                //If the combo is expired restart it
                if(expiration < 0f){
                    combo = 1;
                }
                //If we aren't stuck in an animation, it should play the next one
                if (ChangeAnimation(weaponType + "Combo" + combo)){
                    gs.currentWeapon.ToggleActive(true);
                    if (animationDurations.TryGetValue(weaponType + "Combo" + combo, out expiration)){
                        expiration += 0.75f; //flat padding for maintaining a combo
                    }
                    else{
                        Debug.Log("Couldn't find animaiton expiration: " + weaponType + "Combo" + combo);
                        expiration = 2.75f; //Default combo timer
                    }
                    //Increment the combo
                    combo = combo < 3 ? combo + 1 : 1;
                    comboQueued = false;
                    return;
                //If we couldnt play the animation, queue the next one if we are close to expiring
                }else if (expiration > 0f && expiration < 0.75f){
                    //The expiration < 0.75f is so that we only queue animations near the end of the expiration timer
                    comboQueued = true;
                }
            }

            float debugAnimDuration;
            
            Debug.Log("Combo: " + combo + ", Expiration: " + expiration + " , Combo Queued: " + comboQueued + ", Animation: " + currentAnimation + ", Duration: " + (animationDurations.TryGetValue(currentAnimation, out debugAnimDuration) ? debugAnimDuration : "Unkown"));
        }
        
         
        expiration = expiration < 0 ? -1f : expiration - Time.deltaTime;
        

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
                    ChangeAnimation("Right Strafe");
                } else if (angle > 30){
                    ChangeAnimation("Left Strafe");
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
    //Sets list in "UpdateAnimData" function
    public static List<string> nonCancellable = new List<string>();

    public bool ChangeAnimation(string animationName){
        if( currentAnimation == animationName){
            return false;
        }
        if (nonCancellable.Contains(currentAnimation)){
            //non cancellables can only be canceled by other non cancellables after they have played most of their animaiton
            var state = charAnimator.GetCurrentAnimatorStateInfo(0);
            if( nonCancellable.Contains(animationName)){
                if (state.normalizedTime < 0.75f){
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

    // Updates the dictionary of animation clip times, mainly for attack animations
    private Dictionary<string, float> animationDurations = new Dictionary<string, float>();
    public void UpdateAnimData()
    {
        AnimationClip[] clips = charAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            charAnimator.logWarnings = false;
            float speedMultiplier = charAnimator.GetFloat(clip.name + "Speed");
            charAnimator.logWarnings = true;

            speedMultiplier = speedMultiplier == 0 ? 1f : speedMultiplier;

            animationDurations.Add(clip.name, clip.length / speedMultiplier);

            if (clip.name.Contains("Combo"))
            {
                nonCancellable.Add(clip.name);
            }

            Debug.Log("Animation: " + clip.name + ", Duration: " + (clip.length / speedMultiplier));
        }
    }

}