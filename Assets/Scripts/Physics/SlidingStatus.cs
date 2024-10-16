using UnityEngine;

public class SlidingStatus : Status{

    public Rigidbody rb;
    public CustomPhysicsBody pb;
    
    public SlidingStatus(GameObject obj){
        rb = obj.GetComponent<Rigidbody>();
        pb = obj.GetComponent<CustomPhysicsBody>();
    }
    public void Tick(){
        Vector3 horizontal = rb.velocity - Vector3.Dot(rb.velocity, -pb.groundNormal) * -pb.groundNormal;
        horizontal += pb.groundNormal * 0.1f;
        rb.AddForce(horizontal * 1.5f, ForceMode.Acceleration);
    }
    public void Start(){

    }
    public void Stop(){

    }

}

