using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NavMeshAgentChaser : MonoBehaviour
{
    
    [SerializeField]
    [Tooltip("The target transform the AI is chasing")]
    private Transform target;

    [SerializeField]
    [Tooltip("The minimum and maximum distance the AI wants to stay near the target")]
    private float minEngageDistance = 2f, maxEngageDistance = 3f;

    [SerializeField]
    [Tooltip("The acceleration multiplier the AI uses to back up from the target when too close")]
    private float backupAccelMultiplier = 2f;

    [SerializeField]
    [Tooltip("The maximum distance the AI has to be to the target in order to chase it")]
    private float maxChaseDistance = 30f;

    [SerializeField]
    [Tooltip("If stuck while chasing target, this is the amount of time in seconds after which the AI will jump")]
    private float maxStuckTime = 4f;

    [SerializeField]
    [Tooltip("This is the distance the AI has to move while chasing the player to be considered not stuck")]
    private float maxStuckDistance = 1f;

    private float originalAccel;
    private float stuckTime = 0f;                       //Elapsed time "stuck"
    private Vector3 stuckPos;                           //Position to test if stuck
    private CharacterMovement charMovement;             //The AI's movement script to call movement functions on
    private CustomPhysicsBody physicsBody;              //The AI's physics script to find gravity


    void Start()
    {
        charMovement = GetComponent<CharacterMovement>();
        physicsBody = GetComponent<CustomPhysicsBody>();

        originalAccel = charMovement.acceleration;
        stuckPos = transform.position;
    }

    void Update()
    {
        if (target != null)
        {
            if (Vector3.Distance(target.position, transform.position) < maxChaseDistance)
            {
                charMovement.acceleration = originalAccel;
                

                if (Vector3.Distance(target.position, transform.position) > maxEngageDistance)
                {
                    charMovement.Move((target.position - transform.position).normalized);
                    charMovement.look_direction = target.position - transform.position;
                    
                    if (Vector3.Distance(stuckPos, transform.position) < maxStuckDistance)
                    {
                        stuckTime += Time.deltaTime;

                        if (stuckTime > maxStuckTime)
                        {
                            charMovement.Jump();
                        }
                    }
                    else
                    {
                        stuckPos = transform.position;
                        stuckTime = 0f;
                    }
                }
                else
                {
                    stuckTime = 0f;
                    
                    charMovement.look_direction = target.position - transform.position;

                    if (Vector3.Distance(target.position, transform.position) < minEngageDistance)
                    {
                        charMovement.acceleration = originalAccel * backupAccelMultiplier;
                        charMovement.Move((transform.position - target.position).normalized);
                    }
                }
            }            
        }
    }

}
