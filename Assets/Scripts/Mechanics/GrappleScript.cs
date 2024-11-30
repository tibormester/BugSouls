using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GrappleScript : MonoBehaviour
{
    private Rigidbody rb;
    private CustomPhysicsBody pb;
    public bool grappling = false;
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
        //Ensure u cant do stuff when u die
        pb.GetComponent<Health>().DeathEvent += () => Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        
        if(Input.GetButtonDown("Grapple")){ //This is E
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
        if (Input.GetKeyDown(KeyCode.Q)){
            if (currentHeld != null){
                //Start a routine to count how long we charge the throw for (and animate it)
                StartCoroutine(ThrowingInput());
            }
        }
    }
    public float maxChargeTime = 2f;
    public Vector3 chargeVector = new Vector3(-0.05f, -0.2f, -0.5f);

    private IEnumerator ThrowingInput(){
        var localPosition = currentHeld.transform.localPosition;
        var elapsed = 0f;
        float percentage = 0f;
        Vector3 originalScale = currentHeld.transform.localScale;
        while(!Input.GetKeyUp(KeyCode.Q)){
            percentage = elapsed / maxChargeTime;
            currentHeld.transform.localPosition = localPosition + (chargeVector * Mathf.Clamp(percentage, 0f, 1.5f));
            
            if (percentage >= 2.0){
                //Ideally id have another animation to inidicate its at max charge percentage, but this should be cool enough
                currentHeld.transform.localScale = originalScale * 0.9f;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        currentHeld.transform.localScale = originalScale;
        Throw(Mathf.Clamp(percentage, 0, 2));
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
                //Check what we hit, if its the terrain start grappling to it (bring player to terrain)
                if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Terrain") || 
                    hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy")){ 
                    if(currentWeb != null){
                        Destroy(currentWeb);
                    }
                    //Update the web to go to the hit location
                    newWeb.transform.LookAt(hit.point);
                    difference = (hit.point- transform.position).magnitude;
                    newWeb.transform.localScale = new Vector3(1,1, difference);
                    //Store the hit location
                    currentWeb = newWeb;
                    hookedObject = hit.rigidbody != null ? hit.rigidbody.transform : hit.collider.gameObject.transform;
                    hookedOffset = hookedObject.InverseTransformPoint(hit.point);
                    //Start the grappling with a tiny hop
                    grappling = true;
                    rb.AddForce(rb.transform.up * 4f, ForceMode.VelocityChange);
                    grappleTicks = 0;
                    //We are finished so return early
                    launching = false;
                    yield break;
                }//If its a weapon or a throwable start grabbing it and bring it to the hand 
                else if(hit.rigidbody != null &&  (
                        hit.rigidbody.gameObject.GetComponent<Weapon>() != null ||
                        hit.rigidbody.gameObject.GetComponent<Throwable>() != null)  ){
                    if(hit.rigidbody.gameObject.GetComponent<Throwable>() != null && currentHeld != null){
                        Throw(0.1f);
                    }
                    //Update the web to go to the hit location
                    newWeb.transform.LookAt(hit.point);
                    difference = (hit.point- transform.position).magnitude;
                    newWeb.transform.localScale = new Vector3(1,1, difference);
                    //Give a little pause for more impact
                    yield return new WaitForSeconds(0.1f);
                    //Call the pickup function
                    StartCoroutine(PickUp(hit.rigidbody.gameObject, newWeb));
                    //We are finished so return early
                    launching = false;
                    yield break;
                } else {
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

        if(hookedObject.gameObject.layer == LayerMask.NameToLayer("Enemy")){
            rb.AddForce(difference.normalized * 2* grappleStrength, ForceMode.Acceleration);
            hookedObject.GetComponent<Rigidbody>()?.AddForce(-difference.normalized *grappleStrength, ForceMode.Acceleration);
        } else{
            rb.AddForce(difference.normalized * grappleStrength, ForceMode.Acceleration);
        }
        //Update the spawned grapple web
        //If the player moves, we gotta keep reorientating and updating the length, lets spawn this fromm the center of the player?
        var hitpoint = hookedObject.TransformPoint(hookedOffset);
        currentWeb.transform.LookAt(hitpoint); //Orientate

        float diff = (hitpoint - transform.position).magnitude;
        currentWeb.transform.localScale = new Vector3(1,1,diff);//Scale

        //pb.Reorientate();

        //Unstick when on ground
        if (grappleTicks > 20 && pb.IsGrounded()){
            grappling = false;
            grappleTicks = 0;
            //Delete the spawned web
            Destroy(currentWeb);
            currentWeb = null;
        }
        
    }

    public float throwStrength = 25f;
    private void Throw( float strengthMultiplier = 1f){
        if(currentHeld != null){
            //This throws from the player towards the intersection along the camera ray
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Cast ray from camera, can also use cam.transform.forward for a different direction
            RaycastHit hit;
            //Ray cast to see if we are looking at something and then construct a ray from the player to that
            if (Physics.Raycast(ray, out hit, 10 * throwStrength, ~LayerMask.NameToLayer("Player"))){
                ray.direction = (hit.point - transform.position).normalized;
                ray.origin = transform.position;
            } else{ //Change the ray to point at the maximum launch distance
                ray.direction = (ray.GetPoint(10 * throwStrength) - transform.position).normalized;
                ray.origin = transform.position;
            }
            currentHeld.GetComponent<Throwable>().Thrown(ray.direction * throwStrength * strengthMultiplier);
            currentHeld = null;
        }
    }
    //We pass the web through so we dont get overhead from creating and deleting a new one
    //Unlike the grappling hook, the pickup function pulls at a fixed rate by altering the transform instead of the rigidbody
    //I might need to disable the items rigidbody while this is happening
    //Lerp the web length by the percentage and move the item to the tip of the web
    public int PickupSpeed = 5;
    public Transform PlayerHand;
    private IEnumerator PickUp(GameObject item, GameObject web){
        var wait = new WaitForFixedUpdate();
        Throwable throwable = item.GetComponent<Throwable>();
        Weapon weapon = item.GetComponent<Weapon>();
        Rigidbody rb = item.GetComponent<Rigidbody>();

        //Initialize stuff for shrinking the web
        Vector3 localTarget = Vector3.zero;
        Transform targetTransform = transform;

        //Weapon or throwable unique init
        if(rb != null){
            rb.isKinematic = true;
        }
        if(weapon != null){
            targetTransform = PlayerHand;
        }else if(throwable != null){
            localTarget = new Vector3(1.1f, 0.2f, 1.1f);
        }
        int counter = 0;
        while(true){
            Vector3 difference = targetTransform.TransformPoint(localTarget) - item.transform.position;
            float distance = difference.magnitude;
            if(distance < PickupSpeed){
                Destroy(web);
                break;
            } else{
                //Moves the item in the right direction
                item.transform.position += difference.normalized * PickupSpeed;
                web.transform.LookAt(item.transform.position);
                web.transform.localScale = new Vector3(0,0, distance - PickupSpeed);
            }
            yield return wait;
            //Just in case, dont want to get stuck with a web on our screen, delete both the web and item and give up
            counter++;
            if(counter > 9999){
                Debug.LogWarning("Couldn't pick up: " + item.name);
                Destroy(web);
                Destroy(item);
                yield break;
            }
        }
        //Weapon or thrwable unique after the fact
        if(rb != null){
            rb.isKinematic = false;
        }
        if(weapon != null){
            //Implement a system for detecting if there is a current weapon and dropping it
            if (currentWeapon != null)
            {
                Rigidbody currentWeaponRb = currentWeapon.GetComponent<Rigidbody>();
                currentWeapon.transform.SetParent(null);
                currentWeaponRb.isKinematic = false;
                Weapon wp = currentWeapon.GetComponent<Weapon>();
                wp.StopAllCoroutines();
                wp.ToggleActiveSword(false);
                currentWeapon.StopAllCoroutines();//In case it is swinging, we dont want it to stop and diable its colliders
                foreach (var collider in currentWeaponRb.GetComponentsInChildren<Collider>())
                {
                    collider.enabled = true;
                }
            }
            //Disable the weapon's colliders so that way it doesnt collide with the ground while walking
            currentWeapon = weapon;
            foreach (var collider in rb.GetComponentsInChildren<Collider>()){
                collider.enabled = false;
            }
            //Relocate to the player
            rb.isKinematic = true;
            weapon.transform.SetParent(targetTransform, false);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
        }else if(throwable != null){
            currentHeld = throwable;
            currentHeld.PickedUp(heldTransform, heldOffset);
        }
        yield return null;
    }
    public GameObject heldTransform;
    public Vector3 heldOffset = new Vector3(0.3f, 0.1f, 0.2f);
}
