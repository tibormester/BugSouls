using UnityEngine;

public class GrappleScript : MonoBehaviour
{
    private Rigidbody rb;
    private CustomPhysicsBody pb;
    private bool grappling = false;
    private Vector3 hookedOffset;
    private Transform hookedObject;
    public float grappleStrength = 5f;
    public float grappleDistance = 1.5f;
    public float grappleMaximum = 20f;
    
    public int grappleTicks = 0;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pb = GetComponent<CustomPhysicsBody>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if(Input.GetButtonDown("Grapple")){
            LaunchGrappleHook();
        }
        if(Input.GetButtonDown("Jump")){
            grappling = false;
        }
    }
    void FixedUpdate(){
        if(grappling){
            ApplyTensionForce();
        }
    }

    void LaunchGrappleHook(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Cast ray from camera, can also use cam.transform.forward for a different direction
        RaycastHit hit;
        // Raycast using the mask to ignore the player's layer
        if (Physics.Raycast(ray, out hit, grappleMaximum,~LayerMask.NameToLayer("Player")))
        {
            hookedObject = hit.collider.gameObject.transform;
            hookedOffset = hookedObject.InverseTransformPoint(hit.point);
            grappling = true;
            rb.AddForce(rb.transform.up * 2f, ForceMode.VelocityChange);
            grappleTicks = 0;
        }
    }

    void ApplyTensionForce(){
        grappleTicks += 1;
        
        Vector3 difference = hookedObject.TransformPoint(hookedOffset) - transform.position;
        difference = difference - grappleDistance*difference.normalized; //Stop a little away from the surface
        Debug.DrawLine(transform.position, transform.position + difference, Color.red);
        rb.AddForce(difference.normalized * grappleStrength, ForceMode.Acceleration);
        if (grappleTicks > 20 && pb.IsGrounded()){
            grappling = false;
        }
    }
}
