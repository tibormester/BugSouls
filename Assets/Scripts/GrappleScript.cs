using System.Collections;
using Unity.VisualScripting;
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
    public GameObject webPrototype;
    public GameObject currentWeb;
    
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
            if(!launching){//Only launch one web at a time
                StartCoroutine(LaunchGrappleHook());
            }
        }
        if(Input.GetButtonDown("Jump")){
            //Destroy the current web
            if(currentWeb){
                Destroy(currentWeb);
                currentWeb = null;
            }
            grappling = false;
        }
    }
    void FixedUpdate(){
        //If we have a web, pull us towards it
        if(grappling){
            ApplyTensionForce();
        }
    }

    public int LaunchSpeed = 15;
    public int maxLaunchTicks = 120;
    public bool launching = false;
    IEnumerator LaunchGrappleHook(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Cast ray from camera, can also use cam.transform.forward for a different direction

        //TODO: Refactor so that we ray cast each frame... ?

        RaycastHit hit;
        // Raycast using the mask to ignore the player's layer
        if (Physics.Raycast(ray, out hit, grappleMaximum,~LayerMask.NameToLayer("Player")))
        {
            //Spawn a web prototype, start a corountine for a few frames where it extends towards the contact point
            //If there is an existing web, we want to delete it
            //If the player moves, we gotta keep reorientating and updating the length, lets spawn this fromm the center of the player?
            //After it full extends, stat grapping
            GameObject newWeb = Instantiate(webPrototype, transform);
            launching = true;
            
            newWeb.transform.LookAt(hit.point);
            float difference = (hit.point - transform.position).magnitude;
            float traveled = newWeb.transform.localScale.z;
            int launchTicks = 0;
            while(traveled < difference -0.2f){
                traveled = Mathf.Min(traveled + LaunchSpeed, difference);
                newWeb.transform.localScale = new Vector3(1,1,traveled);

                yield return null;
                newWeb.transform.LookAt(hit.point);
                difference = (hit.point - transform.position).magnitude;
                traveled = newWeb.transform.localScale.z;
                launchTicks++;
                //If we launch the web and move away fast enough so that it cant land at the hit target in time, then return null
                if(launchTicks > maxLaunchTicks){
                    launching = false;
                    yield break;
                }
            }

            if(currentWeb != null){
                Destroy(currentWeb);
            }
            currentWeb = newWeb;

            //TODO: Implement some code that detects if its a terrain with a grappleable component then start grappling, otherwise if its an object with the throwable component,
            //Start pulling it in with another coroutine. If that throwable is a weapon than equip it, if its not than let it be thrown...

            hookedObject = hit.collider.gameObject.transform;
            hookedOffset = hookedObject.InverseTransformPoint(hit.point);
            grappling = true;
            rb.AddForce(rb.transform.up * 2f, ForceMode.VelocityChange);
            grappleTicks = 0;
            
        }
        launching = false;
        yield return null;
    }

    void ApplyTensionForce(){
        grappleTicks += 1;
        
        Vector3 difference = hookedObject.TransformPoint(hookedOffset) - transform.position;
        difference = difference - grappleDistance*difference.normalized; //Stop a little away from the surface
        Debug.DrawLine(transform.position, transform.position + difference, Color.red);
        rb.AddForce(difference.normalized * grappleStrength, ForceMode.Acceleration);

        //Update the spawned grapple web
        //If the player moves, we gotta keep reorientating and updating the length, lets spawn this fromm the center of the player?
        var hitpoint = hookedObject.TransformPoint(hookedOffset);
        currentWeb.transform.LookAt(hitpoint); //Orientate

        float diff = (hitpoint - transform.position).magnitude;
        currentWeb.transform.localScale = new Vector3(1,1,diff);//Scale

        if (grappleTicks > 20 && pb.IsGrounded()){
            grappling = false;
            grappleTicks = 0;
            //Delete the spawned web
            Destroy(currentWeb);
            currentWeb = null;
        }
    }
}
