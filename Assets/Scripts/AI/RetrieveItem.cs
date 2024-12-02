using System.Collections;
using UnityEngine;

/**

    Automatically tries to restore items dropped from the character's dropped script

**/
[CreateAssetMenu(fileName = "RetrieveItem", menuName = "Behaviours/Movement/RetrieveItem", order = 0)]
public class RetrieveItem : AIBehaviour{
    public static string itemPosTag = "RetrieveItem.ItemLocalPos";
    public static string itemRotTag = "RetrieveItem.ItemLocalRot";
    public static string itemTag = "RetrieveItem.Item";
    public static string waitTag = "RetrieveItem.Wait";
    public enum WaitStatus {
        Waiting,
        Waited,
        None,
    }
    
    public override void InitializeLocalData(GeneralAI ai){
        var droppable = ai.GetComponent<Droppable>();
        var pos = droppable.item.transform.localPosition;
        var rot = droppable.item.transform.localRotation;
        ai.data[itemPosTag] = pos;
        ai.data[itemRotTag] = rot;
        ai.data[itemTag] = droppable.item;
        ai.data[waitTag] = false;
        //Whenever an item is dropped, start the timer
        ai.droppable.DroppedItem += (GameObject item) => {
            ai.StartCoroutine(Wait(ai));
        };
    }

    public float delay = 4f;

    public float MoveSpeed = 10f;
    public float Acceleration = 3f;

    public float PickupRange = 2f;

    public override bool CanStart(GeneralAI ai){
        if(ai.GetComponent<Droppable>().item == null && (bool)ai.data[waitTag] == false){
            return true;
        }
        return false;
    }
    private IEnumerator Wait(GeneralAI ai){
        ai.data[waitTag] = true;
        yield return new WaitForSeconds(delay);
        ai.data[waitTag] = false;
    }
    public override IEnumerator Behaviour(GeneralAI ai){
        var item = ai.data[itemTag] as GameObject;

        Vector3 difference = item.transform.position - ai.transform.position;
        float distance2 = difference.sqrMagnitude;
        //Move towards the Item
        if (distance2 > PickupRange * PickupRange){
            var movement = ai.GetComponent<CharacterMovement>();
            movement.moveSpeed = MoveSpeed;
            movement.acceleration = Acceleration;
            movement.Move(difference);
            movement.look_direction = difference;

        } //Pickup the item since we are close enough
        else{
            //What to do when we actually pick it up...
            var health =  ai.GetComponent<Health>();
            item.layer = LayerMask.NameToLayer("Enemy");
            //Reset the item's transform
            item.transform.SetParent(ai.transform);
            item.transform.localPosition = (Vector3)ai.data[itemPosTag];
            item.transform.localRotation = (Quaternion)ai.data[itemRotTag];
            Destroy(item.GetComponent<Rigidbody>());
            Destroy(item.GetComponent<Throwable>());

            //Give the item back to the droppable script
            var droppable = ai.GetComponent<Droppable>();
            if (droppable.item != null) droppable.Drop();
            droppable.item = item;
        }

        yield return null;
    }

}

