using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FleeBehaviour", menuName = "Behaviours/Movement/Flee", order = 2)]
public class FleeBehaviour : AIBehaviour {
    public static string fleeingTag = "FleeBehaviour.fleeing";

    public float FleeAcceleration = 25f;
    public float FleeMoveSpeed = 18f;

    //Some other code needs to toggle this flag
    public override void InitializeLocalData(GeneralAI ai){
        ai.data[fleeingTag] = false;
    }
    public override bool CanStart(GeneralAI ai) {
        if(ai.data[fleeingTag] is bool &&  (bool)ai.data[fleeingTag] == true){
            return true;
        } else{
            return false;
        }
    }
    public override IEnumerator Behaviour(GeneralAI ai){
        Vector3 difference = (ai.transform.position - ai.target.position).normalized;

        ai.movement.acceleration = FleeAcceleration;
        ai.movement.moveSpeed = FleeMoveSpeed;

        ai.movement.Move(difference);
        ai.movement.look_direction = difference;
        yield return new WaitForFixedUpdate();
    }
}