using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationController : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log(animator);
        
    }

    // Update is called once per frame
    void Update()
    {
        bool isWalking = animator.GetBool("isWalking");
        bool isRunning = animator.GetBool("isRunning");

        // does not work yet, need to use blend trees
        bool isJumping = animator.GetBool("isJumping");

        bool jumpUp = Input.GetKey("space");
        bool goForward = Input.GetKey("w");
        bool runButton = Input.GetKey("left shift");

        if (!isWalking && goForward) {
            animator.SetBool("isWalking", true);
        }
        
        if (!isRunning && (goForward && runButton)) {
            animator.SetBool("isRunning", true);
        }

        //not working
        if (!isJumping && jumpUp) {
            animator.SetBool ("isJumping", true);
        }

        if (isWalking && !goForward) {
            animator.SetBool("isWalking", false);
        } 
        
         if (isRunning && (!goForward || !runButton)) {
            animator.SetBool("isRunning", false);
        }

        //not working
        if (isJumping && !jumpUp) {
            animator.SetBool("isJumping", false);
        }
    }
}
