using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SicklySpiderScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Health health;
    public MeleeSpiderlingAI ai;
    public TextMeshPro text;
    public CharacterMovement charMovement;
    public string[] dialogue = new string[] {};
    void Start(){
        text = GetComponent<TextMeshPro>();
        charMovement = transform.parent.GetComponent<CharacterMovement>();

        //Setup the spider to stop talking and attack if damaged
        health = transform.parent.GetComponent<Health>();
        ai = transform.parent.GetComponent<MeleeSpiderlingAI>();
        ai.enabled = false;

        Coroutine corountine = StartCoroutine(ReadDialogue());

        health.Damaged += () => {
            ai.enabled = true;
            StopCoroutine(corountine);
            text.text = "";
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
        yield return new WaitForSeconds(10f);
        foreach(string line in new string[]{"Get out of here I can't hold off the corruption much longer!",
                "Matriarch forgive me.", "Aghh!"}){
            foreach(char letter in line){
                text.text += letter;
                yield return charWait;
            }
            yield return new WaitForSeconds(4f);
            text.text = "";
        }
        ai.enabled = true;
    }




}
