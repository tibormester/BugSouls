using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class Stamina : MonoBehaviour
{
    public CustomPhysicsBody physicsBody;
    public StaminaBar stamina;
    public CharacterMovement player;
    public float maxStamina = 50f;
    public float currStamina;
    // Start is called before the first frame update
    void Start()
    {
        physicsBody = GetComponent<CustomPhysicsBody>();
        stamina.setMaxStamina(maxStamina);
        currStamina = maxStamina;
        stamina.setStamina(maxStamina);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.LeftShift) && (Input.GetKey("w") || Input.GetKey("a") || Input.GetKey("d") || Input.GetKey("s")))
        {
            currStamina -= 5f * Time.deltaTime;
            if(currStamina <= 0) 
            {
                currStamina = 0;
                player.sprintSpeed = player.moveSpeed;
                
            }
            else{
                player.sprintSpeed = 14f;
                stamina.setStamina(currStamina);
                if(Input.GetKeyDown(KeyCode.Space) && physicsBody.IsGrounded())
                {
                    currStamina -= 5f;
                    if(currStamina < 0) currStamina = 0;
                    stamina.setStamina(currStamina);
                }
            }
            
        }
        else if(currStamina >= 5f && Input.GetButtonDown("Jump") && physicsBody.IsGrounded())
        {
            currStamina -= 5f;
            if(currStamina <= 0) currStamina = 0;
            stamina.setStamina(currStamina);
        }
        
        if(currStamina <= maxStamina)
        {
            currStamina += 2.5f * Time.deltaTime;
            stamina.setStamina(currStamina);
        }
            
        

    }
}
