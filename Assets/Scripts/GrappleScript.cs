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
    public Throwable currentHeld;
    public Weapon currentWeapon;
    
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
        //Create the web
        GameObject newWeb = Instantiate(webPrototype, transform);
        launching = true;

        //Get the initial direction vector
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Cast ray from camera, can also use cam.transform.forward for a different direction
        RaycastHit hit;
        //Ray cast to see if we are looking at something and then construct a ray from the player to that
        if (Physics.Raycast(ray, out hit, maxLaunchTicks * LaunchSpeed, ~LayerMask.NameToLayer("Player"))){
            ray.direction = (hit.point - transform.position).normalized;
            ray.origin = transform.position;
        } else{ //Change the ray to point at the maximum launch distance
            ray.direction = (ray.GetPoint(LaunchSpeed * maxLaunchTicks) - transform.position).normalized;
            ray.origin = transform.position;
        }

        //TODO: initialize local variables
        float distance = 0f;
        Vector3 targetLocation = ray.origin;
        float difference;
        int launchTicks = 0;
        
        //Initial web orientation
        targetLocation = ray.origin + distance * ray.direction; 
        newWeb.transform.LookAt(targetLocation);
        difference = (targetLocation- transform.position).magnitude;
        newWeb.transform.localScale = new Vector3(1,1, difference);

        //Each tick, check the ray cast towards the target location for the object 
        while(launchTicks < maxLaunchTicks){
            // Raycast from the current target Location out by the launch speed to the new target location
            if (Physics.Raycast(targetLocation, ray.direction, out hit, LaunchSpeed, ~LayerMask.NameToLayer("Player"))){
                //Check what we hit, if its the terrain start grappling to it
                if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Terrain")){ 
                    if(currentWeb != null){
                        Destroy(currentWeb);
                    }
                    //Update the web to go to the hit location
                    newWeb.transform.LookAt(hit.point);
                    difference = (hit.point- transform.position).magnitude;
                    newWeb.transform.localScale = new Vector3(1,1, difference);
                    //Store the hit location
                    currentWeb = newWeb;
                    hookedObject = hit.collider.gameObject.transform;
                    hookedOffset = hookedObject.InverseTransformPoint(hit.point);
                    //Start the grappling with a tiny hop
                    grappling = true;
                    rb.AddForce(rb.transform.up * 4f, ForceMode.VelocityChange);
                    grappleTicks = 0;
                    //We are finished so return early
                    launching = false;
                    yield break;
                }//If its a wepon start grabbing it and bring it to the hand
                else if(hit.rigidbody.gameObject.GetComponent<Weapon>() != null){
                    //Update the web to go to the hit location
                    newWeb.transform.LookAt(hit.point);
                    difference = (hit.point- transform.position).magnitude;
                    newWeb.transform.localScale = new Vector3(1,1, difference);
                    //We are finished so return early
                    launching = false;
                    yield break;
                }//If its a throwable bring it to the throwable location 
                else if(hit.rigidbody.gameObject.GetComponent<Throwable>() != null){
                    //Update the web to go to the hit location
                    newWeb.transform.LookAt(hit.point);
                    difference = (hit.point- transform.position).magnitude;
                    newWeb.transform.localScale = new Vector3(1,1, difference);
                    //We are finished so return early
                    launching = false;
                    yield break;
                } //Otherwise it was something that we cannot travel to and we need to abort the launch
                else {
                    Destroy(newWeb);
                    launching = false;
                    yield break;
                }
            } //If we didnt hit anything keep moving the target location forward
            else{ 
                targetLocation += ray.direction * LaunchSpeed;
                launchTicks++;
            }
            //Reorientate the web from the players new position to the new target location
            newWeb.transform.LookAt(targetLocation);
            difference = (targetLocation- transform.position).magnitude;
            newWeb.transform.localScale = new Vector3(1,1, difference);
            yield return null;
        }
        //If the launch process times out, destroy the web
        Destroy(newWeb);
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
