using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditorInternal;
using TMPro;
using System.Linq;
using System.Text.RegularExpressions;


public class CharacterAnimation : MonoBehaviour
{
    private CharacterMovement charMove;
    private CustomPhysicsBody cusPhysBod;
    private Animator charAnimator;



    [SerializeField]
    private float minWalkingSpeed2 = 1f;
    private float minRunningSpeed2 = 2f;


    private GrappleScript gs;

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
    private int expiration = -1;//goes down by one per frame until it is -1
    void Update()
    {    
        //TODO implement an input buffer
        //If the user clicks, if we arent in cooldown, attack
        //If we are expired, reset the combo, otherwise incrmenet the combo
        if(Input.GetKeyDown(KeyCode.Mouse0) && gs.currentWeapon != null){    
            //Reset the combo if it expired or u reach the max
            if(expiration < 0 || combo > 3){
                combo = 1;
            }//If we aren't stuck in an animation, it should play the next one
            if (ChangeAnimation("1handed combo " + combo)){
                gs.currentWeapon.ToggleActive(true);
                combo += 1;
                //Set the expiration and cooldown timers, maybe add code so this is unique per attack
                expiration = 500;
                return;
            }
        } 
        expiration = expiration < 0 ? -1 : expiration - 1;
        

        var horizontalVelocity = charMove.GetHorizontalVelocity();
        var verticalVelocity = Vector3.Dot(GetComponent<Rigidbody>().velocity, charMove.transform.up);

        float angle = Vector3.SignedAngle(horizontalVelocity, transform.forward, transform.up);
        
        bool grounded = cusPhysBod.IsGrounded();
        bool jumping = verticalVelocity > 3f;

        Debug.Log("angle from straight: " + angle);
        
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
    public static string[] nonCancellable = new string[] {"1handed combo 1", "1handed combo 2","1handed combo 3"};

    public bool ChangeAnimation(string animationName){
        if( currentAnimation == animationName){
            return false;
        }
        if (nonCancellable.Contains(currentAnimation)){
            //non cancellables can only be canceled by other non cancellables after they have played most of their animaiton
            var state = charAnimator.GetCurrentAnimatorStateInfo(0);
            if( nonCancellable.Contains(animationName)){
                if (state.normalizedTime < 0.9){
                    return false;
                }
            } else{
                if (state.normalizedTime < 0.9){
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
