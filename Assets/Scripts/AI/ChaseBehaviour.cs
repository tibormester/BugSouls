using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChaseBehaviour", menuName = "Behaviours/Movement/Chase", order = 0)]
public class ChaseBehaviour : AIBehaviour{

    public float maxChaseDistance = 100f;
    public float minEngageDistance = 2f, maxEngageDistance = 3f;

    public float MoveSpeed = 8f;
    public float Acceleration = 5f;

    public float BackupMoveSpeed = 16f;
    public float BackupAcceleration = 7f;
    
    // Start is called before the first frame update
    public override bool CanStart(GeneralAI ai){
        if (ai.target == null ){
            return false;
        }   
        var distance2 = (ai.transform.position - ai.target.position).sqrMagnitude;
        //The target is too far or in the sweet spot don't chase
        if(distance2 > maxChaseDistance*maxChaseDistance || (distance2 > minEngageDistance*minEngageDistance && distance2 < maxEngageDistance*maxEngageDistance)){
            return false;
        }
        return true;
    }

    public override IEnumerator Behaviour(GeneralAI ai){
        //Get the distance vector
        Vector3 difference = (ai.target.position - ai.transform.position);
        float distance = difference.magnitude;
        Vector3 normalized = difference / distance;

        yield return new WaitUntil(() => ai.movement != null);
        //Look at the target
        ai.movement.look_direction = difference;
        //If we are far away move forward
        if (distance > maxEngageDistance){
            //Set Forward Movement Stats
            ai.movement.acceleration = Acceleration;
            ai.movement.moveSpeed = MoveSpeed;

            ai.movement.Move(normalized);
        }
        //Otherwise, we are within the min engage distance and we need to backup 
        else{
            ai.movement.acceleration = BackupAcceleration;
            ai.movement.moveSpeed = BackupMoveSpeed;

            ai.movement.Move(-normalized);
        }
        yield return new WaitForFixedUpdate();
    }   


}


