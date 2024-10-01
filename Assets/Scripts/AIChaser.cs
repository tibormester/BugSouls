using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NavMeshAgentChaser : MonoBehaviour
{
    
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float speedMultiplier = 2f;     
    private CharacterMovement charMovement; //The target's movement script to call movement functions on
    [SerializeField]
    private float minDistance = 2f, maxDistance = 3f;              //Min distance to move towards player


    void Start()
    {
        charMovement = GetComponent<CharacterMovement>();
    }

    void Update()
    {
        if (target != null)
        {
            if (Vector3.Distance(target.position, transform.position) > maxDistance)
            {
                charMovement.Move((target.position - transform.position).normalized * speedMultiplier);
            }
            else if (Vector3.Distance(target.position, transform.position) < minDistance)
            {
                charMovement.Move((transform.position - target.position).normalized * speedMultiplier);
            }
        }
    }

}
