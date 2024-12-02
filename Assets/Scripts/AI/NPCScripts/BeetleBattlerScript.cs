using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BeetleBattlerScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Health health;
    public GeneralAI ai;
    public TextMeshPro text;
    public CharacterMovement charMovement;
    public string[] dialogue = new string[] {
        "Finally, someone who hasn't fallen to that wretched plant.",
        "The vine took everything—my brood, my honor. It won't take my rage.",
        "Fight me, ant. Will you give me a warriors end?",
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
            StopCoroutine(corountine);
            text.text = "";
        };
        health.DeathEvent += () => {
            Instantiate(beetleSwordPrefab, transform.position + Vector3.up, transform.rotation);
            Destroy(this);
        };
        
    }
    public GameObject beetleSwordPrefab;
    private void Update(){
        text.transform.LookAt(ai.target);
    }
    public IEnumerator ReadDialogue(){
        //time after each character
        var charWait = new WaitForSeconds(0.01f);
        //how to get to the next line
        var lineWait = new WaitForSeconds(1.5f);
        foreach(string line in dialogue){
            foreach(char letter in line){
                text.text += letter;
                if(ai.target)charMovement.look_direction = ai.target.position - transform.parent.position;
                yield return charWait;
            }
            yield return lineWait;
            text.text = "";
        }
        ai.processing = true;
    }




}
