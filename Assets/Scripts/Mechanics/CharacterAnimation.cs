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



    // Start is called before the first frame update
    void Start()
    {
        charMove = GetComponent<CharacterMovement>();
        cusPhysBod = GetComponent<CustomPhysicsBody>();
        charAnimator = GetComponentInChildren<Animator>();

        minRunningSpeed2 = (charMove.moveSpeed * 1.1f) * (charMove.moveSpeed * 1.1f);
    }


    // Update is called once per fram
    private int combo = 1;//1,2,3
    private int cooldown = -1;//goes down by one per frame until it is -1
    private int expiration = -1;//goes down by one per frame until it is -1
    void Update()
    {    
        //TODO implement an input buffer
        //If the user clicks, if we arent in cooldown, attack
        //If we are expired, reset the combo, otherwise incrmenet the combo
        if(Input.GetKeyDown(KeyCode.Mouse0)){    
            if(cooldown < 0){
                //Reset the combo if it expired or u reach the max
                if(expiration < 0 || combo > 3){
                    combo = 1;
                }
                ChangeAnimation("1handed combo " + combo);
                combo += 1;

                //Set the expiration and cooldown timers, maybe add code so this is unique per attack
                expiration = 500;
                cooldown = 200;
                return;
            } 
        } 
        expiration = expiration < 0 ? -1 : expiration - 1;
        cooldown = cooldown < 0 ? -1 : cooldown - 1;
        

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

    public void ChangeAnimation(string animationName){
        if( currentAnimation == animationName){
            return;
        }
        //If its a unCancellable check to see if its almost done before aborting
        if (nonCancellable.Contains(currentAnimation)){
            var state = charAnimator.GetCurrentAnimatorStateInfo(0);
            if (state.normalizedTime < 0.9){
                return;
            }
        }

        charAnimator.Play(animationName);
        currentAnimation = animationName;
    }
    
}
