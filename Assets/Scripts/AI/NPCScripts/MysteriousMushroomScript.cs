using System.Collections;
using TMPro;
using UnityEngine;

public class MysteriousMushroomScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Health health;
    public GeneralAI ai;
    public TextMeshPro text;
    public CharacterMovement charMovement;
    private string[] dialogue = new string[] {
        "Ah, the spider ant stirs. Foolish, or brave? Perhaps both.\n(Press TAB to talk)",
        "The vine whispers promises. I heard them... but I shut my ears. Did you?",
        "Keep distance, the tree's roots. There, the truth festers. But beware, it does not let go easily.",
        "Remember, to sever the vine, you must strike at its heart. Not all roots can be pulled",
    };
    void Start(){
        text = GetComponent<TextMeshPro>();
        charMovement = transform.parent.GetComponent<CharacterMovement>();

        //Setup the spider to stop talking and attack if damaged
        health = transform.parent.GetComponent<Health>();
        ai = transform.parent.GetComponent<GeneralAI>();

        ai.processing = false;

        Coroutine corountine = StartCoroutine(ReadDialogue());

        health.Damaged += () => {
            ai.processing = true;
            text.text = "";
            Destroy(this);
        };
        
    }
    private void Update(){
        text.transform.LookAt(ai.target);
        ai.movement.look_direction = ai.target.position - ai.transform.position;
    }
    public IEnumerator ReadDialogue(){
        //time after each character
        var charWait = new WaitForSeconds(0.01f);
        //how to get to the next line
        var lineWait = new WaitUntil( () => Input.GetKeyDown(KeyCode.Tab)); 
        foreach(string line in dialogue){
            foreach(char letter in line){
                text.text += letter;
                if(ai.target)charMovement.look_direction = ai.target.position - transform.parent.position;
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
                if(ai.target)charMovement.look_direction = ai.target.position - transform.parent.position;
                yield return charWait;
            }
            yield return new WaitForSeconds(10f);
            text.text = "";
        }
        ai.processing = true;
    }




}
