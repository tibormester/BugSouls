using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MysteriousMushroomScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Health health;
    public MushroomGuyScript ai;
    public TextMeshPro text;
    public CharacterMovement charMovement;
    public string[] dialogue = new string[] {
        "Ah, the spider ant stirs. Foolish, or brave? Perhaps both.",
        "The vine whispers promises. I heard them... but I shut my ears. Did you?",
        "Keep distance, the tree's roots. There, the truth festers. But beware, it does not let go easily.",
        "Remember, to sever the vine, you must strike at its heart. Not all roots can be pulled",
    };
    void Start(){
        text = GetComponent<TextMeshPro>();
        charMovement = transform.parent.GetComponent<CharacterMovement>();

        //Setup the spider to stop talking and attack if damaged
        health = transform.parent.GetComponent<Health>();
        ai = transform.parent.GetComponent<MushroomGuyScript>();
        ai.enabled = false;

        Coroutine corountine = StartCoroutine(ReadDialogue());

        health.Damaged += () => {
            ai.enabled = true;
            StopCoroutine(corountine);
            text.text = "";
        };
        health.DeathEvent += () => Destroy(this);
        
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
        yield return new WaitForSeconds(10f);
        foreach(string line in new string[]{"What are you still doing here?", "Don't you have places to be?",
                "Did you not hear me?", "You're ruining my rest, leave!", "Leave, I won't ask again!"}){
            foreach(char letter in line){
                text.text += letter;
                yield return charWait;
            }
            yield return new WaitForSeconds(10f);
            text.text = "";
        }
        ai.enabled = true;
    }




}
