using System.Collections;
using TMPro;
using UnityEngine;

public class MatriarchDialogueScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Health health;
    public GeneralAI ai;
    public TextMeshPro text;
    public CharacterMovement charMovement;
    public string[] dialogue = new string[] {
        "The corruption... it twists, it consumes. I fought to protect... ",
        "I only delayed the inevitable.",
    };
    void Start(){
        text = GetComponent<TextMeshPro>();
        charMovement = transform.parent.GetComponent<CharacterMovement>();

        //Setup the spider to stop talking and attack if damaged
        health = transform.parent.GetComponent<Health>();
        ai = transform.parent.GetComponent<GeneralAI>();

        health.DeathEvent += () => {
            StartCoroutine(ReadDialogue());
        };
        
    }
    private void Update(){
        text.transform.LookAt(ai.target);
    }
    public IEnumerator ReadDialogue(){
        //time after each character
        var charWait = new WaitForSeconds(0.01f);
        //how to get to the next line
        var lineWait = new WaitUntil( () => Input.GetKeyDown(KeyCode.Tab)); 
        foreach(string line in dialogue){
            foreach(char letter in line){
                text.text += letter;
                charMovement.look_direction = ai.target.position - transform.parent.position;
                yield return charWait;
            }
            yield return lineWait;
            text.text = "";
        }

    }




}
