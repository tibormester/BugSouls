using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour
{
    // Start is called before the first frame update
    private void FixedUpdate() {
        foreach (Data data in contacting.Values){
            Tick(data);
        }
    }

    // Update is called once per frame
    public void OnEnter(CustomPhysicsBody body){
        if(contacting.TryGetValue(body, out Data data)){
            data.ticks = 0;
            return;
        }
        data = new Data(body);
        contacting.Add(body, data);
        if(data.movement){
            data.movement.moveSpeed *= 2;
            data.movement.acceleration *= 2;
        }
    }
    public void OnExit(CustomPhysicsBody body){
        if(contacting.TryGetValue(body, out Data data)){
            if(data.movement){
                data.movement.moveSpeed = data.moveSpeed;
                data.movement.acceleration = data.acceleration;
            }
            contacting.Remove(body);
        }
    }
    private Dictionary<CustomPhysicsBody,Data> contacting = new();
    private void Tick(Data data){
        if(data.movement != null){
            //Character stuff per tick
        }else{
            //Non character stuff per tick
            data.rb.velocity *= 1.03f;
        }
        data.ticks += 1;
        if (data.ticks > 150){
            OnExit(data.body);
        }
    }
    struct Data{
        public CustomPhysicsBody body;
        public Rigidbody rb;
        public CharacterMovement movement;
        public float moveSpeed;
        public float acceleration;
        public int ticks;

        public Data(CustomPhysicsBody b){
            body = b;
            rb = b.gameObject.GetComponent<Rigidbody>();
            movement = b.gameObject.AddComponent<CharacterMovement>();
            if(movement){
                moveSpeed = movement.moveSpeed;
                acceleration = movement.acceleration;
            }else{
                moveSpeed = 0;
                acceleration = 0;
            }
            ticks = 0;
        }
    }
}
