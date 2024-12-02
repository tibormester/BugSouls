using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StrafeBehaviour", menuName = "Behaviours/Movement/Strafe", order = 1)]
public class StrafeBehaviour : AIBehaviour {
    public static string directionTag = "StrafeBehaviour.direction";

    public float StrafeMoveSpeed = 16f;
    public float StrafeAcceleration = 7f;

    public override void InitializeLocalData(GeneralAI ai){
        ai.data[directionTag] = -1f;
    }

    public override bool CanStart(GeneralAI ai){
        if (ai.target == null ){
            return false;
        }   
        return true;
    }

    public override IEnumerator Behaviour(GeneralAI ai){
        //Look at the player
        Vector3 diff = ai.target.transform.position - ai.transform.position;
        ai.movement.look_direction = diff;

        float direction = (float)ai.data[directionTag]; //TODO: Change this to TryGetOrDefault 1f
        ai.movement.acceleration = StrafeAcceleration;
        ai.movement.moveSpeed = StrafeMoveSpeed;
        ai.movement.Move(ai.transform.right * direction);
        //Change strafe direction 1/100 times (called 20 times a second so Expected once every 5 seconds)
        if(Random.Range(0,100) >= 99){
            direction *= -1f;
        }
        ai.data[directionTag] = direction;
        yield return new WaitForFixedUpdate();
    }
}